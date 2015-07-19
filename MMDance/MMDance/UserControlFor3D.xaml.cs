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
using System.Windows.Media.Media3D;
using Kit3D;

namespace MMDance
{
    /// <summary>
    /// Interaction logic for UserControlFor3D.xaml
    /// </summary>
    public partial class UserControlFor3D : UserControl
    {
        AxisAngleRotation3D m_axA3d = new AxisAngleRotation3D();
        AxisAngleRotation3D m_axB3d = new AxisAngleRotation3D();
        TranslateTransform3D m_trans3d = new TranslateTransform3D();
        

        Model3DGroup m_Model3DGroup = new Model3DGroup();

        public List<Point> m_listLimits = new List<Point>();
        public List<Point> m_listCurve = new List<Point>();

        public UserControlFor3D()
        {
            InitializeComponent();
            
            m_Model3DGroup = GetNewGroup();
            myModelVisual3D.Content = m_Model3DGroup;
        }

        Model3DGroup GetNewGroup()
        {
            Model3DGroup myModel3DGroup = new Model3DGroup();
            // Define the lights cast in the scene. Without light, the 3D object cannot  
            // be seen. Note: to illuminate an object from additional directions, create  
            // additional lights.
            AmbientLight _ambLight = new AmbientLight(System.Windows.Media.Brushes.Gray.Color);
            DirectionalLight _dirLight = new DirectionalLight();
            _dirLight.Color = System.Windows.Media.Brushes.White.Color;
            _dirLight.Direction = new System.Windows.Media.Media3D.Vector3D(1, -0.2, -1);

            myModel3DGroup.Children.Add(_ambLight);
            myModel3DGroup.Children.Add(_dirLight);

            _dirLight = new DirectionalLight();
            _dirLight.Color = System.Windows.Media.Brushes.White.Color;
            _dirLight.Direction = new System.Windows.Media.Media3D.Vector3D(-1, 0.2, 1);
            myModel3DGroup.Children.Add(_dirLight);
            return myModel3DGroup;
            
        }
        double GetRadiusLimit(double Z)
        {
            double yMax = 10000;
            if (m_listLimits != null)
            {
                for (int i = 0; i < m_listLimits.Count; i++)
                {
                    Point pt2 = m_listLimits[i];
                    if (pt2.X >= Z)
                    {
                        if (i == 0)
                        {
                            return pt2.Y;
                        }
                        Point pt1 = m_listLimits[i - 1];
                        yMax = (-(pt1.X * pt2.Y - pt2.X * pt1.Y) - (pt1.Y - pt2.Y) * Z) / (pt2.X - pt1.X);
                        break;
                    }
                }
            }
            
            return yMax;
        }

