using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MMDance
{
    public class PictureColors
    {
        public PictureColors(Color color_in, int count_in)
        {
            color = color_in;
            count = count_in;
        }
        
        protected Color m_color;
        protected SolidColorBrush m_brush = new SolidColorBrush();

        public Color color 
        {
            get
            {
                return m_color;
            }
            set
            {
                m_brush = new SolidColorBrush(value);
                m_color = value;
            }
        }
        public string color_text
        {
            get
            {
                return string.Format("{0},{1},{2}", m_color.R, m_color.G, m_color.B); 
            }
        }
        public int count 
        {
            get;
            set;
        }
        public Brush BackgroundColor
        {
            get 
            {
                return (Brush)m_brush; 
            }
        }
    }
}
