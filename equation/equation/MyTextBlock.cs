using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
namespace equation
{
    class MyTextBlock : TextBlock
    {
        public string m_src_value = string.Empty;
        public bool m_bLeft = true;
        public string GetOpposite()
        {
            string ret = m_src_value;

            if(m_src_value.IndexOf('-') == 0)
            {
                ret = ret.Replace('-', '+');
            }
            else
            {
                ret = ret.Replace('+', '-');
                if (ret.IndexOf('-') != 0)
                {
                    ret = "-" + ret;
                }
            }
            return ret;
        }
        public bool IsExplicitPositive()
        {
            return m_src_value.IndexOf('+') == 0;
        }
        public bool IsExplicitNegative()
        {
            return m_src_value.IndexOf('-') == 0;
        }
        public void RemoveExplicitPositive()
        {
            if(IsExplicitPositive())
            {
                m_src_value = m_src_value.Replace("+", "");
                Text = m_src_value;
            }
        }

    }
}
