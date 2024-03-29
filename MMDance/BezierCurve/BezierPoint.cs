﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;

namespace BezierCurve
{

    public class BezierPoint
    {
        public BezierPoint()
        {

        }
        public BezierPoint(Point pt)
        {
            X = pt.X;
            Y = pt.Y;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public Point point 
        { 
            get
            {
                return new Point(X, Y);
            }
        }
    }
    public class BezierSegment
    {
        public List<BezierPoint> Points { get; set; }
        public int Number { get; set; }
        public BezierSegment()
        {
            Points = new List<BezierPoint>();
        }
        public List<Point> GetPoints()
        {
            List<Point> pts = new List<Point>();
            foreach (BezierPoint pt in Points)
            {
                pts.Add(pt.point);
            }
            return pts;
        }
    }

    public class BezierViewModel : DependencyObject
    {
        public IList<BezierSegment> Segments { get; set; }

        public BezierViewModel()
        {
            Segments = new List<BezierSegment>();
        }
    }
}
