using System.Windows.Media.Media3D;
namespace Kit3D
{
    /// <summary>
    /// Represents a ray in 3D space with an origin and a direction
    /// </summary>
    public struct Ray
    {
        private Point3D origin;
        private Vector3D direction;

        public Ray(Point3D origin, Vector3D direction)
        {
            this.origin = origin;

            direction.Normalize();
            this.direction = direction;
        }

        public Point3D Origin
        {
            get { return this.origin; }
        }

        public Vector3D Direction
        {
            get { return this.direction; }
        }
    }
}
