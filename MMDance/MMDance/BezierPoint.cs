using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Data;

namespace BezierCurve
{

    public class BezierPoint
    {
        public BezierPoint()
        {

        }
        public BezierPoint(Point pt)
        {
            X = pt.X;
            Y = pt.Y;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public Point point 
        { 
            get
            {
                return new Point(X, Y);
            }
        }
    }
    public class BezierSegment
    {
        public List<BezierPoint> Points { get; set; }
        public int Number { get; set; }
        public BezierSegment()
        {
            Points = new List<BezierPoint>();
        }
        public List<Point> GetPoints()
        {
            List<Point> pts = new List<Point>();
            foreach (BezierPoint pt in Points)
            {
                pts.Add(pt.point);
            }
            return pts;
        }
    }

    public class BezierViewModel : DependencyObject
    {
        public List<BezierSegment> Segments { get; set; }

        public BezierViewModel()
        {
            Segments = new List<BezierSegment>();
        }
    }

    public static class BezierViewModelSerializer
    {
        public static BezierViewModel DeserializeObject(string toDeserialize)
        {
            BezierViewModel ret = new BezierViewModel();
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(BezierViewModel));
                StringReader textReader = new StringReader(toDeserialize);
                ret = xmlSerializer.Deserialize(textReader) as BezierViewModel;
            }
            catch (System.Exception ex)
            {
            	
            }

            return ret;
        }

        public static string SerializeObject(BezierViewModel toSerialize)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(BezierViewModel));
                StringWriter textWriter = new StringWriter();
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
            catch (System.Exception ex)
            {
            	
            }
            return string.Empty;
        }
    }

    public class BezierViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;
            if (str == null)
            {
                return null;
            }
            return BezierViewModelSerializer.DeserializeObject(str);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }
            BezierViewModel model = value as BezierViewModel;
            if (model == null)
            {
                return string.Empty;
            }
            return BezierViewModelSerializer.SerializeObject(model);
        }
    }
}
