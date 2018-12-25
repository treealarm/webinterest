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
using System.IO;

namespace StateStat
{
    /// <summary>
    /// Interaction logic for UserControlGraph.xaml
    /// </summary>
    public partial class UserControlGraph : UserControl
    {

        public UserControlGraph()
        {
            InitializeComponent();
            Assembly assembly = this.GetType().Assembly;
            WriteResourceToFile("StateStat.Resources.out.avi", "out.avi");
        }

        public void WriteResourceToFile(string resourceName, string fileName)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                {
                    return;
                }
                using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        public void Clear()
        {
        }


        public void UpdateCurState(ObjectStates val)
        {

        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void MediaElement1_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MediaElement1.IsVisible)
            {
                //MediaElement1.Source = new Uri(@"C:\avi\1\1.avi", UriKind.Absolute);
                //MediaElement1.Play();
            }
            else
            {
                //MediaElement1.Stop();
            }
        }
    }
}
