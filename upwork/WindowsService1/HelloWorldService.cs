using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace WindowsService1
{
    [ServiceContract]
    public interface IHelloWorldService
    {
        [OperationContract]
        [WebGet]
        string GetDataCenter();

        [OperationContract]
        [WebGet]
        void SetDataCenter(string newIp);
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
        public void SetDataCenter(string newIp)
        {
            try
            {
                AppSettings.Default.DataCenter = newIp;
                AppSettings.Default.Save();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
