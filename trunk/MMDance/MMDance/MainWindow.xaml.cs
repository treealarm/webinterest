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

        void StructureToByteArray(object obj, byte[] bytearray, int position)
        {
            int len = Marshal.SizeOf(obj);

            IntPtr ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, true);

            Marshal.Copy(ptr, bytearray, position, len);

            Marshal.FreeHGlobal(ptr);
        }

        void ByteArrayToStructure<T>(byte[] bytearray, ref T structureObj, int position) where T : class
        {
            int length = Marshal.SizeOf(structureObj);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            Marshal.Copy(bytearray, position, ptr, length);
            structureObj = (T)Marshal.PtrToStructure(ptr, structureObj.GetType());
            Marshal.FreeHGlobal(ptr);
        }

        public MainWindow()
        {
            InitializeComponent();
            m_step_mult.m_uMult[0] = 1;
            m_step_mult.m_uMult[1] = 2;
            m_step_mult.m_uMult[2] = 3;

            byte[] mybytes = new byte[65];

            StructureToByteArray(m_step_mult, mybytes, 2);

            do_steps_multiplier new_step_mult = new do_steps_multiplier();
            ByteArrayToStructure(mybytes, ref new_step_mult, 2);
            int i = 9;
        }


        private void canvas1_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private delegate void UpdateCurrentPositionDelegate(double x1, double y1);
        private void UpdateCurrentPosition(double x1, double y1)
        {
            x_line.X1 = x1;
            x_line.X2 = x1;
            x_line.Y1 = 0;
            x_line.Y2 = image_canvas.ActualHeight;

            y_line.Y1 = y1;
            y_line.Y2 = y1;
            y_line.X1 = 0;
            y_line.X2 = image_canvas.ActualWidth;
        }
        private void image_canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.GetPosition(image_canvas).Y < menu1.Height)
            {
                menu1.Visibility = Visibility.Visible;
            }
            else
            {
                menu1.Visibility = Visibility.Collapsed;
            }
            //UpdateCurrentPosition(e.GetPosition(image_canvas).X, e.GetPosition(image_canvas).Y);
        }

        BitmapImage bitmapImage = null;
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Открыть изображение";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                bitmapImage = new BitmapImage(new Uri(op.FileName));
                loaded_image.Source = bitmapImage;
            }
        }

        Thread WorkingThread = null;
        bool StopThread = false;

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

            double xratio = bitmapImage.Width / image_canvas.ActualWidth;
            double yratio = bitmapImage.Height / image_canvas.ActualHeight;
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
                                    new UpdateCurrentPositionDelegate(UpdateCurrentPosition),
                                    x * xratio, y * yratio);
                }
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
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
