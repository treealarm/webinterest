﻿using System;
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
            myPCamera.Position = new Point3D(0, 0, 20);

            // Specify the direction that the camera is pointing.
            myPCamera.LookDirection = new Vector3D(0, 0, -1);

            // Define camera's horizontal field of view in degrees.
            myPCamera.FieldOfView = 60;

            // Asign the camera to the viewport
            myViewport3D.Camera = myPCamera;
            // Define the lights cast in the scene. Without light, the 3D object cannot  
            // be seen. Note: to illuminate an object from additional directions, create  
            // additional lights.
            AmbientLight _ambLight = new AmbientLight(System.Windows.Media.Brushes.DarkBlue.Color);
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
        
        
        double[] R = new double[] { 20, 10, 5, 1 };
        double gamma = Math.PI / (2 * 3);
        double Radius = 20;
        public void Calculate()
        {
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();


            // Create a collection of vertex positions for the MeshGeometry3D. 
            Point3DCollection myPositionCollection = new Point3DCollection();
            myMeshGeometry3D.Positions = myPositionCollection;


            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();
            

            // Apply the mesh to the geometry model.
            myGeometryModel.Geometry = myMeshGeometry3D;
            int index = 0;

            for (int i = 1; i < R.Length; i++)
            {
                Points p = myFunction(i-1);
                Points p1 = myFunction(i);


                
                myPositionCollection.Add(new Point3D(0, 1, 0));
                myPositionCollection.Add(new Point3D(0, 0, 0));
                myPositionCollection.Add(new Point3D(1, 1, 0));


                double L1 = Math.Sqrt(Math.Pow(p.Z1 - p.Z2, 2) +
                    Math.Pow(p.Y1 - p.Y2,2)  + Math.Pow(p.X1 - p.X2,2));
                
                double L2 = Math.Sqrt(Math.Pow(p1.Z1 - p1.Z2, 2) +
                    Math.Pow(p1.Y1 - p1.Y2, 2) + Math.Pow(p1.X1 - p1.X2, 2));

                double L3 = Math.Sqrt(Math.Pow(p.Z1 - p1.Z1, 2) +
                    Math.Pow(p.Y1 - p1.Y1, 2) + Math.Pow(p.X1 - p1.X1, 2));

                string s = string.Format("L1:{0:00.00}", L1);
            }
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;
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
