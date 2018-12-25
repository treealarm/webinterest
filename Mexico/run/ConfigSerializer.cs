using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace StateStat
{
    
    public class EV_Settings
    {
        public double[] ColumnWidth = new double[4] { 100, 100, 100, 100 };
    }


    public class ConfigSerializer
    {
        public static EV_Settings Default = new EV_Settings();

        static string GetPath(string my_id)
        {
            string FileName = Application.ExecutablePath + my_id + ".xml";
            return FileName;
        }
        public static void LoadAll(string my_id)
        {
            Default = new EV_Settings();
            try
            {
                string FileName = GetPath(my_id);
                using (StreamReader outfile = new StreamReader(FileName, true))
                {
                    Default = ConfigSerializer.DeserializeObject(outfile.ReadToEnd());

                    List<double> temp = new List<double>();
                    temp.AddRange(Default.ColumnWidth);

                    while (temp.Count() < 4)
                    {
                        temp.Add(100);                        
                    }
                    Default.ColumnWidth = temp.ToArray();
                }
            }
            catch (Exception ex)
            {

            }

        }

        public static void SaveAll(string my_id)
        {
            if (Default == null)
            {
                return;
            }
            string FileName = GetPath(my_id);
            using (StreamWriter outfile = new StreamWriter(FileName, false))
            {
                outfile.Write(ConfigSerializer.SerializeObject(Default));
            }
        }

        public static EV_Settings DeserializeObject(string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(EV_Settings));
            StringReader textReader = new StringReader(toDeserialize);
            return xmlSerializer.Deserialize(textReader) as EV_Settings;
        }

        public static string SerializeObject(EV_Settings toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(EV_Settings));
            StringWriter textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }
    }
}
