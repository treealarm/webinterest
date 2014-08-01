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

namespace RaschetSphery1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GeometryModel3D myGeometryModel = new GeometryModel3D();
        public MainWindow()
        {
            InitializeComponent();
            Calculate();
            // Declare scene objects.
            Viewport3D myViewport3D = new Viewport3D();
            Model3DGroup myModel3DGroup = new Model3DGroup();
            
            ModelVisual3D myModelVisual3D = new ModelVisual3D();
            // Defines the camera used to view the 3D object. In order to view the 3D object, 
            // the camera must be positioned and pointed such that the object is within view  
            // of the camera.
            PerspectiveCamera myPCamera = new PerspectiveCamera();

            // Specify where in the 3D scene the camera is.
            myPCamera.Position = new Point3D(0, 0, -80);

            // Specify the direction that the camera is pointing.
            myPCamera.LookDirection = new Vector3D(0, 0, 1);

            // Define camera's horizontal field of view in degrees.
            myPCamera.FieldOfView = 60;

            // Asign the camera to the viewport
            myViewport3D.Camera = myPCamera;
            // Define the lights cast in the scene. Without light, the 3D object cannot  
            // be seen. Note: to illuminate an object from additional directions, create  
            // additional lights.
            AmbientLight _ambLight = new AmbientLight(System.Windows.Media.Brushes.LightGreen.Color);
            myModel3DGroup.Children.Add(_ambLight);




            // Create a horizontal linear gradient with four stops.   
            SolidColorBrush myHorizontalGradient = new SolidColorBrush(Color.FromRgb(222,0,0));

            // Define material and apply to the mesh geometries.
            DiffuseMaterial myMaterial = new DiffuseMaterial(myHorizontalGradient);
            myGeometryModel.Material = myMaterial;


            // Add the geometry model to the model group.
            myModel3DGroup.Children.Add(myGeometryModel);

            // Add the group of models to the ModelVisual3d.
            myModelVisual3D.Content = myModel3DGroup;

            // 
            myViewport3D.Children.Add(myModelVisual3D);

            // Apply the viewport to the page so it will be rendered. 
            this.Content = myViewport3D;
        }
        
        double Radius = 20;
        double step = 5;
        public void Calculate()
        {
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();


            // Create a collection of vertex positions for the MeshGeometry3D. 
            Point3DCollection myPositionCollection = new Point3DCollection();

            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();
            

            // Apply the mesh to the geometry model.
            myGeometryModel.Geometry = myMeshGeometry3D;

            Point3DCollection[] Discs = new Point3DCollection[Convert.ToInt32(Radius/step)+1];
            for (double Z = 0; Z <= Radius; Z += step)
            {
                Point3DCollection disc = new Point3DCollection();
                double r1 = Math.Sqrt(Radius * Radius - Math.Pow(Z, 2));
                for (double alfa = 0; alfa <= 2 * Math.PI; alfa += Math.PI/10)
                {
                    double Y = r1 * Math.Sin(alfa);
                    double X = Math.Sqrt(Radius * Radius - Math.Pow(Y, 2) - Math.Pow(Z, 2));
                    disc.Add(new Point3D(X, Y, Z));
                }
                for (double alfa = 0; alfa <= 2 * Math.PI; alfa += Math.PI / 10)
                {
                    double Y = r1 * Math.Sin(alfa);
                    double X = -Math.Sqrt(Radius * Radius - Math.Pow(Y, 2) - Math.Pow(Z, 2));
                    disc.Add(new Point3D(X, Y, Z));
                }
 
                Discs[Convert.ToInt32(Z / step)] = disc;
            }


            for (int i = 1; i < Discs.Length; i++)
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

    }
}
