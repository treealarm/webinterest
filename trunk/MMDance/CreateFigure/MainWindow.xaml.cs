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
using MMDance;
using System.IO;

namespace CreateFigure
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

        private void buttonCreateCircle_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog op1 = new SaveFileDialog();
            op1.Filter = "xml|*.xml;";
            if (op1.ShowDialog() == true)
            {
                List<Point> list = new List<Point>();
                double R = Convert.ToDouble(textBoxDiameter.Text)/2;
                int nPoints = Convert.ToInt32(textBoxPoints.Text);
                AddCircle(list, new Point(0, 0), R, nPoints);
                ListPoint.SerializeObject(list, op1.FileName);
            }
        }
        void AddCircle(List<Point> list, Point center, double R, int nPoints)
        {
            double fAngleDelta = ConvertToRadians(360 / nPoints);
            for (double angle = 0; angle < Math.PI * 2; angle += fAngleDelta)
            {
                double y = R * Math.Sin(angle) + center.Y;
                double x = R * Math.Cos(angle) + center.X;
                list.Add(new Point(x, y));
            }
        }

        public double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public static BitmapImage GetImage(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }
            BitmapImage image = new BitmapImage();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.BeginInit();
            image.UriSource = new Uri(fileName, UriKind.RelativeOrAbsolute);
            image.EndInit();
            return image;
        }

        public void GetCrossSectionProfile(List<Point> list, string fileName, int TotalPoints, double fPointWidth)
        {
            BitmapImage myBitmapImage = GetImage(fileName);
            if (myBitmapImage == null)
            {
                return;
            }

            FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
            newFormatedBitmapSource.BeginInit();
            newFormatedBitmapSource.Source = myBitmapImage;
            newFormatedBitmapSource.DestinationFormat = PixelFormats.Rgb24;
            newFormatedBitmapSource.EndInit();

            int width = newFormatedBitmapSource.PixelWidth;

            int stride = width * 3;
            int size = newFormatedBitmapSource.PixelHeight * stride;
            byte[] pixels = new byte[size];
            Array.Clear(pixels, 0, size);
            newFormatedBitmapSource.CopyPixels(pixels, stride, 0);

            int x = 0;
            int y = 0;
            Point ptCenter = new Point();
            ptCenter.X = newFormatedBitmapSource.PixelWidth / 2;
            ptCenter.Y = newFormatedBitmapSource.PixelHeight / 2;
            int r_begin_min = (int)Math.Min(ptCenter.X, ptCenter.Y);

            ScaleTransform scaleTransform = new ScaleTransform(fPointWidth, fPointWidth);

            double delta = TotalPoints / 360;
            for (double angle = 0; angle < 360; angle += delta)
            {
                double rad_angle = Math.PI * angle / 180.0;
                int r_begin = (int)Math.Sqrt(2 * r_begin_min * r_begin_min);

                for (double r = r_begin; r > 0; r-=1)
                {
                    x = (int)(r * Math.Cos(rad_angle)) + (int)ptCenter.X;
                    y = (int)(r * Math.Sin(rad_angle)) + (int)ptCenter.Y;
                    if (x >= newFormatedBitmapSource.PixelWidth ||
                        y >= newFormatedBitmapSource.PixelHeight ||
                        x < 0 || y < 0)
                    {
                        continue;
                    }

                    int index = y * stride + 3 * x;
                    byte red = pixels[index];
                    byte green = pixels[index + 1];
                    byte blue = pixels[index + 2];

                    Color cur_col = Color.FromRgb(red, green, blue);
                    if (cur_col != Color.FromRgb(255, 255, 255))
                    {
                        int x_add = x - (int)ptCenter.X;
                        int y_add = y - (int)ptCenter.Y;
                        Point ptToAdd = new Point(x_add, y_add);


                        if (list.Count == 0 || list.Last() != ptToAdd)
                        {
                            list.Add(ptToAdd);
                            if (list.Count == 215)
                            {
                                int h = 0 + 4;
                            }
                        }

                        break;
                    }
                }
            }
            if (scaleTransform != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = scaleTransform.Transform(list[i]);
                }

            }
        }
        private void buttonCreateFigure_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.bmp|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                SaveFileDialog op1 = new SaveFileDialog();
                op1.Filter = "xml|*.xml;";
                if (op1.ShowDialog() == true)
                {
                    List<Point> list = new List<Point>();
                    GetCrossSectionProfile(list, op.FileName, Convert.ToInt32(textBoxPointsInFigure.Text), Convert.ToDouble(textBoxPointSize.Text));
                    ListPoint.SerializeObject(list, op1.FileName);
                }
            }
        }

        public void GetLongitudinalSectionProfile(string FileNameCurve, List<double> list, double hInOnePoint)
        {
            BitmapImage myBitmapImage = GetImage(FileNameCurve);
            if (myBitmapImage == null)
            {
                return;
            }

            FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
            newFormatedBitmapSource.BeginInit();
            newFormatedBitmapSource.Source = myBitmapImage;
            newFormatedBitmapSource.DestinationFormat = PixelFormats.Rgb24;
            newFormatedBitmapSource.EndInit();

            int width = newFormatedBitmapSource.PixelWidth;
            int height = newFormatedBitmapSource.PixelHeight;
            int stride = width * 3;
            int size = newFormatedBitmapSource.PixelHeight * stride;
            byte[] pixels = new byte[size];
            Array.Clear(pixels, 0, size);
            newFormatedBitmapSource.CopyPixels(pixels, stride, 0);

            int x = 0;
            int y = 0;
            for (x = 0; x < width; x++)
            {
                for (y = 0; y < height; y++)
                {
                    int index = y * stride + 3 * x;
                    byte red = pixels[index];
                    byte green = pixels[index + 1];
                    byte blue = pixels[index + 2];

                    Color cur_col = Color.FromRgb(red, green, blue);
                    if (cur_col != Color.FromRgb(255, 255, 255))
                    {
                        list.Add(height - y);
                        break;
                    }
                }
            }
            //double base_point = 0;
            //if (list.Count > 0)
            //{
            //    base_point = list[0];
            //    if (MathHelper.IsZero(base_point))
            //    {
            //        base_point = MathHelper.Epsilon;
            //    }
            //}
            for (x = 0; x < list.Count; x++)
            {
                double cur_point = list[x];
                double factor = cur_point  * hInOnePoint;
                list[x] = factor;
            }
        }

        private void buttonCreateCurve_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.bmp|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                SaveFileDialog op1 = new SaveFileDialog();
                op1.Filter = "xml|*.xml;";
                if (op1.ShowDialog() == true)
                {
                    List<double> list = new List<double>();
                    GetLongitudinalSectionProfile(op.FileName, list, Convert.ToDouble(textBoxCurvePointHeight.Text));
                    ListDouble.SerializeObject(list, op1.FileName);
                }
            }
        }

    }
}
