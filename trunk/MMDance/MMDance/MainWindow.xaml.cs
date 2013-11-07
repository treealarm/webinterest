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

namespace MMDance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Ellipse myEllipse = new Ellipse();
        Thickness ell_pos = new Thickness(55, 55, 0, 0);

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
