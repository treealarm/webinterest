using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;

namespace StateStat
{
    public class ObjectTypeAndState
    {
        public string objtype
        {
            get;
            set;
        }

        public string objstate
        {
            get;
            set;
        }
    }

    public class ObjectFilter
    {
        public string objtype
        {
            get;
            set;
        }
        
        public string objstate
        {
            get;
            set;
        }

        public System.Windows.Media.Color color
        {
            get;
            set;
        }
    }

    public class ObjectStates
    {
        public ObjectStates()
        {
        }
        public ObjectStates(string lp, DateTime time, string _gps)
        {
            l_plate = lp;
            time_stamp = time;
            gps = _gps;
        }

        public string gps
        {
            get;
            set;
        }
        
        public string l_plate
        {
            get;
            set;
        }

        public System.Windows.Media.Color text_color
        {
            get;
            set;
        }
        public System.Windows.Media.SolidColorBrush text_color_brush
        {
            get
            {
                return new System.Windows.Media.SolidColorBrush(text_color);
            }
        }
        public DateTime time_stamp
        {
            get;
            set;
        }

        public void MakeCopy(ObjectStates other)
        {
            foreach (var property in other.GetType().GetProperties())
            {
                try
                {
                    PropertyInfo propertyS = GetType().GetProperty(property.Name);
                    if (!propertyS.CanWrite)
                    {
                        continue;
                    }
                    var value = property.GetValue(other, null);
                    propertyS.SetValue(this, property.GetValue(other, null), null);
                }
                catch(Exception e)
                {

                }
            }
        }
    }
}
