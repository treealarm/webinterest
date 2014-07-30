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
            myPCamera.Position = new Point3D(0, 0, -2);

            // Specify the direction that the camera is pointing.
            myPCamera.LookDirection = new Vector3D(0, 0, -1);

            // Define camera's horizontal field of view in degrees.
            myPCamera.FieldOfView = 60;

            // Asign the camera to the viewport
            myViewport3D.Camera = myPCamera;
            // Define the lights cast in the scene. Without light, the 3D object cannot  
            // be seen. Note: to illuminate an object from additional directions, create  
            // additional lights.
            DirectionalLight myDirectionalLight = new DirectionalLight();
            myDirectionalLight.Color = Colors.White;
            myDirectionalLight.Direction = new Vector3D(-0.61, -0.5, -0.61);

            myModel3DGroup.Children.Add(myDirectionalLight);

            // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet  
            // is created.
            //MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();


            //// Create a collection of vertex positions for the MeshGeometry3D. 
            //Point3DCollection myPositionCollection = new Point3DCollection();
            //myPositionCollection.Add(new Point3D(-0.5, -0.5, 0.5));
            //myPositionCollection.Add(new Point3D(0.5, -0.5, 0.5));
            //myPositionCollection.Add(new Point3D(0.5, 0.5, 0.5));
            //myPositionCollection.Add(new Point3D(0.5, 0.5, 0.5));
            //myPositionCollection.Add(new Point3D(-0.5, 0.5, 0.5));
            //myPositionCollection.Add(new Point3D(-0.5, -0.5, 0.5));
            //myMeshGeometry3D.Positions = myPositionCollection;


            //// Create a collection of triangle indices for the MeshGeometry3D.
            //Int32Collection myTriangleIndicesCollection = new Int32Collection();
            //myTriangleIndicesCollection.Add(0);
            //myTriangleIndicesCollection.Add(1);
            //myTriangleIndicesCollection.Add(2);
            //myTriangleIndicesCollection.Add(3);
            //myTriangleIndicesCollection.Add(4);
            //myTriangleIndicesCollection.Add(5);
            //myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            //// Apply the mesh to the geometry model.
            //myGeometryModel.Geometry = myMeshGeometry3D;

            // The material specifies the material applied to the 3D object. In this sample a   
            // linear gradient covers the surface of the 3D object. 

            // Create a horizontal linear gradient with four stops.   
            LinearGradientBrush myHorizontalGradient = new LinearGradientBrush();
            myHorizontalGradient.StartPoint = new Point(0, 0.5);
            myHorizontalGradient.EndPoint = new Point(1, 0.5);
            //myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            //myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
            //myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.75));
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1.0));

            // Define material and apply to the mesh geometries.
            DiffuseMaterial myMaterial = new DiffuseMaterial(myHorizontalGradient);
            myGeometryModel.Material = myMaterial;

            // Apply a transform to the object. In this sample, a rotation transform is applied,   
            // rendering the 3D object rotated.
            RotateTransform3D myRotateTransform3D = new RotateTransform3D();
            AxisAngleRotation3D myAxisAngleRotation3d = new AxisAngleRotation3D();
            myAxisAngleRotation3d.Axis = new Vector3D(0, 3, 0);
            myAxisAngleRotation3d.Angle = 40;
            myRotateTransform3D.Rotation = myAxisAngleRotation3d;
            myGeometryModel.Transform = myRotateTransform3D;

            // Add the geometry model to the model group.
            myModel3DGroup.Children.Add(myGeometryModel);

            // Add the group of models to the ModelVisual3d.
            myModelVisual3D.Content = myModel3DGroup;

            // 
            myViewport3D.Children.Add(myModelVisual3D);

            // Apply the viewport to the page so it will be rendered. 
            this.Content = myViewport3D;
        }
        
        
        double[] R = new double[] { 20, 10, 5, 1 };
        double gamma = Math.PI / (2 * 3);
        double Radius = 20;
        public void Calculate()
        {
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();


            // Create a collection of vertex positions for the MeshGeometry3D. 
            Point3DCollection myPositionCollection = new Point3DCollection();
            //myPositionCollection.Add(new Point3D(-0.5, -0.5, 0.5));
            //myPositionCollection.Add(new Point3D(0.5, -0.5, 0.5));
            //myPositionCollection.Add(new Point3D(0.5, 0.5, 0.5));
            //myPositionCollection.Add(new Point3D(0.5, 0.5, 0.5));
            //myPositionCollection.Add(new Point3D(-0.5, 0.5, 0.5));
            //myPositionCollection.Add(new Point3D(-0.5, -0.5, 0.5));
            myMeshGeometry3D.Positions = myPositionCollection;


            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();
            //myTriangleIndicesCollection.Add(0);
            //myTriangleIndicesCollection.Add(1);
            //myTriangleIndicesCollection.Add(2);
            //myTriangleIndicesCollection.Add(3);
            //myTriangleIndicesCollection.Add(4);
            //myTriangleIndicesCollection.Add(5);
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            // Apply the mesh to the geometry model.
            myGeometryModel.Geometry = myMeshGeometry3D;
            int index = 0;
            myPositionCollection.Add(new Point3D(0, 0, 0));
            myPositionCollection.Add(new Point3D(1, 0, 0));
            myPositionCollection.Add(new Point3D(1, 1, 0));

            for (int j = 0; j < 3; j++)
            {
                myTriangleIndicesCollection.Add(index); index++;
            }

            for (int i = 1; i < R.Length; i++)
            {
                Points p = myFunction(i-1);
                Points p1 = myFunction(i);

                //myPositionCollection.Add(new Point3D(p.X1, p.Y1, p.Z1));
                //myPositionCollection.Add(new Point3D(p1.X1, p1.Y1, p1.Z1));
                //myPositionCollection.Add(new Point3D(p.X2, p.Y2, p.Z2));

                //myPositionCollection.Add(new Point3D(p.X2, p.Y2, p.Z2));
                //myPositionCollection.Add(new Point3D(p1.X1, p1.Y1, p1.Z1));
                //myPositionCollection.Add(new Point3D(p1.X2, p1.Y2, p1.Z2));

                //for (int j = 0; j < 6; j++)
                //{
                //    myTriangleIndicesCollection.Add(index); index++;
                //}

                double L1 = Math.Sqrt(Math.Pow(p.Z1 - p.Z2, 2) +
                    Math.Pow(p.Y1 - p.Y2,2)  + Math.Pow(p.X1 - p.X2,2));
                
                double L2 = Math.Sqrt(Math.Pow(p1.Z1 - p1.Z2, 2) +
                    Math.Pow(p1.Y1 - p1.Y2, 2) + Math.Pow(p1.X1 - p1.X2, 2));

                double L3 = Math.Sqrt(Math.Pow(p.Z1 - p1.Z1, 2) +
                    Math.Pow(p.Y1 - p1.Y1, 2) + Math.Pow(p.X1 - p1.X1, 2));

                string s = string.Format("L1:{0:00.00}", L1);
            }
        }
        Points myFunction(int i)
        {
            Points p = new Points();

            p.X1 = R[i];
            p.X2 = R[i] * Math.Cos(gamma);

            p.Y1 = 0;
            p.Y2 = Math.Sqrt(R[i] * R[i] - p.X1 * p.X1);

            p.Z1 = Math.Sqrt(Radius * Radius - p.X1 * p.X1 - p.Y1 * p.Y1);
            p.Z2 = Math.Sqrt(Radius * Radius - p.X2 * p.X2 - p.Y2 * p.Y2);

            return p;
        }
    }
}
