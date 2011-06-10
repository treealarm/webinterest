using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

namespace WizCoreInterface
{
    public class Utf8StringWriter : StringWriter { public override Encoding Encoding { get { return Encoding.UTF8; } } } 
    public class MsgSerializer
    {
        public string XML = string.Empty;
        public MsgSerializer(ITV.Misc.Msg msg)
        {
            Utf8StringWriter stringWriter = new Utf8StringWriter();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter); 
            xmlTextWriter.Formatting = Formatting.Indented; 
            xmlTextWriter.WriteStartDocument(); //Start doc 
            WriteXml(xmlTextWriter, msg);
            xmlTextWriter.WriteEndDocument();
            xmlTextWriter.Close();
            XML = stringWriter.ToString();
        }
        public void WriteXml(XmlWriter writer, ITV.Misc.Msg msg)
        {
            writer.WriteStartElement("MSG");
            writer.WriteStartElement("SOURCE_TYPE");
            writer.WriteString(msg.SourceType);
            writer.WriteEndElement();
            writer.WriteStartElement("SOURCE_ID");
            writer.WriteString(msg.SourceId);
            writer.WriteEndElement();
            writer.WriteStartElement("ACTION");
            writer.WriteString(msg.Action);
            writer.WriteEndElement();
            writer.WriteStartElement("Params");
            foreach (KeyValuePair<string, string> o in msg)
            {
                writer.WriteStartElement(o.Key);
                writer.WriteCData(o.Value);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}
