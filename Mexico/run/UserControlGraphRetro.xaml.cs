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
using System.Windows.Forms.DataVisualization.Charting;
using System.Reflection;
using System.Collections.ObjectModel;

namespace StateStat
{
    /// <summary>
    /// Interaction logic for UserControlGraph.xaml
    /// </summary>
    public partial class UserControlGraphRetro : UserControl
    {
        System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();

        const int VIEW_X_SIZE = 20;
        public UserControlGraphRetro()
        {
            InitializeComponent();
        }

        public static ObservableCollection<ObjectStates> m_DataSource = new ObservableCollection<ObjectStates>();


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
           
        }

        public void CleanUpdateGraphFromDB(DateTime from, DateTime to)
        {

        }

        public void SetFilter(string sFilter)
        {
            if (string.IsNullOrEmpty(sFilter))
            {
                RecordListView.ItemsSource = m_DataSource;
            }
            else
            {
                RecordListView.ItemsSource = m_DataSource.Where(t => t.l_plate.Contains(sFilter) || t.gps.Contains(sFilter));
            }
        }
        private void Grid1_Loaded_1(object sender, RoutedEventArgs e)
        {
            RecordListView.ItemsSource = m_DataSource;

            for (int i = 0; i < 2000; i++)
            {
                ObjectStates item = new ObjectStates(UserControlTable.RandomNumber(3) + "-" + UserControlTable.RandomString(3), DateTime.Now-TimeSpan.FromMinutes(i), UserControlTable.RandomString(10));
                m_DataSource.Add(item);
            }

            for (int i = 100; i < 200; i++)
            {
                ObjectStates item = new ObjectStates("AAA" + "-" + i.ToString(), DateTime.Now, UserControlTable.RandomString(10));
                m_DataSource.Add(item);
            } 
        }
    }
}
