using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace WindowsService1
{
    public class LocalWorkstationState : ILocalWorkstationState
    {
        AService1 GetService()
        {
            return Program.g_AService;
        }

        //http://localhost:8000/DataCenter/GetApiPath
        public string GetApiPath()
        {
            GetService().ReadApiPath();
            return AService1.API_PATH;
        }

        public string SetApiPath(string apiPath, string redirect)
        {
            try
            {
                Properties.Settings.Default.ASPDataCenter = apiPath;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                return "ERROR";
            }
            if (!string.IsNullOrEmpty(redirect))
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Redirect;
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Location", redirect);
            }

            return "OK";
        }


        public List<ServiceState> GetLocalData()
        {
            List<ServiceState> list = new List<ServiceState>();
            lock (GetService().m_states)
            {
                foreach (KeyValuePair<string, ServiceState> item in GetService().m_states)
                {
                    list.Add(item.Value);
                }
                return list;
            }
        }

        public List<ServiceState> GetEvents(int pastNumberOfMinutes, string username, string eventname)
        {
            List<ServiceState> list = new List<ServiceState>();

            IEnumerable<EventLogEntry> coll = AService1.m_EventLog.Entries.OfType<EventLogEntry>();
            if (pastNumberOfMinutes > 0)
            {
                TimeSpan span = TimeSpan.FromMinutes(pastNumberOfMinutes);
                DateTime dt = DateTime.Now - span;
                coll = AService1.m_EventLog.Entries.
                    Cast<EventLogEntry>().Where(t => t.TimeGenerated > dt);

            }

            var jsSerializer = new JavaScriptSerializer();
            foreach (EventLogEntry e in coll)
            {
                try
                {
                    if (e.ReplacementStrings.Count() > 0)
                    {
                        ServiceState state = jsSerializer.Deserialize<ServiceState>(e.ReplacementStrings[0]);
                        list.Add(state);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            
            return list;
        }
    }
}
