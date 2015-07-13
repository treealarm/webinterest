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

namespace MMDance
{
    /// <summary>
    /// Interaction logic for ControlUserControl.xaml
    /// </summary>
    public partial class ControlUserControl : UserControl
    {
        public ControlUserControl()
        {
            InitializeComponent();
        }

        private void buttonStartWork_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.Start();
        }

        
        public void SetTimerSetting()
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.SetTimerSettings(
                Properties.Settings.Default.TimerRes, 
                Properties.Settings.Default.TimerStrike,
                new byte[]{
                Properties.Settings.Default.TimerMultiplierX,
                Properties.Settings.Default.TimerMultiplierY,
                Properties.Settings.Default.TimerMultiplierZ,
                Properties.Settings.Default.TimerMultiplierW});

        }
        private void buttonOpenDevice_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            if (!wnd.m_ControlWrapper.Connect())
            {
                MessageBox.Show(Properties.Resources.StringDevIsUnavailable);
            }
           SetTimerSetting();

            SetControlSettings();
            
            wnd.m_step_mult.m_uMult[MainWindow.Z_POS] = Properties.Settings.Default.StepMultiplierZ;
            wnd.m_step_mult.m_uMult[MainWindow.X_POS] = Properties.Settings.Default.StepMultiplierX;
            wnd.m_step_mult.m_uMult[MainWindow.Y_POS] = Properties.Settings.Default.StepMultiplierY;
            wnd.m_step_mult.m_uMult[MainWindow.W_POS] = Properties.Settings.Default.StepMultiplierW;
        }

        private void SetControlSettings()
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.SetControlSettings(checkBoxOutpusEnergy.IsChecked==true);
        }

        void PassSteps(int x = 0, int y = 0, int z = 0, int w = 0, bool update_pos = true)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            MainWindow.do_steps var_do_steps = new MainWindow.do_steps();
            var_do_steps.m_uSteps[MainWindow.Z_POS] = z;
            var_do_steps.m_uSteps[MainWindow.X_POS] = x;
            var_do_steps.m_uSteps[MainWindow.Y_POS] = y;
            var_do_steps.m_uSteps[MainWindow.W_POS] = w;
            wnd.SetStepsToController(var_do_steps, update_pos);
        }
 
        private void Z_Plus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(0, 0, Convert.ToInt32(ZShift.Text));
        }

        private void Z_Minus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(0, 0, -Convert.ToInt32(ZShift.Text));
        }

        private void Y_Plus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(0, Convert.ToInt32(YShift.Text));
        }

        private void Y_Minus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(0, -Convert.ToInt32(YShift.Text));
        }

        private void X_Plus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(Convert.ToInt32(XShift.Text), 0, 0, 0, false);
        }

        private void X_Minus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(-Convert.ToInt32(XShift.Text), 0, 0, 0, false);
        }


        private void W_Plus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(0, 0, 0,Convert.ToInt32(WShift.Text), false);
        }

        private void W_Minus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(0, 0, 0, -Convert.ToInt32(WShift.Text), false);
        }
        private void checkBoxOutpusEnergy_Checked(object sender, RoutedEventArgs e)
        {
            SetControlSettings();
        }

        private void checkBoxOutpusEnergy_Unchecked(object sender, RoutedEventArgs e)
        {
            SetControlSettings();
        }

        private void checkBoxPause_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.SetPause(checkBoxPause.IsChecked == true);
        }
        private void checkBoxPause_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.SetPause(checkBoxPause.IsChecked == true);
        }

        private void checkBoxInk_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.SetInk(checkBoxInk.IsChecked == true);
        }

        private void checkBoxInk_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.SetInk(checkBoxInk.IsChecked == true);
        }

        private void checkBoxPauseSoft_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.SetPauseSoft(checkBoxPauseSoft.IsChecked == true);
        }

        private void checkBoxPauseSoft_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.SetPauseSoft(checkBoxPauseSoft.IsChecked == true);
        }

        private void buttonUpdateTimer_Click(object sender, RoutedEventArgs e)
        {
            SetTimerSetting();
        }

        private void buttonSaveAll_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog op = new SaveFileDialog();
            op.Filter = "xml|*.xml;";
            if (op.ShowDialog() == true)
            {
                MainWindow.GetMainWnd().SaveAll(op.FileName);
            }
        }

        private void buttonLoadAll_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "xml|*.xml;";
            if (op.ShowDialog() == true)
            {
                MainWindow.GetMainWnd().LoadAll(op.FileName);
            }
        }
    }
}
