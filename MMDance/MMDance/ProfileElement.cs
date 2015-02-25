using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Xml.Serialization;

namespace MMDance
{
    [Serializable]
    public class ProfileElement
    {
        public string FileName { get; set; }
        public int Length { get; set; }
        
        public BitmapImage Image
        {
            get
            {
                if (!File.Exists(FileName))
                {
                    return null;
                }
                BitmapImage image = new BitmapImage();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.BeginInit();
                image.UriSource = new Uri(FileName, UriKind.RelativeOrAbsolute); 
                image.EndInit();
                return image;
            }
        }

    }
    public static class ProfileElementSerializer
    {
        public static List<ProfileElement> DeserializeObject(string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ProfileElement>));
            StringReader textReader = new StringReader(toDeserialize);
            return xmlSerializer.Deserialize(textReader) as List<ProfileElement>;
        }

        public static string SerializeObject(List<ProfileElement> toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ProfileElement>));
            StringWriter textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }
    }
}
