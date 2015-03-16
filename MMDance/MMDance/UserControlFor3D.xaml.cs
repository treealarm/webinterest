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
using Kit3D.Windows.Media.Media3D;
using Kit3D.Math;

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
        

        ModelVisual3D myModelVisual3D = new ModelVisual3D();
        Model3DGroup m_Model3DGroup = new Model3DGroup();
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //ax3d.Angle += 1;
        }

        public UserControlFor3D()
        {
            InitializeComponent();
            
            // Defines the camera used to view the 3D object. In order to view the 3D object, 
            // the camera must be positioned and pointed such that the object is within view  
            // of the camera.
            OrthographicCamera myPCamera = new OrthographicCamera(new Point3D(0, 0, -200), 
                new Vector3D(0, -0.1, 1), 
                new Vector3D(0, 1, 0), 600);

            // Asign the camera to the viewport
            myViewport3D.Camera = myPCamera;

            // 
            myViewport3D.Children.Add(myModelVisual3D);
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
        public void Calculate(List<Point> list, int pos, int len)
        {
            if (pos == 0)
            {
                m_Model3DGroup = GetNewGroup();
                myModelVisual3D.Content = m_Model3DGroup;

                Transform3DGroup transGroup = new Transform3DGroup();
                
                m_axA3d = new AxisAngleRotation3D(new Vector3D(0, 1, 0), sliderA.Value);
                RotateTransform3D myRotateTransform = new RotateTransform3D(m_axA3d);
                transGroup.Children.Add(myRotateTransform);

                m_axB3d = new AxisAngleRotation3D(new Vector3D(1, 0, 0), sliderB.Value);
                myRotateTransform = new RotateTransform3D(m_axB3d);
                transGroup.Children.Add(myRotateTransform);

                

                m_trans3d = new TranslateTransform3D(new Vector3D(0, 0, 0));
                transGroup.Children.Add(m_trans3d);

                m_Model3DGroup.Transform = transGroup;
            }
            GeometryModel3D myGeometryModel = new GeometryModel3D();
            m_Model3DGroup.Children.Add(myGeometryModel);

            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();
            // Create a collection of vertex positions for the MeshGeometry3D. 
            Point3DCollection myPositionCollection = new Point3DCollection();

            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();


            // Apply the mesh to the geometry model.
            myGeometryModel.Geometry = myMeshGeometry3D;

            double x_origin = 0;
            double y_origin = 0;
            for (int i = 0; i < list.Count; i++)
            {
                x_origin += list[i].X;
                y_origin += list[i].Y;
            }
            x_origin /= list.Count;
            y_origin /= list.Count;
            List<Point3DCollection>  Discs = new List<Point3DCollection>();
            double angle = 0;
            for (double Z = pos; Z < pos+len; Z+=1 )
            {
                Point3DCollection disc = new Point3DCollection();
                angle += 1;

                AxisAngleRotation3D myRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle);
                RotateTransform3D myRotateTransform = new RotateTransform3D(myRotation);

                for (int i = 0; i < list.Count; i++ )
                {
                    double x = list[i].X - x_origin;
                    double y = list[i].Y - y_origin;

                    Point3D newPoint = myRotateTransform.Transform(new Point3D(x, y, Z));
                   
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

            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;
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

        private void sliderA_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_axA3d.Angle = sliderA.Value;
        }
        
        private void sliderB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_axB3d.Angle = sliderB.Value;
        }

        private void sliderX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_trans3d.OffsetX = sliderX.Value;
        }        
    }
}