        double GetRadiusIncrement(double Z)
        {
            double yMax = 0;
            if (m_listCurve != null && m_listCurve.Count > 1)
            {
                for (int i = 1; i < m_listCurve.Count; i++)
                {
                    Point pt1 = m_listCurve[i - 1];
                    Point pt2 = m_listCurve[i];
                    if (pt2.X >= Z)
                    {
                        yMax = (-(pt1.X * pt2.Y - pt2.X * pt1.Y) - (pt1.Y - pt2.Y) * Z) / (pt2.X - pt1.X);
                        break;
                    }
                }
            }

            return yMax;
        }
        Rect3D m_bounds = new Rect3D(); 
        public void Calculate(List<Point> listCross, int pos, int len, double angle)
        {
            m_bounds = new Rect3D();
            if (len <= 0)
            {
                return;
            }
            if (pos == 0)
            {
                m_Model3DGroup = GetNewGroup();
                myModelVisual3D.Content = m_Model3DGroup;

                Transform3DGroup transGroup = new Transform3DGroup();

                m_axB3d = new AxisAngleRotation3D(new Vector3D(0, 0, 1), sliderB.Value);
                RotateTransform3D myRotateTransform = new RotateTransform3D(m_axB3d);
                transGroup.Children.Add(myRotateTransform);
                
                m_axA3d = new AxisAngleRotation3D(new Vector3D(0, 1, 0), sliderA.Value);
                myRotateTransform = new RotateTransform3D(m_axA3d);
                transGroup.Children.Add(myRotateTransform);


                m_trans3d = new TranslateTransform3D(new Vector3D(0, 0, 0));
                transGroup.Children.Add(m_trans3d);

                m_Model3DGroup.Transform = transGroup;
                m_Model3DGroup.Children.Add(meshCubeModel);
            }
            GeometryModel3D myGeometryModel = new GeometryModel3D();
            m_Model3DGroup.Children.Add(myGeometryModel);

            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();
            
            Point3DCollection myPositionCollection = new Point3DCollection();
            
            myGeometryModel.Geometry = myMeshGeometry3D;

            List<Point3DCollection>  Discs = new List<Point3DCollection>();
            double cur_angle = 0;
            double ZIncrement = 1;
            if (MathHelper.IsZero(angle) &&
                (m_listCurve == null || m_listCurve.Count < 2) && 
                (m_listLimits == null || m_listLimits.Count < 2))
            {
                ZIncrement = len;
            }

            for (double Z = pos; Z <= pos + len; Z += ZIncrement)
            {
                Point3DCollection disc = new Point3DCollection();
                cur_angle += angle;

                AxisAngleRotation3D myRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), cur_angle);
                RotateTransform3D myRotateTransform = new RotateTransform3D(myRotation);
                TranslateTransform3D transTransform = new TranslateTransform3D(0, 0, Z);
                //ScaleTransform3D ScaleTrans = new ScaleTransform3D(new Vector3D(Zoom, Zoom, 0), new Point3D(0, 0, Z));
                double RadLimit = GetRadiusLimit(Z);
                double RadIncrement = GetRadiusIncrement(Z);
                if (listCross.First() != listCross.Last())
                {//make closed loop
                    listCross.Add(listCross.First());
                }

                for (int i = 0; i < listCross.Count; i++ )
                {
                    double x = listCross[i].X;
                    double y = listCross[i].Y;

                    Vector3D vec = new Vector3D(x, y, 0);

                    if (!MathHelper.IsZero(RadIncrement))
                    {
                        Vector3D vecNorm = new Vector3D(x, y, 0);
                        double ZoomFactor = RadIncrement/10;
                        vec *= ZoomFactor;
                    }

                    if (vec.Length > RadLimit)
                    {
                        Vector3D vecNorm = new Vector3D(x, y, 0);
                        vecNorm.Normalize();
                        vec = vecNorm * RadLimit;
                    }
     
                    Point3D newPoint = (Point3D)myRotateTransform.Transform(vec);
                    newPoint = transTransform.Transform(newPoint);
                   
                    disc.Add(newPoint);
                }

                if (Discs.Count == 0)
                {
                    //крышки над примитивами
                    Point3DCollection disc0 = new Point3DCollection();
                    for (int i = 0; i < disc.Count; i++)
                    {
                        Point3D pt = disc[i];
                        pt.X = 0;
                        pt.Y = 0;
                        disc0.Add(pt);
                    }
                    Discs.Add(disc0);
                }
                Discs.Add(disc);
            }

            SolidColorBrush mySolidBrush1 = new SolidColorBrush();
            mySolidBrush1.Color = Color.FromRgb(200, 110, 110);
            DiffuseMaterial FrontMaterial = new DiffuseMaterial(mySolidBrush1);

            SolidColorBrush mySolidBrush = new SolidColorBrush();
            mySolidBrush.Color = Color.FromRgb(110, 200, 110);
            DiffuseMaterial BackMaterial = new DiffuseMaterial(mySolidBrush);

            myGeometryModel.Material = FrontMaterial;
            myGeometryModel.BackMaterial = BackMaterial;

            for (int i = 1; i < Discs.Count; i++)
            {
                Point3DCollection disc1 = Discs[i - 1];
                Point3DCollection disc2 = Discs[i];

                for (int j = 1; j < disc1.Count; j++)
                {
                    Point3D p1 = disc1[j - 1];
                    Point3D p2 = disc1[j];
                    Point3D p3 = disc2[j];
                    Point3D p4 = disc2[j - 1];

                    myPositionCollection.Add(p4);
                    myPositionCollection.Add(p3);
                    myPositionCollection.Add(p1);

                    myPositionCollection.Add(p1);
                    myPositionCollection.Add(p3);
                    myPositionCollection.Add(p2);
                }
            }

