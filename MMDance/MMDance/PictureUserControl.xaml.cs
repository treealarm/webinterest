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
using System.Xml.Serialization;
using System.IO;
using System.Collections.Specialized;
using System.ComponentModel;
using Kit3D;

namespace MMDance
{
    /// <summary>
    /// Interaction logic for PictureUserControl.xaml
    /// </summary>
    public partial class PictureUserControl : UserControl
    {
        ProfileElementList m_ProfileData = new ProfileElementList();
        public PictureUserControl()
        {
            InitializeComponent();
            try
            {
                m_ProfileData = ProfileElementSerializer.DeserializeObject(Properties.Settings.Default.ProfileDataSource);
            }
            catch (Exception e)
            {
            }
            if (m_ProfileData == null)
            {
                m_ProfileData = new ProfileElementList();
            }

            ProfileDataGrid.DataContext = m_ProfileData;
        }

        public void UpdateCurrentPosition(double x1, double y1)
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

        private void loaded_image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainWindow.GetMainWnd().UpdateCurrentPosition();
        }

        private void image_canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(image_canvas);
            double xratio = MainWindow.GetMainWnd().GetImageSize().Width / image_canvas.ActualWidth;
            double yratio = MainWindow.GetMainWnd().GetImageSize().Height / image_canvas.ActualHeight;
            textBlock.Text = string.Format("{0}:{1}", Convert.ToInt32(pt.X * xratio),
                MainWindow.GetMainWnd().GetImageSize().Height-Convert.ToInt32(pt.Y * yratio));
            textBlock.Visibility = System.Windows.Visibility.Visible;
            Canvas.SetLeft(textBlock, pt.X-20);
            Canvas.SetTop(textBlock, pt.Y - 20);
        }

        private void Click_FileName(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.bmp|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                ProfileElement element = ((FrameworkElement)sender).DataContext as ProfileElement;
                element.FileName = op.FileName;
                ProfileDataGrid.CommitEdit();
            }
        }

        private void Click_FileNameCurve(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.bmp|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                ProfileElement element = ((FrameworkElement)sender).DataContext as ProfileElement;
                element.FileNameCurve = op.FileName;
                ProfileDataGrid.CommitEdit();
            }
        }

        private void ProfileDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            DataTemplate temp = ProfileDataGrid.RowDetailsTemplate;
            ProfileDataGrid.RowDetailsTemplate = null;
            ProfileDataGrid.RowDetailsTemplate = temp;
        }
        public void UpdateProfileResult()
        {
            int cur_pos = 0;
            for (int i = 0; i < m_ProfileData.Count; i++)
            {
                ProfileElement element = m_ProfileData[i];
                List<Point> list = new List<Point>();
                
                GetCrossSectionProfile(element, list);

                List<double> listLong = new List<double>();
                GetLongitudinalSectionProfile(element, listLong);

                UserControlFor3D.Calculate(list, listLong, cur_pos, element.Length, element.Angle);
                cur_pos += element.Length;
            }
        }

        public void GetCrossSectionProfile(ProfileElement cur, List<Point> list)
        {
            if (cur == null)
            {
                return;
            }
            BitmapImage myBitmapImage = cur.GetImage(cur.FileName);
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
            for (double angle = 0; angle < 360; angle += 0.9)
            {
                double rad_angle = Math.PI * angle / 180.0;
                int r_begin = (int)Math.Sqrt(2*r_begin_min * r_begin_min);

                for (double r = r_begin; r > 0; r--)
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
                        }
                        
                        break;
                    }
                }
            }
         }


        public void GetLongitudinalSectionProfile(ProfileElement cur, List<double> list)
        {
            if (cur == null)
            {
                return;
            }
            BitmapImage myBitmapImage = cur.GetImage(cur.FileNameCurve);
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
            for (x = 0; x < width; x++ )
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
            double base_point = 0;
            if (list.Count > 0)
            {
                base_point = list[0];
                if (MathHelper.IsZero(base_point))
                {
                    base_point = MathHelper.Epsilon;
                }
            }
            for (x = 0; x < list.Count; x++)
            {
                double cur_point = list[x];
                if (MathHelper.IsZero(cur_point))
                {
                    cur_point = MathHelper.Epsilon;
                }
                double factor = cur_point / base_point;
                list[x] = factor;
            }
        }
        private void ProfileDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<UIElement> itemstoremove = new List<UIElement>();
            foreach (UIElement ui in image_canvas.Children)
            {
                if (ui.Uid.StartsWith("Line"))
                {
                    itemstoremove.Add(ui);
                }
            }
            foreach (UIElement ui in itemstoremove)
            {
                image_canvas.Children.Remove(ui);
            }

            List<Point> list = new List<Point>();
            ProfileElement element = ProfileDataGrid.SelectedItem as ProfileElement;
            if (element == null || element.Length <= 0)
            {
                return;
            }
            
            GetCrossSectionProfile(element, list);
            if (list.Count == 0)
            {
                return;
            }
            List<double> listLong = new List<double>();
            GetLongitudinalSectionProfile(element, listLong);
            UserControlFor3D.Calculate(list, listLong, 0, element.Length, element.Angle);

            //for (int i = 1; i < list.Count; i++ )
            //{
            //    Line myLine = new Line();
            //    myLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            //    myLine.X1 = list[i-1].X;
            //    myLine.Y1 = list[i-1].Y;
                
            //    myLine.X2 = list[i].X;
            //    myLine.Y2 = list[i].Y;
            //    myLine.HorizontalAlignment = HorizontalAlignment.Left;
            //    myLine.VerticalAlignment = VerticalAlignment.Center;
            //    myLine.StrokeThickness = 1;
            //    myLine.Uid = "Line" + i.ToString();
            //    image_canvas.Children.Add(myLine);
            //}
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ProfileDataSource = ProfileElementSerializer.SerializeObject(m_ProfileData);
            Properties.Settings.Default.Save();
            UpdateProfileResult();
        }
    }
}
