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
using System.ComponentModel;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data.SqlClient;
using System.Drawing;
using StateStat.Common;
using System.Windows.Controls.Primitives;

namespace StateStat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        public static MainWindow Me = null;

        public MainWindow()
        {
            Me = this;
            InitializeComponent();
        }

        protected DebugWindow m_DebugForm = null;

        public static string GetIdOfSlave()
        {
            String[] arguments = Environment.GetCommandLineArgs();
            string id = string.Empty;
            if (arguments.Length > 1)
            {
                id = arguments[1];
            }
            return id;
        }

        public static IntPtr m_MyHandle = IntPtr.Zero;
        private void Window_Initialized(object sender, EventArgs e)
        {
            m_MyHandle = (new WindowInteropHelper(this)).EnsureHandle();
            HwndSource source = HwndSource.FromHwnd(m_MyHandle);
            source.AddHook(new HwndSourceHook(WndProc));

            m_DebugForm = new DebugWindow();
            string id = GetIdOfSlave();

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += new EventHandler(notifyIcon_Click);
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            notifyIcon.Text = "StateStat " + id;
            var iconHandle = Properties.Resources.Main.Handle;
            notifyIcon.Icon = System.Drawing.Icon.FromHandle(iconHandle);
            notifyIcon.Visible = true;
        }

        public static int WM_EXIT = 0x0400 + 2000;
        public static int WM_KEYDOWN = 0x0100;
        public static int WM_SYSKEYDOWN = 0x0104;

        public void DoClose()
        {
            this.Closing -= new CancelEventHandler(this.Window_Closing);
            Close();
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = msg == WM_EXIT;
            if (handled)
            {
                DoClose();
            }

            return IntPtr.Zero;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
#if !DEBUG
            //e.Cancel = true;
#endif
        }

        void notifyIcon_Click(object sender, EventArgs e)
        {
           
        }
        void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            m_DebugForm.Show();
        }

        private void connector_OnConnected(object sender, EventArgs e)
        {

        }

        private void connector_OnDisconnected(object sender, EventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                ConfigSerializer.SaveAll(GetIdOfSlave());
            }            

            notifyIcon.Visible = false;
            m_DebugForm.DoClose();
        }

        string UserId = string.Empty;


        public static string GenerateConnectionString()
        {
            string dbServer = string.Empty;
            string dbName = string.Empty;
            string UserId = string.Empty;
            string Password = string.Empty;

            string connection_string = @"(local)\FULL2014;intellect1;;;";
            string[] cs_data = connection_string.Split(';');


            for (int i = 0; i < cs_data.Length; i++)
            {
                switch (i)
                {
                    case 0: dbServer = cs_data[i]; break;
                    case 1: dbName = cs_data[i]; break;
                    case 2: UserId = cs_data[i]; break;
                    case 3: Password = cs_data[i]; break;
                }
            }

            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();

            // Set the properties for the data source.
            sqlBuilder.DataSource = dbServer;
            sqlBuilder.InitialCatalog = dbName;
            sqlBuilder.UserID = UserId;
            sqlBuilder.Password = Password;
            sqlBuilder.IntegratedSecurity = UserId == string.Empty;

            // Build the SqlConnection connection string.
            return sqlBuilder.ToString();
        }

        private static SqlConnection m_Connection = null;
        public static SqlConnection GetConnection()
        {
            if (m_Connection == null)
            {
                m_Connection = OpenConnection();
            }
            return m_Connection;
        }

        public Dictionary<string, Dictionary<string, string>> m_dicStates = new Dictionary<string, Dictionary<string, string>>();
        public Dictionary<string, ObjectFilter> m_dicObjFilter = new Dictionary<string, ObjectFilter>();
        public Dictionary<string, string> m_objtype_descr = new Dictionary<string, string>();

        public System.Windows.Media.Color GetStateColor(string objtype, string objstate)
        {
            System.Windows.Media.Color color = System.Windows.Media.Color.FromRgb(0,0,0);
            var first = MainWindow.Me.m_dicObjFilter.Where(t => t.Key == (objtype + objstate) || t.Key == objtype);
            if (first.Any())
            {
                color = first.First().Value.color;
            }
            return color;
        }
        public int MaxPoints
        {
            get;
            set;
        }

        public string GetStateDescription(string objtype, string objstate)
        {
            if (string.IsNullOrEmpty(objtype) || string.IsNullOrEmpty(objstate))
            {
                return string.Empty;
            }
            string descr = objstate;

            Dictionary<string, string> state_list = null;
            if (m_dicStates.TryGetValue(objtype, out state_list))
            {
                state_list.TryGetValue(objstate, out descr);
            }
            return descr;
        }

        public string GetObjtypeName(string objtype)
        {
            string descr = string.Empty;
            m_objtype_descr.TryGetValue(objtype, out descr);
            return descr;
        }


        LogFont m_LogFont = new LogFont();
        public Font TextFont;
        

        private static Font tryCreateFont(LogFont lf)
        {
            Font font = null;
            try
            {
                font = Font.FromLogFont(lf);
            }
            catch (Exception ex)
            {
                
            }
            return font;
        }
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        public static int WS_BORDER = 0x00800000; //window with border 
        public static int WS_DLGFRAME = 0x00400000; //window with double border but no title 
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar 

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const uint SC_CLOSE = 0xF060;
        private const uint SC_MINIMIZE = 0xF020;
        private const uint SC_MAXIMIZE = 0xF030;
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hwnd, bool revert);
        [DllImport("user32.dll")]
        private static extern bool DeleteMenu(IntPtr hMenu, uint position, uint flags);

        public void SetWindowCaption(bool yes)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero)
            {
                return;
            }
            if (yes)
            {
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) | (WS_SYSMENU | WS_CAPTION));
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~(WS_SYSMENU));
                ResizeMode = System.Windows.ResizeMode.CanResize;
            }
            else
            {
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~(WS_SYSMENU | WS_CAPTION));
                ResizeMode = System.Windows.ResizeMode.NoResize;
            }
            UpdateLayout();
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
        public static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);


        private static SqlConnection OpenConnection()
        {
            SqlConnection Connection = null;
            try
            {
                string cs = GenerateConnectionString();
                Connection = new SqlConnection(cs);
                Connection.Open();
            }
            catch (System.Exception ex)
            {
                string s = ex.Message;
                if (ex.InnerException != null)
                {
                    s += ex.InnerException;
                }
                System.Windows.MessageBox.Show(s);
            }
            return Connection;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            ConfigSerializer.LoadAll(GetIdOfSlave());
        }

        private void toggle_Table_Checked(object sender, RoutedEventArgs e)
        {
            if (TabControl1 == null)
            {
                return;
            }
            TabControl1.SelectedIndex = 0;
        }

        private void toggle_Graph_Checked(object sender, RoutedEventArgs e)
        {
            if (TabControl1 == null)
            {
                return;
            }
            TabControl1.SelectedIndex = 1;
        }

        private void toggle_GraphRetro_Checked(object sender, RoutedEventArgs e)
        {
            if (TabControl1 == null)
            {
                return;
            }
            TabControl1.SelectedIndex = 2;
        }

        void UpdatePeriodControlVisibility()
        {
            if (TabControl1.SelectedIndex == 2)
            {
                PeriodControl1.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                PeriodControl1.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void TabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePeriodControlVisibility();
        }
    }
}
