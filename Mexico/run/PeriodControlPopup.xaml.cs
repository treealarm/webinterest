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

namespace StateStat
{
    /// <summary>
    /// Interaction logic for PeriodControlPopup.xaml
    /// </summary>

    public partial class PeriodControlPopup : UserControl
    {
        private PeriodControlPopupConfigurationData m_data = null;


        public PeriodControlPopup(PeriodControl base_control)
        {
            InitializeComponent();
            m_data = new PeriodControlPopupConfigurationData(base_control);
            this.DataContext = m_data;
        }

        private void PreviewTextInputHour(object sender, TextCompositionEventArgs e)
        {
            var s = sender as TextBox;
            // Use SelectionStart property to find the caret position.
            // Insert the previewed text into the existing text in the textbox.
            var text = s.Text.Insert(s.SelectionStart, e.Text);

            int d;
            // If parsing is successful, set Handled to false
            e.Handled = !int.TryParse(text, out d);
            if (!e.Handled)
            {
                d = -1;
                if(int.TryParse(e.Text, out d))
                {
                    e.Handled = !(d >=0 && d < 24);
                }
                
            }
        }

        private void PreviewTextInputMinutes(object sender, TextCompositionEventArgs e)
        {
            var s = sender as TextBox;
            // Use SelectionStart property to find the caret position.
            // Insert the previewed text into the existing text in the textbox.
            var text = s.Text.Insert(s.SelectionStart, e.Text);

            int d;
            // If parsing is successful, set Handled to false
            e.Handled = !int.TryParse(text, out d);
            if (!e.Handled)
            {
                d = -1;
                if (int.TryParse(e.Text, out d))
                {
                    e.Handled = !(d >= 0 && d < 60);
                }

            }
        }
    }
}
