using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.IO;

namespace WindowsService1
{
    [ServiceContract(Namespace = "")]
    [XmlSerializerFormat]
    public interface IHelloWorldService
    {
        [OperationContract]
        [WebGet]
        string GetDataCenter();

        [OperationContract]
        [WebGet]
        string SetDataCenter(string newIp);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream Form();
    }

    public class HelloWorldService : IHelloWorldService
    {
        //http://localhost:8000/DataCenter/GetDataCenter
        public string GetDataCenter()
        {
            string s = string.Empty;
            try
            {
                s = AppSettings.Default.DataCenter.ToString();
            }
            catch (Exception ex)
            {
            }
            return s;
        }

        //http://localhost:8000/DataCenter/SetDataCenter?newIp=127.0.0.1
        public string SetDataCenter(string newIp)
        {
            try
            {
                AppSettings.Default.DataCenter = newIp;
                AppSettings.Default.Save();
            }
            catch (Exception ex)
            {
                return "ERROR";
            }
            return "OK";
        }

        public Stream Form()
        {
            string html = Properties.Resources.index.Replace("newip_val", AppSettings.Default.DataCenter);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return new MemoryStream(Encoding.UTF8.GetBytes(html));
        }
    }
}
