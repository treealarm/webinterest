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
    /// Interaction logic for UserControlEditBezier.xaml
    /// </summary>
    public partial class UserControlEditBezier : UserControl
    {
        public UserControlEditBezier()
        {
            InitializeComponent();
            BezierSegment Segment = new BezierSegment();
            DataContext = m_Segments;
        }

        public BezierViewModel m_Segments = new BezierViewModel();

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
                Point[] points = new[] { 
                new Point(0, 50),
                new Point(100, 100),
                new Point(200, 100),
                new Point(300, 100),
                new Point(400, 50)
            };
                List<Point> b = GetBezierApproximation(points.ToList(), 256);
                for (int i = 1; i < b.Count; i++)
                {
                    Line l = new Line();
                    l.X1 = b[i - 1].X;
                    l.Y1 = b[i - 1].Y;
                    l.X2 = b[i].X;
                    l.Y2 = b[i].Y;
                    l.Stroke = new SolidColorBrush(Color.FromRgb(0,200,0));
                    ((Canvas)sender).Children.Add(l);
                }
        }

        List<Point> GetBezierApproximation(List<Point> controlPoints, int outputSegmentCount)
        {
            if (controlPoints.Count < 2)
            {
                return null;
            }
            List<Point> points = new List<Point>();
            for (int i = 0; i <= outputSegmentCount; i++)
            {
                double t = (double)i / outputSegmentCount;
                Point pt = GetBezierPoint(t, controlPoints, 0, controlPoints.Count);
                points.Add(pt);
            }
            return points;
        }

        Point GetBezierPoint(double t, List<Point> controlPoints, int index, int count)
        {
            if (count == 1)
                return controlPoints[index];
            var P0 = GetBezierPoint(t, controlPoints, index, count - 1);
            var P1 = GetBezierPoint(t, controlPoints, index + 1, count - 1);
            return new Point((1 - t) * P0.X + t * P1.X, (1 - t) * P0.Y + t * P1.Y);
        }

        void RedrawLines()
        {
            List<BezierSegment> SortedList = m_Segments.Segments.OrderBy(o => o.Number).ToList();
            m_Canvas.Children.Clear();
            for (int k = 0; k < SortedList.Count; k++)
            {
                BezierSegment seg = SortedList[k];
                List<Point> b = GetBezierApproximation(seg.GetPoints(), 256);
                if (b == null)
                {
                    continue;
                }
                for (int i = 1; i < b.Count; i++)
                {
                    Line l = new Line();
                    l.X1 = b[i - 1].X;
                    l.Y1 = b[i - 1].Y;
                    l.X2 = b[i].X;
                    l.Y2 = b[i].Y;
                    l.Stroke = new SolidColorBrush(Color.FromRgb(0, 200, 0));
                    m_Canvas.Children.Add(l);
                }
            }
        }
        private void dataGridSegments_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //RedrawLines();
            //return;
            //int newVal = Convert.ToInt32(((TextBox)e.EditingElement).Text);
        }

        private void dataGridPoints_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //RedrawLines();
        }

        private void m_Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                RedrawLines();
            }
        }
    }
}
