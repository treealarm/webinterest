using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Web;

namespace ConsoleApplication1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "WorkstationServiceLog" in both code and config file together.
    public class WorkstationServiceLogImp : IWorkstationServiceLog
    {
        public void WorkstationServiceLog(Stream input)
        {
            string body = new StreamReader(input).ReadToEnd();
            NameValueCollection nvc = HttpUtility.ParseQueryString(body);
            try
            {
                string Workstation = nvc["Workstation"];
                string User = nvc["User"];
                string Event = nvc["Event"];
                DateTime Time = new DateTime();
                DateTime.TryParse(nvc["Time"], out Time); 
            }
            catch(Exception ex)
            {
            }
        }
    }
}
