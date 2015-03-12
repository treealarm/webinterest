/*
using System;
using Kit3D.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kit3D.Math
{
    /// <summary>
    /// Class that represents a viewing frustum
    /// </summary>
    public sealed class Frustum
    {
        public enum ClipResults
        {
            /// <summary>
            /// All the points are inside the frustum
            /// </summary>
            Inside,

            /// <summary>
            /// All the points are outside of the frustum
            /// </summary>
            Outside,

            /// <summary>
            /// Some of the points intersected with the frustum and
            /// were clipped accordingly
            /// </summary>
            Intersect
        }

        /// <summary>
        /// A set of planes which we can clip against
        /// </summary>
        [Flags]
        public enum ClippingPlanes
        {
            Top,
            Bottom,
            Left,
            Right,
            Front,
            Back
        }

        /// <summary>
        /// Creates a frustum with the given dimensions
        /// </summary>
        /// <param name="frontTopLeft"></param>
        /// <param name="frontTopRight"></param>
        /// <param name="frontBottomRight"></param>
        /// <param name="frontBottomLeft"></param>
        /// <param name="backTopLeft"></param>
        /// <param name="backTopRight"></param>
        /// <param name="backBottomRight"></param>
        /// <param name="backBottomLeft"></param>
        public Frustum(Point3D frontTopLeft,
                       Point3D frontTopRight,
                       Point3D frontBottomRight,
                       Point3D frontBottomLeft,
                       Point3D backTopLeft,
                       Point3D backTopRight,
                       Point3D backBottomRight,
                       Point3D backBottomLeft)
        {
            //Create all of the necessary planes, all of the normals should point into the center
            //of the frustum volume
            this.Front = new Plane(frontBottomLeft, frontTopLeft, frontTopRight);
            this.Back = new Plane(backBottomLeft, backBottomRight, backTopRight);
            this.Left = new Plane(frontBottomLeft, backBottomLeft, backTopLeft);
            this.Right = new Plane(frontBottomRight, frontTopRight, backTopRight);
            this.Top = new Plane(frontTopLeft, backTopLeft, backTopRight);
            this.Bottom = new Plane(frontBottomRight, backBottomRight, backBottomLeft);
        }

        /// <summary>
        /// Clips the specified points to the frustum
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="clippedPoints"></param>
        /// <returns></returns>
        public ClipResults Clip(Point3D p0, Point3D p1, Point3D p2, out List<Point3D> clippedPoints, ClippingPlanes planesToClipAgainst)
        {
            clippedPoints = null;

            //If we got to here, then just assume if is it inside
            return ClipResults.Inside;
        }

        private Plane Front
        {
            get;
            set;
        }

        private Plane Back
        {
            get;
            set;
        }

        private Plane Left
        {
            get;
            set;
        }

        private Plane Right
        {
            get;
            set;
        }

        private Plane Top
        {
            get;
            set;
        }

        private Plane Bottom
        {
            get;
            set;
        }
    }
}
*/