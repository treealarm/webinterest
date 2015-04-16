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

        public UserControlFor3D()
        {
            InitializeComponent();
            
            m_Model3DGroup = GetNewGroup();
            myModelVisual3D.Content = m_Model3DGroup;

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
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
        double GetZoomTransform(int Z, List<double> listLong, int len)
        {
            if (listLong.Count == 0)
            {
                return 1;
            }
            int pos = Z * (listLong.Count-1) / len;
            return listLong[pos];
        }
        public void Calculate(List<Point> listCross, List<double> listLong, int pos, int len, double angle)
        {
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
            }
            GeometryModel3D myGeometryModel = new GeometryModel3D();
            m_Model3DGroup.Children.Add(myGeometryModel);
            m_Model3DGroup.Children.Add(meshCubeModel);

            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();
            
            Point3DCollection myPositionCollection = new Point3DCollection();
            
            myGeometryModel.Geometry = myMeshGeometry3D;

            List<Point3DCollection>  Discs = new List<Point3DCollection>();
            double cur_angle = 0;
            double ZIncrement = 1;
            if (MathHelper.IsZero(angle) && listLong.Count == 0)
            {
                ZIncrement = len;
            }

            for (double Z = pos; Z <= pos + len; Z += ZIncrement)
            {
                Point3DCollection disc = new Point3DCollection();
                cur_angle += angle;

                AxisAngleRotation3D myRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), cur_angle);
                RotateTransform3D myRotateTransform = new RotateTransform3D(myRotation);

                double Zoom = GetZoomTransform((int)(Z - pos), listLong, len);
                ScaleTransform3D ScaleTrans = new ScaleTransform3D(new Vector3D(Zoom, Zoom, 0), new Point3D(0, 0, Z));

                if (listCross.First() != listCross.Last())
                {//make closed loop
                    listCross.Add(listCross.First());
                }

                for (int i = 0; i < listCross.Count; i++ )
                {
                    double x = listCross[i].X;
                    double y = listCross[i].Y;

                    Point3D newPoint = myRotateTransform.Transform(new Point3D(x, y, Z));
                    newPoint = ScaleTrans.Transform(newPoint);
                   
                    disc.Add(newPoint);
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

        double m_CurAngle = 0;
        double m_CurZ = 0.0001;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            return;
            Point3D intersection = new Point3D();
            if (!GetIntersection(m_CurAngle, m_CurZ, out intersection))
            {
                m_CurAngle = 0;
                m_CurZ = 0.0001;
                return;
            }

            UpdatePosition(intersection, m_CurAngle);

            m_CurAngle += 1;
            if(m_CurAngle >= 360.0)
            {
                m_CurZ += 10;
                m_CurAngle = 0;
            }
        }

        public void UpdatePosition(Point3D intersection, double CurAngle)
        {
            Point3DCollection millPositionCollection = new Point3DCollection();
            AxisAngleRotation3D axB3d = new AxisAngleRotation3D(new Vector3D(0, 0, 1), CurAngle);
            RotateTransform3D myRotateTransform = new RotateTransform3D(axB3d);
            Point3D p2 = myRotateTransform.Transform(new Point3D(-100, -10, intersection.Z));
            Point3D p3 = myRotateTransform.Transform(new Point3D(-100, 10, intersection.Z));
            millPositionCollection.Add(intersection);
            millPositionCollection.Add(p2);
            millPositionCollection.Add(p3);

            meshCube.Positions = millPositionCollection;
        }


        public bool GetIntersection(double angle, double Z, out Point3D intersection)
        {
            AxisAngleRotation3D myRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle);
            RotateTransform3D myRotateTransform = new RotateTransform3D(myRotation);
            
            Point3D newPoint = myRotateTransform.Transform(new Point3D(-100, -1.5, Z));
            Ray ray = new Ray(newPoint, new Vector3D(-newPoint.X, -newPoint.Y, -newPoint.Z));
            
            newPoint = myRotateTransform.Transform(new Point3D(-100, 1.5, Z));
            Ray ray1 = new Ray(newPoint, new Vector3D(-newPoint.X, -newPoint.Y, -newPoint.Z));

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
                Point3DCollection points = geometry.Positions;
                
                for (int j = 0; j < points.Count; j += 3)
                {
                    Plane plane = new Plane(points[j], points[j + 1], points[j + 2]);
                    double t = IntersectsWithTriangle(ray, points[j], points[j + 1], points[j + 2]);
                    double t1 = IntersectsWithTriangle(ray1, points[j], points[j + 1], points[j + 2]);
                    if (t > 0 || t1 > 0)
                    {
                        if (t > t1)
                        {
                            intersection = ray.Origin + t * ray.Direction;
                        }
                        else
                        {
                            intersection = ray1.Origin + t1 * ray1.Direction;
                        }
                        return true;
                    }

                }
            }
            intersection = new Point3D();
            return false;
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
    }
}
