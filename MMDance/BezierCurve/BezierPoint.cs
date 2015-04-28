using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;

namespace BezierCurve
{

    public class BezierPoint
    {
        public BezierPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
        public double X { get; set; }
        public double Y { get; set; }
    }
    public class BezierSegment
    {
        public IList<BezierPoint> Points { get; set; }
        public BezierSegment()
        {
            Points = new List<BezierPoint>();
            Points.Add(new BezierPoint(10, 10));
        }
        public string Number { get; set; }
    }

    public class BezierViewModel : DependencyObject
    {
        public List<BezierSegment> Segments { get; set; }

        public BezierViewModel()
        {
            Segments = new List<BezierSegment>();
        }
    }
}
