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
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace StateStat
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = System.Windows.Visibility.Hidden;
        }

        public void DoClose()
        {
            this.Closing -= new CancelEventHandler(this.Window_Closing);
            Close();
        }

        protected void DoAddString(string str)
        {
            int maxChunkSize = 8000;
            for (int i = 0; i < str.Length; i += maxChunkSize)
            {
                listBox1.Items.Add((str.Substring(i, Math.Min(maxChunkSize, str.Length - i))));
            }
            
            while (listBox1.Items.Count > 1000)
            {
                listBox1.Items.RemoveAt(0);
            }
        }
        public void AddString(string text)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { DoAddString(text); }));
        }
    }
}
