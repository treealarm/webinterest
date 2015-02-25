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

namespace MMDance
{
    /// <summary>
    /// Interaction logic for PictureUserControl.xaml
    /// </summary>
    public partial class PictureUserControl : UserControl
    {
        public PictureUserControl()
        {
            InitializeComponent();
            List<ProfileElement> users = new List<ProfileElement>();
            users.Add(new ProfileElement() { FileName = "C:\\map.bmp", Length = 10 });
            users.Add(new ProfileElement() { FileName = "C:\\2.bmp", Length = 20 });

            ProfileDataGrid.DataContext = users;
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
                var yourType = ((FrameworkElement)sender).DataContext as ProfileElement;
                yourType.FileName = op.FileName;
                ProfileDataGrid.CommitEdit();
            }
        }

        private void ProfileDataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {

        }
    }
}
