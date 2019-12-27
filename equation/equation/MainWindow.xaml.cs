using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace equation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        SolidColorBrush clrNotSelected = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        SolidColorBrush clrSelected = new SolidColorBrush(Color.FromArgb(200, 255, 0, 0));

        const int fontsize = 100;
        void AddNewNumber(StackPanel panel, string s, bool bLeft)
        {
            MyTextBlock l = new MyTextBlock();
            l.FontSize = fontsize;
            l.Background = clrNotSelected;
            l.VerticalAlignment = VerticalAlignment.Center;
            l.MouseLeftButtonDown += Label_MouseLeftButtonDown;
            l.MouseLeftButtonUp += Label_MouseLeftButtonUp;
            try
            {
                l.Text = s;
                l.m_src_value = s;
                l.m_bLeft = bLeft;
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
                return;
            }

            panel.Children.Add(l);
        }
        public void BtnAddNumber_Click(object sender, RoutedEventArgs e)
        {
            ResolveSP.Visibility = Visibility.Hidden;

            LeftSP.Children.Clear();
            RightSP.Children.Clear();

            String strings = txtAddNember.Text;
            string symbol = string.Empty;
            bool bLeft = true;
            List<string> left = new List<string>();
            List<string> right = new List<string>();
            for (int i = 0; i < strings.Length; i++)
            {
                if (strings[i] == '=')
                {
                    if (symbol != string.Empty)
                    {
                        left.Add(symbol);
                        symbol = string.Empty;
                    }
                    bLeft = false;
                    continue;
                }
                
                if (strings[i] == '-' || strings[i] == '+')
                {
                    if(symbol != string.Empty)
                    {
                        if(bLeft)
                        {
                            left.Add(symbol);
                        }
                        else
                        {
                            right.Add(symbol);
                        }
                    }
                    symbol = string.Empty;
                }
                symbol += strings[i];
            }
            if (symbol != string.Empty)
            {
                right.Add(symbol);
            }

            foreach (var s in left)
            {
                AddNewNumber(LeftSP, s, true);
            }
            foreach (var s in right)
            {
                AddNewNumber(RightSP, s, false);
            }

        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyTextBlock l = sender as MyTextBlock;
            if(l == null || l.Text=="0")
            {
                return;
            }
            if(!l.IsExplicitPositive() && !l.IsExplicitNegative())
            {
                l.m_src_value = "+" + l.m_src_value;
                l.Text = "+" + l.Text;
            }
            l.Background = clrSelected;
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MyTextBlock l = sender as MyTextBlock;
            l.Background = clrNotSelected;
        }

        Point _point = new Point();

        MyTextBlock selectedTb = null;
        TranslateTransform m_selected_tt = new TranslateTransform();
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _point = e.MouseDevice.GetPosition(this);
            UIElement el = (UIElement)sender;
            this.CaptureMouse();

            selectedTb = null;
            foreach (var tb in LeftSP.Children)
            {
                MyTextBlock cur_tb = tb as MyTextBlock;
                if(cur_tb == null)
                {
                    continue;
                }
                if(cur_tb.Background == clrSelected)
                {
                    selectedTb = cur_tb;
                    m_selected_tt = new TranslateTransform();
                    selectedTb.RenderTransform = m_selected_tt;
                    return;
                }
            }

            foreach (var tb in RightSP.Children)
            {
                MyTextBlock cur_tb = tb as MyTextBlock;
                if (cur_tb == null)
                {
                    continue;
                }
                if (cur_tb.Background == clrSelected)
                {
                    selectedTb = cur_tb;
                    m_selected_tt = new TranslateTransform();
                    selectedTb.RenderTransform = m_selected_tt;
                    return;
                }
            }
        }

        void RemoveZeroes(StackPanel panel)
        {
            for(int i = panel.Children.Count - 1; i >=0; i--)
            {
                MyTextBlock cur_tb = panel.Children[i] as MyTextBlock;
                if (cur_tb == null)
                {
                    continue;
                }
                if(cur_tb.Text == "0")
                {
                    panel.Children.RemoveAt(i);
                }
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UIElement el = (UIElement)sender;
            if (selectedTb != null)
            {
                bool set_opposite = CheckIfHaveToBeOpposite(sender, e);
                if (set_opposite)
                {
                    selectedTb.m_src_value = selectedTb.GetOpposite();
                    selectedTb.m_bLeft = !selectedTb.m_bLeft;
                    LeftSP.Children.Remove(selectedTb);
                    RightSP.Children.Remove(selectedTb);
                    if(selectedTb.m_bLeft)
                    {
                        LeftSP.Children.Add(selectedTb);
                    }
                    else
                    {
                        RightSP.Children.Add(selectedTb);
                    }
                }
            }

            this.ReleaseMouseCapture();
            foreach (var tb in LeftSP.Children)
            {
                MyTextBlock cur_tb = tb as MyTextBlock;
                if (cur_tb == null)
                {
                    continue;
                }
                cur_tb.Background = clrNotSelected;
                cur_tb.RenderTransform = null;
            }
            foreach (var tb in RightSP.Children)
            {
                MyTextBlock cur_tb = tb as MyTextBlock;
                if (cur_tb == null)
                {
                    continue;
                }
                cur_tb.Background = clrNotSelected;
                cur_tb.RenderTransform = null;
            }
            selectedTb = null;

            RemoveZeroes(LeftSP);
            RemoveZeroes(RightSP);

            if (RightSP.Children.Count > 0)
            {
                (RightSP.Children[0] as MyTextBlock).RemoveExplicitPositive();
            }
            if (LeftSP.Children.Count > 0)
            {
                (LeftSP.Children[0] as MyTextBlock).RemoveExplicitPositive();
            }


            if (RightSP.Children.Count == 0)
            {
                AddNewNumber(RightSP, "0", false);
            }
            if (LeftSP.Children.Count == 0)
            {
                AddNewNumber(LeftSP, "0", false);
            }
        }

        bool CheckIfHaveToBeOpposite(object sender, MouseEventArgs e)
        {
            bool retval = false;
            Point cur_point = e.MouseDevice.GetPosition(this);
            Vector offset = cur_point - _point;
            if (selectedTb != null)
            {
                m_selected_tt.X = offset.X;
                m_selected_tt.Y = offset.Y;

                int row = 0;
                int col = 0;
                GetRowColumn(cur_point, out row, out col);

                if (selectedTb.m_bLeft)
                {
                    if (col == 2)
                    {
                        selectedTb.Text = selectedTb.GetOpposite();
                        retval = true;
                    }
                    else
                    {
                        selectedTb.Text = selectedTb.m_src_value;
                    }
                }
                else
                {
                    if (col == 0)
                    {
                        selectedTb.Text = selectedTb.GetOpposite();
                        retval = true;
                    }
                    else
                    {
                        selectedTb.Text = selectedTb.m_src_value;
                    }
                }

            }
            return retval;
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            CheckIfHaveToBeOpposite(sender, e);
        }

        public void GetRowColumn(Point position, out int row, out int column)
        {
            column = -1;
            double total = 0;
            foreach (ColumnDefinition clm in Grid1.ColumnDefinitions)
            {
                if (position.X < total)
                {
                    break;
                }
                column++;
                total += clm.ActualWidth;
            }
            row = -1;
            total = 0;
            foreach (RowDefinition rowDef in Grid1.RowDefinitions)
            {
                if (position.Y < total)
                {
                    break;
                }
                row++;
                total += rowDef.ActualHeight;
            }
        }
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
 

        }

        private void Grid1_Loaded(object sender, RoutedEventArgs e)
        {
            BtnAddNumber_Click(sender, e);
        }

        private void LabelEqual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            List<MyTextBlock> temp1 = new List<MyTextBlock>();
            List<MyTextBlock> temp2 = new List<MyTextBlock>();

            foreach (var item in LeftSP.Children)
            {
                var t = item as MyTextBlock;
                if(t != null)
                {
                    t.m_bLeft = !t.m_bLeft;
                    temp1.Add(t);
                }
                
            }
            foreach (var item in RightSP.Children)
            {
                var t = item as MyTextBlock;
                if (t != null)
                {
                    t.m_bLeft = !t.m_bLeft;
                    temp2.Add(t);
                }
            }

            LeftSP.Children.Clear();            
            RightSP.Children.Clear();

            foreach(var item in temp1)
            {
                RightSP.Children.Add(item);
            }
            foreach (var item in temp2)
            {
                LeftSP.Children.Add(item );
            }
        }

        private void BtnResolve_Click(object sender, RoutedEventArgs e)
        {
            ResolveSP.Visibility = Visibility.Visible;
            RightResolveSP.Children.Clear();
            LeftResolveSP.Children.Clear();
            int res = 0;
            foreach (var item in RightSP.Children)
            {
                MyTextBlock tb = item as MyTextBlock;
                try
                {
                    int cur = Convert.ToInt32(tb.m_src_value);
                    res += cur;
                }
                catch 
                {
                    AddNewNumber(RightResolveSP, tb.m_src_value, false);
                }
            }
            if(res != 0)
            {
                if (res > 0 && RightResolveSP.Children.Count > 0)
                {
                    AddNewNumber(RightResolveSP, "+" + res.ToString(), true);
                }
                else
                {
                    AddNewNumber(RightResolveSP, res.ToString(), true);
                }
            }
            else if (LeftResolveSP.Children.Count == 0)
            {
                AddNewNumber(RightResolveSP, "0", true);
            }
            ////////////////////

            res = 0;
            foreach (var item in LeftSP.Children)
            {
                MyTextBlock tb = item as MyTextBlock;
                try
                {
                    int cur = Convert.ToInt32(tb.m_src_value);
                    res += cur;
                }
                catch 
                {
                    AddNewNumber(LeftResolveSP, tb.m_src_value, true);
                }
            }
            if (res != 0)
            {
                if(res > 0 && LeftResolveSP.Children.Count > 0)
                {
                    AddNewNumber(LeftResolveSP, "+" + res.ToString(), true);
                }
                else
                {
                    AddNewNumber(LeftResolveSP, res.ToString(), true);
                }
                
            }
            else if(LeftResolveSP.Children.Count == 0)
            {
                AddNewNumber(LeftResolveSP, "0", true);
            }
        }
    }
}
