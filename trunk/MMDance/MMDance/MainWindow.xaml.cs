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


namespace MMDance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal struct xyz_coord
        {
            int x;
            int y;
            int z;
        }

        [StructLayout(LayoutKind.Sequential, Size = 4 * ControlWrapper.MOTORS_COUNT), Serializable]
        internal class do_steps_multiplier
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = ControlWrapper.MOTORS_COUNT)]
            public Int32[] m_uMult = new Int32[ControlWrapper.MOTORS_COUNT];
        }

        do_steps_multiplier m_step_mult = new do_steps_multiplier();
        //ControlWrapper m_ControlWrapper = new ControlWrapper();


        public MainWindow()
        {
            InitializeComponent();
            UInt16 m_timer_ink_impuls = 222;

            byte[] mybytes = new byte[65];

            ControlWrapper.StructureToByteArray(m_timer_ink_impuls, mybytes, 2);

            UInt16 m_timer_ink_impul1s = 0;

            ControlWrapper.ByteArrayToStructure(mybytes, ref m_timer_ink_impul1s, 2);
            int i = 9;
        }


        BitmapImage bitmapImage = null;
        public void OnFileOpen(string filename)
        {
            bitmapImage = new BitmapImage(new Uri(filename));
            PictureUserControl.loaded_image.Source = bitmapImage;
        }

        Thread WorkingThread = null;
        bool StopThread = false;
        private delegate void UpdateCurrentPositionDelegate(double x1, double y1);

        private void DoEngraving()
        {
            if (bitmapImage == null)
            {
                return;
            }
            bitmapImage.VerifyAccess();
            int stride = bitmapImage.PixelWidth * 4;
            int size = bitmapImage.PixelHeight * stride;
            byte[] pixels = new byte[size];

            double xratio = bitmapImage.Width / PictureUserControl.image_canvas.ActualWidth;
            double yratio = bitmapImage.Height / PictureUserControl.image_canvas.ActualHeight;
            for (int x = 0; x < bitmapImage.Width; x++)
            {
                for (int y = 0; y < bitmapImage.Height; y++)
                {
                    if (StopThread)
                    {
                        return;
                    }
                    bitmapImage.CopyPixels(pixels, stride, 0);

                    
                    int index = y * stride + 4 * x;
                    byte red = pixels[index];
                    byte green = pixels[index + 1];
                    byte blue = pixels[index + 2];
                    byte alpha = pixels[index + 3];
                    Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Normal,
                                    new UpdateCurrentPositionDelegate(PictureUserControl.UpdateCurrentPosition),
                                    x * xratio, y * yratio);
                }
            }
        }

        public void Start()
        {
            bitmapImage.Freeze();
            WorkingThread = new Thread(new ThreadStart(DoEngraving));
            WorkingThread.SetApartmentState(ApartmentState.STA);
            WorkingThread.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopThread = true;
            if (WorkingThread != null)
            {
                WorkingThread.Join();
            }
        }
    }
}
