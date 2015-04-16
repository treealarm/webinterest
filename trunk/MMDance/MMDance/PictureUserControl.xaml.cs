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
using System.Xml;

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

        private void Click_FileName(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "xml|*.xml;";
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
                if (element.Length <= 0)
                {
                    continue;
                }
                List<Point> list = ListPoint.DeserializeObject(element.FileName);
                //GetCrossSectionProfile(element, list, "d:\\1.bmp");
                //ListPoint.SerializeObject(list, "d:\\1.xml");

                List<double> listLong = new List<double>();
                GetLongitudinalSectionProfile(element, listLong);

                m_UserControlFor3D.Calculate(list, listLong, cur_pos, element.Length, element.Angle);
                cur_pos += element.Length;
            }
        }

        public void GetCrossSectionProfile(List<Point> list, string fileName)
        {
            BitmapImage myBitmapImage = ProfileElement.GetImage(fileName);
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

            ScaleTransform scaleTransform = null;

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
            if (scaleTransform != null)
            {
                for (int i = 0; i < list.Count; i++ )
                {
                    list[i] = scaleTransform.Transform(list[i]);
                }
                
            }
         }


        public void GetLongitudinalSectionProfile(ProfileElement cur, List<double> list)
        {
            if (cur == null)
            {
                return;
            }
            BitmapImage myBitmapImage = ProfileElement.GetImage(cur.FileNameCurve);
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
            //double base_point = 0;
            //if (list.Count > 0)
            //{
            //    base_point = list[0];
            //    if (MathHelper.IsZero(base_point))
            //    {
            //        base_point = MathHelper.Epsilon;
            //    }
            //}
            //for (x = 0; x < list.Count; x++)
            //{
            //    double cur_point = list[x];
            //    if (MathHelper.IsZero(cur_point))
            //    {
            //        cur_point = MathHelper.Epsilon;
            //    }
            //    double factor = cur_point / base_point;
            //    list[x] = factor;
            //}
        }
        private void ProfileDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProfileElement element = ProfileDataGrid.SelectedItem as ProfileElement;
            if (element == null || element.Length <= 0)
            {
                return;
            }
            List<Point> list = ListPoint.DeserializeObject(element.FileName);
            //GetCrossSectionProfile(element, list);
            if (list.Count == 0)
            {
                return;
            }
            List<double> listLong = new List<double>();
            GetLongitudinalSectionProfile(element, listLong);
            m_UserControlFor3D.Calculate(list, listLong, 0, element.Length, element.Angle);
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ProfileDataSource = ProfileElementSerializer.SerializeObject(m_ProfileData);
            Properties.Settings.Default.Save();
            UpdateProfileResult();
        }

        private void buttonExport_Click(object sender, RoutedEventArgs e)
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
                    GetCrossSectionProfile(list, op.FileName);
                    ListPoint.SerializeObject(list, op1.FileName);
                }
            }
        }
    }
}
