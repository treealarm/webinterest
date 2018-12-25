using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace StateStat
{
    public class PeriodConfigurationData : INotifyPropertyChanged
    {

        string m_searchString = string.Empty;

        public string SearchString
        {
            get
            {
                return m_searchString;
            }
            set
            {
                m_searchString = value;
                OnPropertyChanged("SearchString");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
