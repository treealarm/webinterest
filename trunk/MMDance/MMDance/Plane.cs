using System.Windows.Media.Media3D;
namespace Kit3D
{
    public struct Plane
    {
        private Vector3D normal;
        private double d;

        /// <summary>
        /// Parameters for a generalized plane equation
        /// </summary>
        public Plane(double a, double b, double c, double d)
        {
            this.normal = new Vector3D(a, b, c);
            this.normal.Normalize();
            this.d = d;
        }

        public Plane(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v1 = p1 - p0;
            Vector3D v2 = p2 - p0;
            this.normal = Vector3D.CrossProduct(v1, v2);
            this.normal.Normalize();
            this.d = -(this.normal.X * p0.X + this.normal.Y * p0.Y + this.normal.Z * p0.Z);
        }

        public Vector3D Normal
        {
            get
            {
                return this.normal;
            }
        }

        /// <summary>
        /// Returns the shortest distance from the point to the plane
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public double DistanceToPoint(Point3D point)
        {
            return point.X * this.normal.X +
                   point.Y * this.normal.Y +
                   point.Z * this.normal.Z +
                   this.d;
        }

        /// <summary>
        /// Returns the point of intersection with the ray and the plane.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns>Point(NaN, NaN, Nan) if the ray is parallel to the plane or the plane is behind the ray</returns>
        public bool IntersectionPoint(Ray ray, out Point3D intersectionPoint)
        {
            //Check to see if the plane and the ray are parallel, if so then
            //they will not intersect
            double d = Vector3D.DotProduct(this.Normal, ray.Direction);
            if ((d > -1e-12) && (d < 1e-12))
            {
                intersectionPoint = new Point3D(double.NaN, double.NaN, double.NaN);
                return false;
            }

            // P = P0 + tV
            // P . N + d = 0
            //
            // (P0 + tV) . N + d = 0
            // P0 . N + tV . N + d = 0
            // t = -(P0 . N + d) / V . N

            double t = -(Vector3D.DotProduct((Vector3D)ray.Origin, this.Normal) + this.D) / d;
            if (t < 0)
            {
                intersectionPoint = new Point3D(double.NaN, double.NaN, double.NaN);
                return false;
            }
            intersectionPoint = ray.Origin + t * ray.Direction;
            return true;
        }

        /// <summary>
        /// The d value from the generalized form of the equation ax + by +cz + d
        /// </summary>
        public double D
        {
            get
            {
                return this.d;
            }
        }
        
    }
}
