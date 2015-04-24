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

namespace BezierCurve
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PolyLineSegment GetBezierApproximation(Point[] controlPoints, int outputSegmentCount)
        {
            Point[] points = new Point[outputSegmentCount + 1];
            for (int i = 0; i <= outputSegmentCount; i++)
            {
                double t = (double)i / outputSegmentCount;
                points[i] = GetBezierPoint(t, controlPoints, 0, controlPoints.Length);
            }
            return new PolyLineSegment(points, true);
        }

        Point GetBezierPoint(double t, Point[] controlPoints, int index, int count)
        {
            if (count == 1)
                return controlPoints[index];
            var P0 = GetBezierPoint(t, controlPoints, index, count - 1);
            var P1 = GetBezierPoint(t, controlPoints, index + 1, count - 1);
            return new Point((1 - t) * P0.X + t * P1.X, (1 - t) * P0.Y + t * P1.Y);
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Point[] points = new[] { 
            new Point(0, 50),
            new Point(100, 100),
            new Point(200, 100),
            new Point(300, 100),
            new Point(400, 50)
        };
            var b = GetBezierApproximation(points, 256);
            PathFigure pf = new PathFigure(b.Points[0], new[] { b }, false);
            PathFigureCollection pfc = new PathFigureCollection();
            pfc.Add(pf);
            var pge = new PathGeometry();
            pge.Figures = pfc;
            Path p = new Path();
            p.Data = pge;
            p.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            ((Grid)sender).Children.Add(p);
        }
    }
}
