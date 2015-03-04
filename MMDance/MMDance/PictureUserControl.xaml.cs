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

            m_ProfileData = ProfileElementSerializer.DeserializeObject(Properties.Settings.Default.ProfileDataSource);
            if (m_ProfileData == null)
            {
                m_ProfileData = new ProfileElementList();
            }
            //m_ProfileData.Add(new ProfileElement() { FileName = "C:\\map.bmp", Length = 10 });
            //m_ProfileData.Add(new ProfileElement() { FileName = "C:\\2.bmp", Length = 20 });

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

        private void Click_Click(object sender, RoutedEventArgs e)
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

        private void ProfileDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Properties.Settings.Default.ProfileDataSource = ProfileElementSerializer.SerializeObject(m_ProfileData);
            Properties.Settings.Default.Save();
            DataTemplate temp = ProfileDataGrid.RowDetailsTemplate;
            ProfileDataGrid.RowDetailsTemplate = null;
            ProfileDataGrid.RowDetailsTemplate = temp;
            UpdateProfileResult();
        }
        public void UpdateProfileResult()
        {
            int cur_pos = 0;
            for (int i = 0; i < m_ProfileData.Count; i++)
            {
                ProfileElement cur = m_ProfileData[i];
                cur_pos += cur.Length;
                List<Point> list = new List<Point>();
                DrawCurProfileResult(cur_pos, cur, list);
            }
        }

        public void DrawCurProfileResult(int start_pos, ProfileElement cur, List<Point> list)
        {
            BitmapImage myBitmapImage = cur.Image;
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
            int r_begin = (int)Math.Min(newFormatedBitmapSource.PixelWidth/2, newFormatedBitmapSource.PixelHeight/2)-1;
            for (int angle = 0; angle < 360; angle++)
            {
                double rad_angle = Math.PI * angle / 180.0;
                for (int r = r_begin; r > 0; r--)
                {
                    x = (int)(r * Math.Cos(rad_angle)) + r_begin;
                    y = (int)(r * Math.Sin(rad_angle)) + r_begin;

                    int index = y * stride + 3 * x;
                    byte red = pixels[index];
                    byte green = pixels[index + 1];
                    byte blue = pixels[index + 2];

                    Color cur_col = Color.FromRgb(red, green, blue);
                    if (cur_col != Color.FromRgb(255, 255, 255))
                    {
                        list.Add(new Point(x, y));
                        break;
                    }
                }
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
            DrawCurProfileResult(0, ProfileDataGrid.SelectedItem as ProfileElement, list);
            for (int i = 1; i < list.Count; i++ )
            {
                Line myLine = new Line();
                myLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                myLine.X1 = list[i-1].X;
                myLine.Y1 = list[i-1].Y;
                
                myLine.X2 = list[i].X;
                myLine.Y2 = list[i].Y;
                myLine.HorizontalAlignment = HorizontalAlignment.Left;
                myLine.VerticalAlignment = VerticalAlignment.Center;
                myLine.StrokeThickness = 1;
                myLine.Uid = "Line" + i.ToString();
                image_canvas.Children.Add(myLine);
            }
        }
    }
}
