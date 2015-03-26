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
    [Serializable]
    public class ProfileElement
    {
        public string FileName { get; set; }
        public int Length { get; set; }
        public double Angle { get; set; }
        public string FileNameCurve { get; set; }
        public double InitialScale { get; set; }

        public BitmapImage GetImage(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }
            BitmapImage image = new BitmapImage();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.BeginInit();
            image.UriSource = new Uri(fileName, UriKind.RelativeOrAbsolute);
            image.EndInit();
            return image;
        }
    }

    public class ProfileElementList : ObservableCollection<ProfileElement>
    {
    }
    public static class ProfileElementSerializer
    {
        public static ProfileElementList DeserializeObject(string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProfileElementList));
            StringReader textReader = new StringReader(toDeserialize);
            return xmlSerializer.Deserialize(textReader) as ProfileElementList;
        }

        public static string SerializeObject(ProfileElementList toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProfileElementList));
            StringWriter textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }
    }

    public class PathValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return Path.GetFileName(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
