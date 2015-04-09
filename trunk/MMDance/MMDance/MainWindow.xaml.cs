﻿using System;
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
using System.Windows.Media.Media3D;


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
            
            WorkingThread = new Thread(new ThreadStart(ProcessCommand));
            WorkingThread.SetApartmentState(ApartmentState.STA);
            WorkingThread.Start();
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
            public int x = 0;//между бабок
            public int y = 0;//ход шпинделя
            public int b = 0;//угол
            public int w = 0;
            public int dir = 1;
        }

        [StructLayout(LayoutKind.Sequential, Size = 1 * ControlWrapper.MOTORS_COUNT), Serializable]
        public class cruise_motors
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = ControlWrapper.MOTORS_COUNT)]
            public byte[] m_is_cruiser = new byte[ControlWrapper.MOTORS_COUNT];
            [MarshalAs(UnmanagedType.I1)]
            public byte m_signal_on_zero;
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
                                    //steps max x8 x16
        public const int X_POS = 2; //2225 //1150 - x16
        public const int Y_POS = 1; //1900 //950  -x16
        public const int B_POS = 3; //12000 
        public const int W_POS = 0; //12000 
        //~0.38 мм на шаг x16 0.76 mm

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

        do_timer_set m_curtimerset = new do_timer_set();

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

        public void SetCruisersToController()
        {
            cruise_motors cm = new cruise_motors();
            cm.m_signal_on_zero = 0;
            for (int motor = 0; motor < ControlWrapper.MOTORS_COUNT; motor++)
            {
                cm.m_is_cruiser[motor] = Convert.ToByte(motor == X_POS || motor == Y_POS);
            }
            byte[] OutputPacketBuffer = new byte[ControlWrapper.LEN_OF_PACKET];	//Allocate a memory buffer equal to our endpoint size + 1
            OutputPacketBuffer[0] = 0;				//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.
            OutputPacketBuffer[1] = ControlWrapper.COMMAND_SET_CRUISERS;
            ControlWrapper.StructureToByteArray(cm, OutputPacketBuffer, 2);
            AddCommand(OutputPacketBuffer);
        }
        public void SetStepsToController(do_steps steps, bool update_pos = true)
        {
	        if(
		        steps.m_uSteps[X_POS] == 0 &&
		        steps.m_uSteps[Y_POS] == 0 &&
                steps.m_uSteps[B_POS] == 0 &&
		        steps.m_uSteps[W_POS] == 0)
	        {
		        return;
	        }
            if (update_pos)
            {
                m_cur_pos.y += steps.m_uSteps[Y_POS];
                m_cur_pos.x += steps.m_uSteps[X_POS];
                m_cur_pos.b += steps.m_uSteps[B_POS];
                m_cur_pos.w += steps.m_uSteps[W_POS];
            }

	       	for(int motor = 0; motor < ControlWrapper.MOTORS_COUNT; motor++)
	        {
		        steps.m_uSteps[motor] = steps.m_uSteps[motor]*m_step_mult.m_uMult[motor];
	        }
	        byte[] OutputPacketBuffer = new byte[ControlWrapper.LEN_OF_PACKET];	//Allocate a memory buffer equal to our endpoint size + 1
	        OutputPacketBuffer[0] = 0;				//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.
	        OutputPacketBuffer[1] = ControlWrapper.COMMAND_SET_STEPS;
            ControlWrapper.StructureToByteArray(steps, OutputPacketBuffer, 2);
	        AddCommand(OutputPacketBuffer);
        }

        int m_nTimerCounter = 0;
        public void GoToXY(int x, int y, int b = -1, int w = -1)
        {
            MainWindow.do_steps var_do_steps = new MainWindow.do_steps();
            
            var_do_steps.m_uSteps[X_POS] = x - m_cur_pos.x;
            var_do_steps.m_uSteps[Y_POS] = y - m_cur_pos.y;

            if (Math.Abs(var_do_steps.m_uSteps[X_POS]) > 10 || Math.Abs(var_do_steps.m_uSteps[Y_POS]) > 10)
            {
                ControlUserControl.SetTimerSetting(1);
                m_nTimerCounter = 0;
            }
            m_nTimerCounter++;
            if (m_nTimerCounter > 2)
            {
                ControlUserControl.SetTimerSetting(0);
                m_nTimerCounter = 0;
            }
            
            
            if (b >= 0)
            {
                var_do_steps.m_uSteps[B_POS] = b - m_cur_pos.b;
            }
            if (w >= 0)
            {
                var_do_steps.m_uSteps[W_POS] = w - m_cur_pos.w;
            }
            SetStepsToController(var_do_steps);
        }

        public void GoToBW(int b, int w)
        {
            MainWindow.do_steps var_do_steps = new MainWindow.do_steps();
            var_do_steps.m_uSteps[B_POS] = b - m_cur_pos.b;
            var_do_steps.m_uSteps[W_POS] = w - m_cur_pos.w;
            SetStepsToController(var_do_steps);
        }

        int m_counter = 0;
        

        public void Start()
        {
            m_counter = 0;

            PictureUserControl.UpdateProfileResult();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1);
            dispatcherTimer.Start(); 
        }

        private bool DoEngraving(ref xyz_coord cur_coords)
        {
            Point3D intersection;
            double angle = GetAngleFromStep(cur_coords.b);
            PictureUserControl.m_UserControlFor3D.GetIntersection(angle, cur_coords.x, out intersection);
            PictureUserControl.m_UserControlFor3D.UpdatePosition(intersection, angle);
            cur_coords.y = (int)intersection.Y;
            return true;
        }

        xyz_coord m_CurTask = new xyz_coord();
        double GetAngleFromStep(int step)
        {
            double angle = step;
            angle *= 1.8;
            return angle;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (m_bPauseSoft)
            {
                return;
            }
            //if (GetQueueLen() < 10)
            {
                if (DoEngraving(ref m_CurTask))
                {
                    GoToXY(m_CurTask.x, m_CurTask.y, m_CurTask.b);
                    m_CurTask.b += m_CurTask.dir;
                    double angle = GetAngleFromStep(m_CurTask.b);
                    if (angle > 360 || angle < 0)
                    {
                        m_CurTask.dir = -m_CurTask.dir;
                        m_CurTask.x++;
                    }
                    m_counter++;
                    ControlUserControl.textBlockCounter.Text = m_counter.ToString();
                }
                else
                {
                    SetInk(true);
                    Thread.Sleep(1000);
                    GoToXY(0, 0);
                    //GoToBW(0, 0);
                    m_CurTask = new xyz_coord();
                    ControlUserControl.checkBoxPauseSoft.IsChecked = true;
                    dispatcherTimer.Stop();
                    //ControlUserControl.checkBoxOutpusEnergy.IsChecked = false;
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
