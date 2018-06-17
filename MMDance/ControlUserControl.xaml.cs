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
            wnd.m_bEmulation = false;
            wnd.Start();
        }

        public void SetTimerSetting()
        {
            MainWindow wnd = MainWindow.GetMainWnd();

            byte TimerMultiplierX = Convert.ToByte(Properties.Settings.Default.TimerMultiplierX);
            byte TimerMultiplierY = Convert.ToByte(Properties.Settings.Default.TimerMultiplierY);
            wnd.SetTimerSettings(
                Properties.Settings.Default.TimerRes, 
                Properties.Settings.Default.TimerStrike,
                new byte[]{
                TimerMultiplierX,
                TimerMultiplierY,
                Properties.Settings.Default.TimerMultiplierB,
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
            
            wnd.m_step_mult.m_uMult[MainWindow.X_POS] = Properties.Settings.Default.StepMultiplierX;
            wnd.m_step_mult.m_uMult[MainWindow.Y_POS] = Properties.Settings.Default.StepMultiplierY;
            wnd.m_step_mult.m_uMult[MainWindow.B_POS] = Properties.Settings.Default.StepMultiplierB;
            wnd.m_step_mult.m_uMult[MainWindow.W_POS] = Properties.Settings.Default.StepMultiplierW;
            
            wnd.SetCruisersToController();
        }

        private void SetControlSettings()
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.SetControlSettings(checkBoxOutpusEnergy.IsChecked==true);
        }

        void PassSteps(int x = 0, int y = 0, int b = 0, int w = 0, bool update_pos = true)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            MainWindow.do_steps var_do_steps = new MainWindow.do_steps();
            var_do_steps.m_uSteps[MainWindow.X_POS] = x;
            var_do_steps.m_uSteps[MainWindow.Y_POS] = y;
            var_do_steps.m_uSteps[MainWindow.B_POS] = b;
            var_do_steps.m_uSteps[MainWindow.W_POS] = w;
            wnd.SetStepsToController(var_do_steps, update_pos);
        }
 
        private void X_Plus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(Convert.ToInt32(XShift.Text));
        }

        private void X_Minus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(-Convert.ToInt32(XShift.Text));
        }

        private void Y_Plus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(0, Convert.ToInt32(YShift.Text));
        }

        private void Y_Minus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(0, -Convert.ToInt32(YShift.Text));
        }

        private void B_Plus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(0, 0, -Convert.ToInt32(BShift.Text), 0, false);
        }

        private void B_Minus_Click(object sender, RoutedEventArgs e)
        {
            PassSteps(0, 0, Convert.ToInt32(BShift.Text), 0, false);
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

        private void GoToXY_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.GoToXY(Convert.ToInt32(GoToXEdit.Text), Convert.ToInt32(GoToYEdit.Text));
        }

        private void GoToBW_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.GoToBW(Convert.ToInt32(GoToBEdit.Text), Convert.ToInt32(GoToWEdit.Text));
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

        private void buttonEmulate_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            wnd.m_bEmulation = true;
            wnd.Start();
        }

        private void buttonOpenGCode_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = MainWindow.GetMainWnd();
            OpenFileDialog op = new OpenFileDialog();
            op.InitialDirectory = op.FileName;
            op.Title = "Open G-Code";
            op.Filter = "All supported |*.*";
            if (op.ShowDialog() == true)
            {
                if (wnd.OnGCodeFileOpen(op.FileName))
                {
                    Properties.Settings.Default.GCodeFile = op.FileName;
                }
            }
        }
    }
}
