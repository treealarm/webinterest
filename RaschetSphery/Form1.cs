using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RaschetSphery
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            for (int i = 1; i < R.Length; i++)
            {
                Points p = myFunction(i-1);
                Points p1 = myFunction(i);
                
                double L1 = Math.Sqrt(Math.Pow(p.Z1 - p.Z2, 2) +
                    Math.Pow(p.Y1 - p.Y2,2)  + Math.Pow(p.X1 - p.X2,2));
                
                double L2 = Math.Sqrt(Math.Pow(p1.Z1 - p1.Z2, 2) +
                    Math.Pow(p1.Y1 - p1.Y2, 2) + Math.Pow(p1.X1 - p1.X2, 2));

                double L3 = Math.Sqrt(Math.Pow(p.Z1 - p1.Z1, 2) +
                    Math.Pow(p.Y1 - p1.Y1, 2) + Math.Pow(p.X1 - p1.X1, 2));

                string s = string.Format("L1:{0:00.00}", L1);
                textBox1.AppendText(s + Environment.NewLine);

                s = string.Format("L2:{0:00.00}", L2);
                textBox1.AppendText(s + Environment.NewLine);

                s = string.Format("L3:{0:00.00}", L3);
                textBox1.AppendText(s + Environment.NewLine);
                textBox1.AppendText(Environment.NewLine);
            }
        }
        double[] R = new double[] { 20, 10, 5 ,1};
        double gamma = Math.PI / (2 * 3);
        double Radius = 20;

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
