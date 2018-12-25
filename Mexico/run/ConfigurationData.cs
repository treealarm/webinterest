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
        public DateTime m_from
        {
            get
            {
                return m_from_val;
            }
            set
            {
                m_from_val = value;
                OnPropertyChanged("StrPeriod");
            }
        }

        public DateTime m_to
        {
            get
            {
                return m_to_val;
            }
            set
            {
                m_to_val = value;
                OnPropertyChanged("StrPeriod");
            }
        }

        private DateTime m_from_val = DateTime.Now - TimeSpan.FromMinutes(10);
        private DateTime m_to_val = DateTime.Now;

        public string StrPeriod
        {
            get
            {
                string s =
                    string.Format("{0} - {1}", m_from.ToString("dd/MM/yy HH:mm", DateTimeFormatInfo.InvariantInfo), m_to.ToString("dd/MM/yy HH:mm", DateTimeFormatInfo.InvariantInfo));

                return s;
            }
            set
            {
                OnPropertyChanged("StrPeriod");
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



    ///////////////////////////////////////////////////////
    public class PeriodControlPopupConfigurationData : INotifyPropertyChanged
    {
        public PeriodControlPopupConfigurationData(PeriodControl base_control_)
        {
            base_control = base_control_;
        }
        PeriodControl base_control = null;

        public DateTime m_from_dt
        {
            get
            {
                if (base_control != null)
                {
                    return base_control.m_data.m_from;
                }
                return DateTime.Now;
            }
            set
            {
                if (base_control != null)
                {
                    base_control.m_data.m_from = new DateTime(value.Year, value.Month, value.Day, int.Parse(from_hh), int.Parse(from_mm), 0);
                    OnPropertyChanged("m_from_dt");
                }
            }
        }

        public string from_hh
        {
            get
            {
                return base_control.m_data.m_from.Hour.ToString("00");
            }
            set
            {
                try
                {
                    base_control.m_data.m_from = new DateTime(m_from_dt.Year, m_from_dt.Month, m_from_dt.Day, int.Parse(value), int.Parse(from_mm), 0);
                }
                catch { }
                OnPropertyChanged("from_hh");
            }
        }

        public string from_mm
        {
            get
            {
                return base_control.m_data.m_from.Minute.ToString("00");
            }
            set
            {
                try
                {
                    base_control.m_data.m_from = new DateTime(m_from_dt.Year, m_from_dt.Month, m_from_dt.Day, int.Parse(from_hh), int.Parse(value), 0);
                }
                catch { }
                
                OnPropertyChanged("from_mm");
            }
        }

        public DateTime m_to_dt
        {
            get
            {
                if (base_control != null)
                {
                    return base_control.m_data.m_to;
                }
                return DateTime.Now;
            }
            set
            {
                if (base_control != null)
                {
                    try
                    {
                        base_control.m_data.m_to = new DateTime(value.Year, value.Month, value.Day, int.Parse(to_hh), int.Parse(to_mm), 0);
                    }
                    catch { }
                    
                    OnPropertyChanged("m_to_dt");
                }
            }
        }

        public string to_hh
        {
            get
            {
                return base_control.m_data.m_to.Hour.ToString("00");
            }
            set
            {
                try
                {
                    base_control.m_data.m_to = new DateTime(m_from_dt.Year, m_from_dt.Month, m_from_dt.Day, int.Parse(value), int.Parse(to_mm), 0);
                }
                catch { }
                
                OnPropertyChanged("to_hh");
            }
        }

        public string to_mm
        {
            get
            {
                return base_control.m_data.m_to.Minute.ToString("00");
            }
            set
            {
                try
                {
                    base_control.m_data.m_to = new DateTime(m_from_dt.Year, m_from_dt.Month, m_from_dt.Day, int.Parse(to_hh), int.Parse(value), 0);
                }
                catch { }
                
                OnPropertyChanged("to_mm");
            }
        }

        //below is the boilerplate code supporting PropertyChanged events:
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
