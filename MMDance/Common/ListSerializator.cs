using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace MMDance
{
    public static class ListPoint
    {
        public static List<Point> DeserializeObject(string fileName)
        {
            try
            {
                using (var stream = System.IO.File.OpenRead(fileName))
                {
                    var serializer = new XmlSerializer(typeof(List<Point>));
                    return serializer.Deserialize(stream) as List<Point>;
                }
            }
            catch (Exception ex)
            {
                return new List<Point>();	
            }            
        }

        public static void SerializeObject(List<Point> toSerialize, string fileName)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Point>));
                xmlSerializer.Serialize(writer, toSerialize);
            }            
        }
    }

    public static class ListDouble
    {
        public static List<double> DeserializeObject(string fileName)
        {
            try
            {
                using (var stream = System.IO.File.OpenRead(fileName))
                {
                    var serializer = new XmlSerializer(typeof(List<double>));
                    return serializer.Deserialize(stream) as List<double>;
                }
            }
            catch (Exception ex)
            {
                return new List<double>();
            }
        }

        public static void SerializeObject(List<double> toSerialize, string fileName)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<double>));
                xmlSerializer.Serialize(writer, toSerialize);
            }
        }
    }
}
