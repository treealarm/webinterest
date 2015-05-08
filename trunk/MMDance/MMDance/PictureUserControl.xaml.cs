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
using Microsoft.Win32;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Specialized;
using System.ComponentModel;
using Kit3D;
using System.Xml;

namespace MMDance
{
    /// <summary>
    /// Interaction logic for PictureUserControl.xaml
    /// </summary>
    public partial class PictureUserControl : UserControl
    {
        ProfileElementList m_ProfileData = new ProfileElementList();
        public PictureUserControl()
        {
            InitializeComponent();
            try
            {
                m_ProfileData = ProfileElementSerializer.DeserializeObject(Properties.Settings.Default.ProfileDataSource);
            }
            catch (Exception e)
            {
            }
            if (m_ProfileData == null)
            {
                m_ProfileData = new ProfileElementList();
            }

            ProfileDataGrid.DataContext = m_ProfileData;
        }

        private void Click_FileName(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "xml|*.xml;";
            if (op.ShowDialog() == true)
            {
                ProfileElement element = ((FrameworkElement)sender).DataContext as ProfileElement;
                element.FileName = op.FileName;
                ProfileDataGrid.CommitEdit();
            }
        }

        private void Click_FileNameCurve(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "xml|*.xml;";
            if (op.ShowDialog() == true)
            {
                ProfileElement element = ((FrameworkElement)sender).DataContext as ProfileElement;
                element.FileNameCurve = op.FileName;
                ProfileDataGrid.CommitEdit();
            }
        }

        private void ProfileDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            DataTemplate temp = ProfileDataGrid.RowDetailsTemplate;
            ProfileDataGrid.RowDetailsTemplate = null;
            ProfileDataGrid.RowDetailsTemplate = temp;
        }

        List<Point> GetBezierApproximation(Point[] controlPoints, int outputSegmentCount)
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i <= outputSegmentCount; i++)
            {
                double t = (double)i / outputSegmentCount;
                Point pt = GetBezierPoint(t, controlPoints, 0, controlPoints.Length);
                points.Add(pt);
            }
            return points;
        }

        Point GetBezierPoint(double t, Point[] controlPoints, int index, int count)
        {
            if (count == 1)
                return controlPoints[index];
            var P0 = GetBezierPoint(t, controlPoints, index, count - 1);
            var P1 = GetBezierPoint(t, controlPoints, index + 1, count - 1);
            return new Point((1 - t) * P0.X + t * P1.X, (1 - t) * P0.Y + t * P1.Y);
        }

        public void UpdateProfileResult()
        {

            MainWindow wnd = MainWindow.GetMainWnd();
            m_UserControlFor3D.m_listLimits = wnd.GetLimitProfileCurvePoints();
            m_UserControlFor3D.m_listCurve = wnd.GetLongProfileCurvePoints();

            int cur_pos = 0;
            for (int i = 0; i < m_ProfileData.Count; i++)
            {
                ProfileElement element = m_ProfileData[i];
                if (element.Length <= 0)
                {
                    continue;
                }
                List<Point> list = ListPoint.DeserializeObject(element.FileName);
                
                m_UserControlFor3D.Calculate(list, cur_pos, element.Length, element.Angle);
                cur_pos += element.Length;
            }
        }

        private void ProfileDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProfileElement element = ProfileDataGrid.SelectedItem as ProfileElement;
            if (element == null || element.Length <= 0)
            {
                return;
            }
            List<Point> list = ListPoint.DeserializeObject(element.FileName);
            
            if (list.Count == 0)
            {
                return;
            }
            m_UserControlFor3D.m_listLimits = null;
            m_UserControlFor3D.Calculate(list, 0, element.Length, element.Angle);
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ProfileDataSource = ProfileElementSerializer.SerializeObject(m_ProfileData);
            Properties.Settings.Default.Save();
            UpdateProfileResult();
        }
    }
}