            myMeshGeometry3D.Positions = myPositionCollection;
            m_bounds.Union(myMeshGeometry3D.Bounds);
            Size3D mySize = m_bounds.Size;
            string s = string.Format("X:{0},Y:{1},Z:{2}",(int)mySize.X,(int)mySize.Y,(int)mySize.Z);
            labelInfo.Content = s;
        }

        public Rect3D GetBounds()
        {
            return m_bounds;
        }

        private double IntersectsWithTriangle(Ray ray, Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D e1 = p1 - p0;
            Vector3D e2 = p2 - p0;
            Vector3D p = Vector3D.CrossProduct(ray.Direction, e2);

            double a = Vector3D.DotProduct(e1, p);
            if (MathHelper.IsZero(a))
            {
                //The ray and the triangle are parallel, they will not cross
                return -1;
            }

            double f = 1.0 / a;
            Vector3D s = ray.Origin - p0;
            double u = f * Vector3D.DotProduct(s, p);
            if ((u < 0) || (u > 1))
            {
                //Outside of the triangle
                return -1;
            }

            Vector3D q = Vector3D.CrossProduct(s, e1);
            double v = f * Vector3D.DotProduct(ray.Direction, q);
            if ((v < 0) || (u + v > 1))
            {
                //Outside of the triangle
                return -1;
            }

            //Need to check the ray intersection is in the direction of the ray
            double t = f * Vector3D.DotProduct(e2, q);
            if (t < 0)
            {
                //Outside of the triangle
                return -1;
            }
            return t;
        }

        public void UpdatePosition(Point3D intersection, double CurAngle)
        {
            Point3DCollection millPositionCollection = new Point3DCollection();
            AxisAngleRotation3D axB3d = new AxisAngleRotation3D(new Vector3D(0, 0, 1), CurAngle);
            RotateTransform3D myRotateTransform = new RotateTransform3D(axB3d);
            const double w = 3;
            {
                Point3D p2 = myRotateTransform.Transform(new Point3D(-max_x, -w, intersection.Z));
                Point3D p3 = myRotateTransform.Transform(new Point3D(-max_x, w, intersection.Z));
                millPositionCollection.Add(intersection);
                millPositionCollection.Add(p2);
                millPositionCollection.Add(p3);
            }

            {
                Point3D p2 = myRotateTransform.Transform(new Point3D(-max_x, 0, intersection.Z - w));
                Point3D p3 = myRotateTransform.Transform(new Point3D(-max_x, 0, intersection.Z + w));
                millPositionCollection.Add(intersection);
                millPositionCollection.Add(p2);
                millPositionCollection.Add(p3);
            }


            meshCube.Positions = millPositionCollection;

            Vector3D intersection1 = (Vector3D)intersection;
            intersection1.Z = 0;
            string s = string.Format("Len:{3}  X:{0},Y:{1},Z:{2}", 
                (int)intersection.X, (int)intersection.Y, (int)intersection.Z,
                (int)intersection1.Length);

            labelInfo2.Content = s;
        }

        public void UpdatePositionPlane(Point3D intersection)
        {
            Point3DCollection millPositionCollection = new Point3DCollection();
            
            const double w = 3;
            {
                Point3D p2 = intersection;
                Point3D p3 = intersection;
                p2.Z = -max_x;
                p3.Z = -max_x;

                p2.X += w;
                p3.X -= w;
                millPositionCollection.Add(intersection);
                millPositionCollection.Add(p2);
                millPositionCollection.Add(p3);
            }
            {
                Point3D p2 = intersection;
                Point3D p3 = intersection;
                p2.Z = -max_x;
                p3.Z = -max_x;

                p2.Y += w;
                p3.Y -= w;
                millPositionCollection.Add(intersection);
                millPositionCollection.Add(p2);
                millPositionCollection.Add(p3);
            }

            meshCube.Positions = millPositionCollection;

            Vector3D intersection1 = (Vector3D)intersection;
            intersection1.Z = 0;
            string s = string.Format("Len:{3}  X:{0},Y:{1},Z:{2}",
                (int)intersection.X, (int)intersection.Y, (int)intersection.Z,
                (int)intersection1.Length);

            labelInfo2.Content = s;
        }
        public void UpdatePosition2(Point3D intersection)
        {
            Point3DCollection millPositionCollection = new Point3DCollection();
            Vector3D vecTransform = new Vector3D(intersection.X, intersection.Y, 0);
            vecTransform.Normalize();
            vecTransform *= 50;
            TranslateTransform3D myRotateTransform = new TranslateTransform3D(vecTransform);
            {
                Point3D p2 = myRotateTransform.Transform(intersection);
                Point3D p3 = p2;
                p3.Z += 20;
                millPositionCollection.Add(intersection);
                millPositionCollection.Add(p2);
                millPositionCollection.Add(p3);
            }

            meshCube.Positions = millPositionCollection;
            Vector3D intersection1 = (Vector3D)intersection;
            intersection1.Z = 0;
            string s = string.Format("Len:{3}  X:{0},Y:{1},Z:{2}",
                (int)intersection.X, (int)intersection.Y, (int)intersection.Z,
                (int)intersection1.Length);

            labelInfo2.Content = s;
        }

        public const double max_x = 100;

        public Ray GetRay(double angle, double Z, Vector3D offset)
        {
            AxisAngleRotation3D myRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle);
            RotateTransform3D myRotateTransform = new RotateTransform3D(myRotation);

            Point3D pt1 = myRotateTransform.Transform(new Point3D(-max_x, 0, Z) + offset);
            Point3D pt2 = myRotateTransform.Transform(new Point3D(0, 0, Z) + offset);
            Vector3D dir = pt2 - pt1;

            Ray ray = new Ray(pt1, dir);
            return ray;
        }
        public Ray GetRayPlane(double X, double Y)
        {
            Point3D pt1 = new Point3D(X, Y, -max_x);
            Point3D pt2 = new Point3D(X, Y, 0);
            Vector3D dir = pt2 - pt1;

            Ray ray = new Ray(pt1, dir);
            return ray;
        }
        
        public enum IntersectionType : int
        {
            E_NOTHING,
            E_INTERSECTION,
            E_OUT
        };

        public static Point3D m_dFreza = new Point3D(3, 3, 3);
        public IntersectionType GetIntersection(double angle, double Z, out Point3D intersection)
        {
            IntersectionType ret = IntersectionType.E_OUT;
            intersection = new Point3D();
            AxisAngleRotation3D myRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle);
            RotateTransform3D myRotateTransform = new RotateTransform3D(myRotation);

            
            Ray[] rays = new Ray[4];

            Vector3D offset = new Vector3D(0, -m_dFreza.Y/2, 0);
            rays[0] = GetRay(angle, Z, offset);

            offset = new Vector3D(0, m_dFreza.Y/2, 0);
            rays[1] = GetRay(angle, Z, offset);

            offset = new Vector3D(0, 0, -m_dFreza.Z/2);
            rays[2] = GetRay(angle, Z, offset);

            offset = new Vector3D(0, 0, m_dFreza.Z/2);
            rays[3] = GetRay(angle, Z, offset);

            offset = new Vector3D(0, 0, 0);
            Ray ray_base = GetRay(angle, Z, offset);
            

            double dist = -1;

            for (int i = 0; i < m_Model3DGroup.Children.Count; i++)
            {
                GeometryModel3D model = m_Model3DGroup.Children[i] as GeometryModel3D;

                if (model == null || model == meshCubeModel)
                {
                    continue;
                }
                MeshGeometry3D geometry = model.Geometry as MeshGeometry3D;
                if (geometry == null)
                {
                    continue;
                }

                if (!model.Bounds.Contains(new Point3D(0, 0, Z)))
                {
                    continue;
                }
                ret = IntersectionType.E_NOTHING;
                Point3DCollection points = geometry.Positions;

                for (int j = 0; j < points.Count; j += 3)
                {
                    if (points[j].Z > (Z+3) && points[j + 1].Z > (Z+3) && points[j + 2].Z > (Z+3))
                    {
                        break;
                    }
                    if (points[j].Z < (Z - 3) && points[j + 1].Z < (Z - 3) && points[j + 2].Z < (Z - 3))
                    {
                        continue;
                    }
                    
                    for (int k = 0; k < rays.Length; k++)
                    {
                        double t = IntersectsWithTriangle(rays[k], points[j], points[j + 1], points[j + 2]);
                        if (t < 0)
                        {
                            continue;
                        }
                        if (t > dist && t < max_x)
                        {
                            dist = t;
                        }
                    }
                }
            }

            if (dist >= 0)
            {
                intersection = ray_base.Origin + dist * ray_base.Direction;
                return IntersectionType.E_INTERSECTION;
            }

            return ret;
        }
        public IntersectionType GetIntersectionPlane(double X, double Y, out Point3D intersection)
        {
            IntersectionType ret = IntersectionType.E_OUT;
            intersection = new Point3D();


            Ray[] rays = new Ray[4];

            double x_offset = m_dFreza.X / 2;
            double y_offset = m_dFreza.Y / 2;
            rays[0] = GetRayPlane(X, Y + y_offset);
            rays[1] = GetRayPlane(X + x_offset, Y);
            rays[2] = GetRayPlane(X, Y - y_offset);
            rays[3] = GetRayPlane(X - x_offset, Y);
            Ray ray_base = GetRayPlane(X, Y);


            double dist = Double.MaxValue;

            for (int i = 0; i < m_Model3DGroup.Children.Count; i++)
            {
                GeometryModel3D model = m_Model3DGroup.Children[i] as GeometryModel3D;

                if (model == null || model == meshCubeModel)
                {
                    continue;
                }
                MeshGeometry3D geometry = model.Geometry as MeshGeometry3D;
                if (geometry == null)
                {
                    continue;
                }

                //if (!model.Bounds.Contains(new Point3D(X, Y, 0)))
                //{
                //    continue;
                //}
                ret = IntersectionType.E_NOTHING;
                Point3DCollection points = geometry.Positions;

                for (int j = 0; j < points.Count; j += 3)
                {
                     for (int k = 0; k < rays.Length; k++)
                    {
                        double t = IntersectsWithTriangle(rays[k], points[j], points[j + 1], points[j + 2]);
                        if (t < 0)
                        {
                            continue;
                        }
                        if (t < dist && t >= 0)
                        {
                            dist = t;
                        }
                    }
                }
            }

            if (dist >= 0)
            {
                intersection = ray_base.Origin + dist * ray_base.Direction;
                return IntersectionType.E_INTERSECTION;
            }

            return ret;
        }
        private void sliderA_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_axA3d.Angle = sliderA.Value;
        }
        
        private void sliderB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_axB3d.Angle = sliderB.Value;
        }

        private void sliderZ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_trans3d.OffsetZ = sliderZ.Value;
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            m_axB3d.Angle += e.Delta/12;
        }


        private void myViewport3D_MouseDown(object sender, MouseButtonEventArgs args)
        {
            Point mousePos = args.GetPosition(myViewport3D);
            PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            VisualTreeHelper.HitTest(myViewport3D, null, ResultCallback, hitParams);
        }
        public HitTestResultBehavior ResultCallback(HitTestResult result)
        {
            // Did we hit 3D?
            RayHitTestResult rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                // Did we hit a MeshGeometry3D?
                RayMeshGeometry3DHitTestResult rayMeshResult =
                    rayResult as RayMeshGeometry3DHitTestResult;

                if (rayMeshResult != null)
                {
                    Point3D intersect = rayMeshResult.PointHit;
                    intersect = m_Model3DGroup.Transform.Inverse.Transform(intersect);
                    UpdatePosition2(intersect);
                    return HitTestResultBehavior.Stop;
                }
            }

            return HitTestResultBehavior.Continue;
        }
    }
}
