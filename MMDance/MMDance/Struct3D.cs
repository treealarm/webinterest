using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace MMDance
{
    public class Points
    {
        public double X1 = 0;
        public double X2 = 0;

        public double Y1 = 0;
        public double Y2 = 0;

        public double Z1 = 0;
        public double Z2 = 0;
    }
    class Triangle
    {
        Point3D a, b, c;
        public Triangle(Point3D a1, Point3D b1, Point3D c1)
        {
            a = a1;
            b = b1;
            c = c1;
        }
        public Vector3D Normal
        {
            get
            {
                var dir = Vector3D.CrossProduct(b - a, c - a);
                dir.Normalize();
                return dir;
            }
        }
        public double GetAngle(Triangle plane2)
        {
            Vector3D norm2 = plane2.Normal;
            return Vector3D.AngleBetween(Normal, norm2);
        }
    }
}
