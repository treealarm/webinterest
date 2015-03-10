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

namespace MMDance
{
    /// <summary>
    /// Interaction logic for UserControlFor3D.xaml
    /// </summary>
    public partial class UserControlFor3D : UserControl
    {
        AxisAngleRotation3D ax3d = new AxisAngleRotation3D();
        ModelVisual3D myModelVisual3D = new ModelVisual3D();
        Model3DGroup m_Model3DGroup = new Model3DGroup();
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ax3d.Angle += 1;
        }

        public UserControlFor3D()
        {
            InitializeComponent();
            
            // Defines the camera used to view the 3D object. In order to view the 3D object, 
            // the camera must be positioned and pointed such that the object is within view  
            // of the camera.
            OrthographicCamera myPCamera = new OrthographicCamera(new Point3D(0, 50, -200), 
                new Vector3D(0, 0, 1), 
                new Vector3D(0, 1, 0), 600);

            // Asign the camera to the viewport
            myViewport3D.Camera = myPCamera;

            // 
            myViewport3D.Children.Add(myModelVisual3D);
            m_Model3DGroup = GetNewGroup();
            myModelVisual3D.Content = m_Model3DGroup;

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 30);
            dispatcherTimer.Start();
        }

        Model3DGroup GetNewGroup()
        {
            Model3DGroup myModel3DGroup = new Model3DGroup();
            // Define the lights cast in the scene. Without light, the 3D object cannot  
            // be seen. Note: to illuminate an object from additional directions, create  
            // additional lights.
            AmbientLight _ambLight = new AmbientLight(System.Windows.Media.Brushes.White.Color);
            DirectionalLight _dirLight = new DirectionalLight();
            _dirLight.Color = System.Windows.Media.Brushes.White.Color;
            _dirLight.Direction = new System.Windows.Media.Media3D.Vector3D(0, -1, 0);

            //myModel3DGroup.Children.Add(_ambLight);
            myModel3DGroup.Children.Add(_dirLight);

            _dirLight = new DirectionalLight();
            _dirLight.Color = System.Windows.Media.Brushes.White.Color;
            _dirLight.Direction = new System.Windows.Media.Media3D.Vector3D(0, 0, 1);
            myModel3DGroup.Children.Add(_dirLight);
            return myModel3DGroup;
            
        }
        public void Calculate(List<Point> list, int pos, int len)
        {
            if (pos == 0)
            {
                m_Model3DGroup = GetNewGroup();
                myModelVisual3D.Content = m_Model3DGroup;
                ax3d = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 1);
                RotateTransform3D myRotateTransform = new RotateTransform3D(ax3d);
                m_Model3DGroup.Transform = myRotateTransform;
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

            List<Point3DCollection>  Discs = new List<Point3DCollection>();
            for (double Z = pos; Z < pos+len; Z+=1 )
            {
                Point3DCollection disc = new Point3DCollection();

                for (int i = 0; i < list.Count; i++ )
                {
                    double X = list[i].X;
                    double Y = list[i].Y;
                    disc.Add(new Point3D(X, Y, Z));
                }


                Discs.Add(disc);
            }

            SolidColorBrush myHorizontalGradient = new SolidColorBrush();
            myHorizontalGradient.Color = Color.FromRgb(127, 127, 127);
            

            DiffuseMaterial myDiffuseMaterial = new DiffuseMaterial(myHorizontalGradient);
            MaterialGroup myMaterialGroup = new MaterialGroup();
            myMaterialGroup.Children.Add(myDiffuseMaterial);

            myGeometryModel.Material = myMaterialGroup;
            myGeometryModel.BackMaterial = myMaterialGroup;

            Triangle t1 = null;
            for (int i = 1; i < Discs.Count; i++)
            {
                Point3DCollection disc1 = Discs[i - 1];
                Point3DCollection disc2 = Discs[i];
                t1 = null;
                for (int j = 1; j < disc1.Count; j++)
                {
                    Point3D p1 = disc1[j - 1];
                    Point3D p2 = disc1[j];
                    Point3D p3 = disc2[j];
                    Point3D p4 = disc2[j - 1];

                    if (t1 != null)
                    {
                        Triangle t2 = new Triangle(p1, p2, p3);
                        double angle = t2.GetAngle(t1);
                    }
                    t1 = new Triangle(p1, p2, p3);

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
    }
}
