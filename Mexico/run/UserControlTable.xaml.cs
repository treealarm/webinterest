using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Drawing;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StateStat
{
    /// <summary>
    /// Interaction logic for UserControlTable.xaml
    /// </summary>
    public partial class UserControlTable : UserControl
    {
        public UserControlTable()
        {
            InitializeComponent();
            dispatcherAlarmTimer.Tick += new EventHandler(dispatcherAlarmTimer_Tick);
            dispatcherAlarmTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
        }

        public void UpdateList()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            FillList();
            RecordListView.Items.Refresh();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void dispatcherAlarmTimer_Tick(object sender, EventArgs e)
        {
            m_counter_alarm++;

            if (m_counter_alarm % 2 == 0)
            {
                MainWindow.Me.DockPanelButtons1.Background = (System.Windows.Media.Brush)FindResource("BorderBrush1");
            }
            else
            {
                MainWindow.Me.DockPanelButtons1.Background = (System.Windows.Media.Brush)FindResource("AlarmBrush");
            }

            MainWindow.Me.toggle_Table.UpdateLayout();
            if (m_counter_alarm > 10)
            {
                m_counter_alarm = 0;
                dispatcherAlarmTimer.Stop();
                MainWindow.Me.DockPanelButtons1.Background = (System.Windows.Media.Brush)FindResource("BorderBrush1");
            }            
        }

        ObservableCollection<ObjectStates> m_DataSource = new ObservableCollection<ObjectStates>();
        public void Clear()
        {
            m_DataSource.Clear();
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string RandomNumber(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public static string getLocation() 
        {
            double x0 = 19.432608;
            double y0 = -99.133209;
            int radius = 100;

            Random random = new Random();

            // Convert radius from meters to degrees
            double radiusInDegrees = radius / 111000f;

            double u = random.NextDouble();
            double v = random.NextDouble();
            double w = radiusInDegrees * Math.Sqrt(u);
            double t = 2 * Math.PI * v;
            double x = w * Math.Cos(t);
            double y = w * Math.Sin(t);

            // Adjust the x-coordinate for the shrinking of the east-west distances
            double new_x = x / Math.Cos(ConvertToRadians(y0));

            double foundLongitude = new_x + x0;
            double foundLatitude = y + y0;
            return foundLongitude.ToString() + ":" + foundLatitude.ToString();
        }

        int m_counter = 0;
        int m_counter_alarm = 0;
        private void FillList()
        {
            ObjectStates item = new ObjectStates(RandomNumber(3) + "-" + RandomString(3), DateTime.Now, getLocation());

            if (m_counter == 10)
            {
                m_counter = 0;
            }

            if (m_counter == 0)
            {
                dispatcherAlarmTimer.Start();
                m_counter_alarm = 0;

                item.l_plate = "AAA-100";
                item.text_color = System.Windows.Media.Color.FromRgb(255, 0, 0);
            }
            m_DataSource.Add(item);
            m_counter++;
            if (m_DataSource.Count > 100)
            {
                m_DataSource.RemoveAt(0);
            }
        }

        System.Windows.Threading.DispatcherTimer dispatcherAlarmTimer = new System.Windows.Threading.DispatcherTimer();

        private void Grid1_Loaded(object sender, RoutedEventArgs e)
        {
            RecordListView.ItemsSource = m_DataSource;
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            dispatcherTimer.Start();
        }

        private void RecordListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;

            while (obj != null && obj != RecordListView)
            {
                if (obj.GetType() == typeof(ListViewItem))
                {
                    ListViewItem val = obj as ListViewItem;
                    ObjectStates data = val.DataContext as ObjectStates;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
        }

        private void RecordListView_Initialized(object sender, EventArgs e)
        {
        }

        private void ParentWin_Closing(object sender, CancelEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            GridView gv = RecordListView.View as GridView;
            for (int i = 0; i < gv.Columns.Count; i++)
            {
                ConfigSerializer.Default.ColumnWidth[i] = gv.Columns[i].Width;
            }   
        }

        private void RecordListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            MainWindow.Me.Closing += ParentWin_Closing;
            GridView gv = RecordListView.View as GridView;
            for (int i = 0; i < gv.Columns.Count; i++)
            {
                gv.Columns[i].Width = ConfigSerializer.Default.ColumnWidth[i];
            }
        }
    }
}
