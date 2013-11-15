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

        private void buttonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = (MainWindow)System.Windows.Application.Current.Windows[0];
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Открыть изображение";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                wnd.OnFileOpen(op.FileName);
            }
        }

        private void buttonStartWork_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = (MainWindow)System.Windows.Application.Current.Windows[0];
            wnd.Start();
        }

        private void buttonOpenDevice_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = (MainWindow)System.Windows.Application.Current.Windows[0];
            if (!wnd.m_ControlWrapper.Connect())
            {
                MessageBox.Show(Properties.Resources.StringDevIsUnavailable);
            }
            wnd.SetTimerSettings(Properties.Settings.Default.TimerRes, Properties.Settings.Default.TimerStrike,
                new byte[]{
                    Properties.Settings.Default.TimerMultiplierX,
                    Properties.Settings.Default.TimerMultiplierY,
                    Properties.Settings.Default.TimerMultiplierZ});


            wnd.SetControlSettings(true, checkBoxOutpusEnergy.IsChecked == false);
            wnd.m_step_mult.m_uMult[MainWindow.X_POS] = Properties.Settings.Default.StepMultiplierX;
            wnd.m_step_mult.m_uMult[MainWindow.Y_POS] = Properties.Settings.Default.StepMultiplierY;
            wnd.m_step_mult.m_uMult[MainWindow.Z_POS] = Properties.Settings.Default.StepMultiplierZ;
        }

        private void X_Plus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void X_Minus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Y_Plus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Y_Minus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Z_Plus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Z_Minus_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
