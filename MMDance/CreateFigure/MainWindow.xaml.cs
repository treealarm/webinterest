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

        Ellipse CreateEllipse(double width, double height, double desiredCenterX, double desiredCenterY)
        {
            Ellipse ellipse = new Ellipse { Width = width, Height = height };
            double left = desiredCenterX - (width / 2);
            double top = desiredCenterY - (height / 2);

            ellipse.Margin = new Thickness(left, top, 0, 0);
            return ellipse;
        }

        private void buttonCreateGreece_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.Children.Clear();
            
            double R = Convert.ToDouble(textBoxGreeceDiameter.Text) / 2;
            int nPoints = Convert.ToInt32(textBoxGreeceEllipceNumber.Text);
            double EllipseWidth = Convert.ToDouble(textBoxGreeceEllipceWidth.Text);
            double EllipseHeight = Convert.ToDouble(textBoxGreeceEllipceHeight.Text);
            double fAngleDelta = ConvertToRadians(360 / nPoints);


            Ellipse myEllipse = new Ellipse { Width = R*2, Height = R*2 };
            myEllipse.Stroke = System.Windows.Media.Brushes.Black;
            myEllipse.Fill = System.Windows.Media.Brushes.BlueViolet;
            myEllipse.RenderTransform = new TranslateTransform(EllipseHeight, EllipseHeight); 
            myCanvas.Children.Add(myEllipse);

            for (double angle = 0; angle < 360; angle += 360 / nPoints)
            {
                myEllipse = new Ellipse { Width = EllipseWidth, Height = EllipseHeight };
                TransformGroup transGroup = new TransformGroup();
                transGroup.Children.Add(new RotateTransform(angle, EllipseWidth / 2, R + EllipseHeight / 2));
                transGroup.Children.Add(new TranslateTransform(R - EllipseWidth / 2  + EllipseHeight, -EllipseHeight / 2 +EllipseHeight));
                myEllipse.RenderTransform = transGroup; 
                myEllipse.Stroke = System.Windows.Media.Brushes.White;
                myEllipse.Fill = System.Windows.Media.Brushes.White;
                myCanvas.Children.Add(myEllipse);
            }
            myCanvas.Width = R * 2 + EllipseHeight*2;
            myCanvas.Height = R * 2 + EllipseHeight*2;
            ExportToBmp(new Uri("D:\\Canvas.bmp"), myCanvas);
        }

        public void ExportToBmp(Uri path, Canvas surface)
        {
            if (path == null) return;

            // Save current canvas transform
            Transform transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;
            
            // Get the size of canvas
            Size size = new Size(surface.Width, surface.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(surface);

            // Create a file stream for saving image
            using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
            {
                // Use png encoder for our data
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(outStream);
            }

            // Restore previously saved layout
            surface.LayoutTransform = transform;
        }

        public static BitmapSource CreateBitmap(int width, int height, double dpi, Action<DrawingContext> render)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                render(drawingContext);
            }
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                width, height, dpi, dpi, PixelFormats.Default);
            bitmap.Render(drawingVisual);

            return bitmap;
        }

        private void buttonCreateBride_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog op1 = new SaveFileDialog();
            op1.Filter = "bmp|*.bmp;";
            if (op1.ShowDialog() == true)
            {
                List<Point> list = new List<Point>();
                double R = Convert.ToDouble(textBoxCenterCircleDiameter.Text) / 2;
                double RThr = Convert.ToDouble(textBoxCirclesDiameter.Text) / 2;
                int nPoints = Convert.ToInt32(textBoxCirclesNumber.Text);

                double shift = (RThr + R);
                double TotalWidth = shift * 2;
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                Pen pen = new Pen();
                pen.Brush = Brushes.White;

                double fAngleDelta = ConvertToRadians(360 / nPoints);

                drawingContext.DrawRectangle(new SolidColorBrush(Color.FromRgb(255, 255, 255)), pen, new Rect(0, 0, TotalWidth, TotalWidth));

                pen = new Pen();
                pen.Brush = Brushes.Black;
                
                for (double angle = 0; angle < Math.PI * 2; angle += fAngleDelta)
                {
                    double y = R * Math.Sin(angle) + shift;
                    double x = R * Math.Cos(angle) + shift;

                    drawingContext.DrawEllipse(new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)), null, new Point(x, y), RThr, RThr);
                }


                drawingContext.DrawRectangle(new SolidColorBrush(Color.FromRgb(0, 0, 0)), null, new Rect(shift-1, shift-1, 2, 2));

                drawingContext.Close();

                RenderTargetBitmap bmp = new RenderTargetBitmap((int)TotalWidth, (int)TotalWidth, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(drawingVisual);

                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));

                using (Stream stm = File.Create(op1.FileName))
                {
                    encoder.Save(stm);
                }
            }

        }

    }
}
