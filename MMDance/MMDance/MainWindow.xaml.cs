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
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            // Describes the brush's color using RGB values.  
            // Each value has a range of 0-255.
            //mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
            //myEllipse.Fill = mySolidColorBrush;
            //myEllipse.StrokeThickness = 2;
            //myEllipse.Stroke = Brushes.Black;

            //// Set the width and height of the Ellipse.
            
            //myEllipse.Width = 200;
            //myEllipse.Height = 100;
            //// Add the Ellipse to the StackPanel.
            //image_canvas.Children.Add(myEllipse);
        }

        private void image_canvas_MouseMove(object sender, MouseEventArgs e)
        {
            x_line.X1 = e.GetPosition(image_canvas).X;
            x_line.X2 = e.GetPosition(image_canvas).X;
            x_line.Y1 = 0;
            x_line.Y2 = image_canvas.ActualHeight;

            y_line.Y1 = e.GetPosition(image_canvas).Y;
            y_line.Y2 = e.GetPosition(image_canvas).Y;
            y_line.X1 = 0;
            y_line.X2 = image_canvas.ActualWidth;
        }
    }
}
