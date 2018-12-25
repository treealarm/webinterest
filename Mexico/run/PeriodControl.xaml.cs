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
using System.Windows.Controls.Primitives;
using System.Globalization;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace StateStat
{
    /// <summary>
    /// Interaction logic for PeriodControl.xaml
    /// </summary>

    public partial class PeriodControl : UserControl
    {
        public PeriodConfigurationData m_data = new PeriodConfigurationData();

        public PeriodControl()
        {
            InitializeComponent();
            DataContext = m_data;
            
            //EditingCommands.ToggleInsert.Execute(null, textBoxPeriod);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MainWindow.Me.myPopup.Child != null)
                {
                    MainWindow.Me.myPopup.Child = null;
                    return;
                }

                MainWindow.Me.myPopup.Child = new PeriodControlPopup(this);
                MainWindow.Me.myPopup.IsOpen = true;
            }
            catch (Exception ex)
            {
                
            }            
        }

        private void textBoxPeriod_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBoxPeriod_KeyUp(object sender, KeyEventArgs e)
        {
            //e.Handled = true;
        }

        private void textBoxPeriod_KeyDown(object sender, KeyEventArgs e)
        {
            //e.Handled = true;
        }

        private void textBoxPeriod_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //int from = textBoxPeriod.SelectionStart;
            //int len = textBoxPeriod.SelectionLength;
            //e.Handled = true;
            //15/13/18 12:56 - 14/12/18 13:06
        }

        private void grid1_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
