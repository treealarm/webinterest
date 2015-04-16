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

        private void buttonCreateBraid_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog op1 = new SaveFileDialog();
            op1.Filter = "xml|*.xml;";
            if (op1.ShowDialog() == true)
            {
                List<Point> list = new List<Point>();
                double R = Convert.ToDouble(textBoxBaseDiameter.Text) / 2;
                double RThr = Convert.ToDouble(textBoxThreadDiameter.Text) / 2;
                int nPoints = Convert.ToInt32(textBoxThreadsNumber.Text);
                int nPointsInThread = Convert.ToInt32(textBoxPointsInThread.Text);
                
                double fAngleDelta = ConvertToRadians(360 / nPoints);
                for (double angle = 0; angle < Math.PI * 2; angle += fAngleDelta)
                {
                    double y = R * Math.Sin(angle);
                    double x = R * Math.Cos(angle);

                    AddCircle(list, new Point(x, y), RThr, nPointsInThread);

                    list.Add(new Point(x, y));
                }
                ListPoint.SerializeObject(list, op1.FileName);
            }
        }
    }
}
