using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Threading;

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;


namespace MMDance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public ControlWrapper m_ControlWrapper = new ControlWrapper();

        public MainWindow()
        {
            InitializeComponent();
            for (int motor = 0; motor < ControlWrapper.MOTORS_COUNT; motor++)
            {
                m_step_mult.m_uMult[motor] = 1;
            }
            
            //UInt16 m_timer_ink_impuls = 222;

            //byte[] mybytes = new byte[65];

            //ControlWrapper.StructureToByteArray(m_timer_ink_impuls, mybytes, 2);

            //UInt16 m_timer_ink_impul1s = 0;

            //ControlWrapper.ByteArrayToStructure(mybytes, ref m_timer_ink_impul1s, 2);

            OnFileOpen(Properties.Settings.Default.PicFile);
            WorkingThread = new Thread(new ThreadStart(ProcessCommand));
            WorkingThread.SetApartmentState(ApartmentState.STA);
            WorkingThread.Start();
        }

        public ObservableCollection<PictureColors> m_colors = new ObservableCollection<PictureColors>();
        BitmapImage bitmapImage = null;
        public bool OnFileOpen(string filename)
        {
            try
            {
                bitmapImage = new BitmapImage(new Uri(filename));
                PictureUserControl.loaded_image.Source = bitmapImage;
                m_colors.Clear();

               Dictionary<Color, int> ColorMap = new Dictionary<Color,int>();

                int stride = bitmapImage.PixelWidth * 4;
                int size = bitmapImage.PixelHeight * stride;
                byte[] pixels = new byte[size];

                bitmapImage.CopyPixels(pixels, stride, 0);
                for (int y = 0; y < bitmapImage.PixelHeight; y++)
                {
                    for (int x = 0; x < bitmapImage.PixelWidth; x++)
                    {
                        int index = y * stride + 4 * x;
                        byte red = pixels[index];
                        byte green = pixels[index + 1];
                        byte blue = pixels[index + 2];
                        byte alpha = pixels[index + 3];
                        Color cur_col = Color.FromRgb(red, green, blue);
                        int Count = 0;
                        ColorMap.TryGetValue(cur_col, out Count);
                        Count++;

                        ColorMap[cur_col] = Count;
                    }
                }
                foreach(KeyValuePair<Color,int> pair in ColorMap)
                {
                    Color color = pair.Key;
                    m_colors.Add(new PictureColors(color, pair.Value));
                }
            }
            catch (System.Exception ex)
            {
                return false;
            }

            

            ControlUserControl.listViewColors.DataContext = m_colors;
            return true;
        }

        Thread WorkingThread = null;
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        bool StopThread = false;
        private delegate void UpdateCurrentPositionDelegate(double x1, double y1);

        private Queue<Byte[]> m_out_list = new Queue<Byte[]>();

        public int GetQueueLen()
        {
            lock (m_out_list)
            {
                return m_out_list.Count;
            }
        }
        public void AddCommand(Byte[] command)
        {
            
            lock (m_out_list)
            {
                m_out_list.Enqueue(command);
            }
        }

        private void ProcessCommand()
        {
            while (!StopThread)
            {
                int SleepVal = 10;
                if (m_ControlWrapper.IsControllerAvailable())
                {
                    lock (m_out_list)
                    {
                        if (m_out_list.Count <= 0)
                        {
                            SleepVal = 100;

                            ControlUserControl.textBlockInfo.Dispatcher.BeginInvoke(new Action(delegate()
                            {
                                ControlUserControl.textBlockInfo.Text = m_ControlWrapper.GetCurText(); //where item is the item to be added and listbox is the control being updated.
                            }));
            
                        }
                        else
                        {
                            Byte[] buf = m_out_list.Dequeue();
                            m_ControlWrapper.WriteCommandToController(buf);
                        }
                    }
                }
                
                Thread.Sleep(SleepVal);
            }
        }

        internal class xyz_coord
        {
            public int x = 0;
            public int y = 0;
            public int b = 0;
            public int w = 0;
            public bool end_of_stride = true;
        }

        [StructLayout(LayoutKind.Sequential, Size = 4 * ControlWrapper.MOTORS_COUNT), Serializable]
        public class do_steps
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = ControlWrapper.MOTORS_COUNT)]
            public Int32[] m_uSteps = new Int32[ControlWrapper.MOTORS_COUNT];
        }

        [StructLayout(LayoutKind.Sequential, Size = 4 * ControlWrapper.MOTORS_COUNT), Serializable]
        public class do_steps_multiplier
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = ControlWrapper.MOTORS_COUNT)]
            public Int32[] m_uMult = new Int32[ControlWrapper.MOTORS_COUNT];
        }

        [StructLayout(LayoutKind.Sequential, Size = 4 + ControlWrapper.MOTORS_COUNT), Serializable]
        internal class do_timer_set
        {
            [MarshalAs(UnmanagedType.U2)]
            public UInt16 m_timer_res;
            [MarshalAs(UnmanagedType.U2)]
            public UInt16 m_strike_impuls;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = ControlWrapper.MOTORS_COUNT)]
            public byte[] m_multiplier = new byte[ControlWrapper.MOTORS_COUNT];
        }

        [StructLayout(LayoutKind.Sequential, Size = 4), Serializable]
        internal class  do_control_signals
        {
            [MarshalAs(UnmanagedType.U1)]
            public byte enable;
        }
        
        public do_steps_multiplier m_step_mult = new do_steps_multiplier();
        xyz_coord m_cur_pos = new xyz_coord();
        xyz_coord m_cur_task = new xyz_coord();//Последняя отправленная на обработку координата
                                    //steps max x8
        public const int X_POS = 2; //2200
        public const int Y_POS = 1; //1900  ~0.38 мм на шаг
        public const int B_POS = 3; //12000 
        public const int W_POS = 0; //12000 

        public void SetControlSettings(bool enable)
        {
            do_control_signals control_sign = new do_control_signals();
            control_sign.enable = Convert.ToByte(enable);
            
            byte[] OutputPacketBuffer = new byte[ControlWrapper.LEN_OF_PACKET];
            OutputPacketBuffer[0] = 0;
            OutputPacketBuffer[1] = ControlWrapper.COMMAND_SET_CONTROL_SIGNALS;
            ControlWrapper.StructureToByteArray(control_sign, OutputPacketBuffer, 2);
            AddCommand(OutputPacketBuffer);
        }

        public void SetTimerSettings(UInt16 timer_res, UInt16 strike_impuls, byte[] multiplier)
        {
            do_timer_set timerset = new do_timer_set();
            timerset.m_timer_res = (UInt16)(UInt16.MaxValue - timer_res);
            timerset.m_strike_impuls = strike_impuls;

            for (int i = 0; i < ControlWrapper.MOTORS_COUNT && i < multiplier.Length; i++)
            {
                timerset.m_multiplier[i] = multiplier[i];
            }
            
            byte[] OutputPacketBuffer = new byte[ControlWrapper.LEN_OF_PACKET];
            OutputPacketBuffer[0] = 0;
            OutputPacketBuffer[1] = ControlWrapper.COMMAND_SET_TIME;
            ControlWrapper.StructureToByteArray(timerset, OutputPacketBuffer, 2);
            AddCommand(OutputPacketBuffer);
        }
        bool m_bPauseSoft = false;
        public void SetPauseSoft(bool pause)
        {
            m_bPauseSoft = pause;
        }
        public void SetPause(bool pause)
        {
            byte[] OutputPacketBuffer = new byte[ControlWrapper.LEN_OF_PACKET];
            OutputPacketBuffer[0] = 0;
            OutputPacketBuffer[1] = ControlWrapper.COMMAND_SET_PAUSE;
            OutputPacketBuffer[2] = Convert.ToByte(pause);
            AddCommand(OutputPacketBuffer);
        }
        
        public void SetInk(bool ink)
        {
            byte[] OutputPacketBuffer = new byte[ControlWrapper.LEN_OF_PACKET];
            OutputPacketBuffer[0] = 0;
            OutputPacketBuffer[1] = ControlWrapper.COMMAND_SET_INK;
            OutputPacketBuffer[2] = Convert.ToByte(ink);
            AddCommand(OutputPacketBuffer);
        }

        public void UpdateCurrentPosition()
        {
            if(bitmapImage == null)
            {
                return;
            }
            try
            {
                double xratio = PictureUserControl.image_canvas.ActualWidth / bitmapImage.PixelWidth;
                double yratio = PictureUserControl.image_canvas.ActualHeight / bitmapImage.PixelHeight;
                PictureUserControl.UpdateCurrentPosition(m_cur_pos.x * xratio, m_cur_pos.y * yratio);
            }
            catch (Exception e)
            {
            }
        }
        
        public void SetStepsToController(do_steps steps)
        {
	        if(
		        steps.m_uSteps[X_POS] == 0 &&
		        steps.m_uSteps[Y_POS] == 0 &&
                steps.m_uSteps[B_POS] == 0 &&
		        steps.m_uSteps[W_POS] == 0)
	        {
		        return;
	        }
	        m_cur_pos.y += steps.m_uSteps[Y_POS];
	        m_cur_pos.x += steps.m_uSteps[X_POS];
            m_cur_pos.b += steps.m_uSteps[B_POS];
	        m_cur_pos.w += steps.m_uSteps[W_POS];

	       	for(int motor = 0; motor < ControlWrapper.MOTORS_COUNT; motor++)
	        {
		        steps.m_uSteps[motor] = steps.m_uSteps[motor]*m_step_mult.m_uMult[motor];
	        }
	        byte[] OutputPacketBuffer = new byte[ControlWrapper.LEN_OF_PACKET];	//Allocate a memory buffer equal to our endpoint size + 1
	        OutputPacketBuffer[0] = 0;				//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.
	        OutputPacketBuffer[1] = ControlWrapper.COMMAND_SET_STEPS;
            ControlWrapper.StructureToByteArray(steps, OutputPacketBuffer, 2);
	        AddCommand(OutputPacketBuffer);

            UpdateCurrentPosition();
        }

        public void GoToXY(int x, int y)
        {
            MainWindow.do_steps var_do_steps = new MainWindow.do_steps();
            var_do_steps.m_uSteps[X_POS] = x - m_cur_pos.x;
            var_do_steps.m_uSteps[Y_POS] = y - m_cur_pos.y;
            SetStepsToController(var_do_steps);
        }

        public void GoToBW(int b, int w)
        {
            MainWindow.do_steps var_do_steps = new MainWindow.do_steps();
            var_do_steps.m_uSteps[B_POS] = b - m_cur_pos.b;
            var_do_steps.m_uSteps[W_POS] = w - m_cur_pos.w;
            SetStepsToController(var_do_steps);
        }

        private bool DoEngraving(ref xyz_coord cur_coords)
        {
            if (bitmapImage == null)
            {
                return false;
            }
            bitmapImage.VerifyAccess();

            int stride = bitmapImage.PixelWidth * 4;
            int size = bitmapImage.PixelHeight * stride;
            byte[] pixels = new byte[size];

            int start_x = cur_coords.x;
            int start_y = cur_coords.y+1;
            cur_coords.end_of_stride = false;
            if (start_y >= bitmapImage.PixelHeight)
            {
                start_y = 0;
                start_x++;
                cur_coords.end_of_stride = true;
            }

            bitmapImage.CopyPixels(pixels, stride, 0);
            for (int x = start_x; x < bitmapImage.PixelWidth; x++)
            {
                for (int y = start_y; y < bitmapImage.PixelHeight; y++)
                {
                    int index = y * stride + 4 * x;
                    byte red = pixels[index];
                    byte green = pixels[index + 1];
                    byte blue = pixels[index + 2];
                    byte alpha = pixels[index + 3];
                    //if (red != 0 || green!=0 || blue!=0)
                    {
                        int grayScale = (int)((red * 0.3) + (green * 0.59) + (blue * 0.11));

                        int black = grayScale * Properties.Settings.Default.BlackColorMax / 255;
                        int white = grayScale * Properties.Settings.Default.WhiteColorMax / 255;
                        cur_coords.b += black;
                        cur_coords.w += white;
                        cur_coords.x = x;
                        cur_coords.y = y;
                        return true;
                    }
                }
            }
            return false;
        }

        public void Start()
        {
            if (bitmapImage != null)
            {
                bitmapImage.Freeze();
            }
            
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1);
            dispatcherTimer.Start(); 
        }

        xyz_coord m_CurTask = new xyz_coord();
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (m_bPauseSoft)
            {
                return;
            }
            if (GetQueueLen() < 10)
            {
                if (DoEngraving(ref m_CurTask))
                {
                    SetInk(m_CurTask.end_of_stride);
                    if (m_CurTask.end_of_stride)
                    {
                        Thread.Sleep(1000);
                    }
                    GoToXY(m_CurTask.x, m_CurTask.y);
                    GoToBW(m_CurTask.b, m_CurTask.w);
                }
                else
                {
                    SetInk(true);
                    Thread.Sleep(1000);
                    GoToXY(0, 0);
                    GoToBW(0, 0);
                }
            }
        } 
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Properties.Settings.Default.TimerRes = "50";
            Properties.Settings.Default.Save();
            StopThread = true;
            if (WorkingThread != null)
            {
                WorkingThread.Join();
            }
        }
        static public MainWindow GetMainWnd()
        {
            return (MainWindow)System.Windows.Application.Current.Windows[0];
        }
    }
}
