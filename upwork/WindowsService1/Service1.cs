using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.InteropServices;
using System.ServiceModel.Web;

namespace WindowsService1
{
    public partial class AService1 : ServiceBase
    {

        public AService1()
        {
            InitializeComponent();
        }

        public ServiceHost serviceHost = null;

        public Dictionary<string, ServiceState> m_states = new Dictionary<string, ServiceState>();
        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("Starting");

            #if (DEBUG)
            System.Diagnostics.Debugger.Launch();
            #endif
            
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            try
            {
                serviceHost = new ServiceHost(typeof(HelloWorldService));

                serviceHost.Open();

                SetState("OnStart");
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message);
            }
        }

        protected void SetState(string state, int sessionId = -1)
        {
            EventLog.WriteEntry(state);
            string username = GetUsername(sessionId);

            ServiceState pState = new ServiceState(state, username);
            lock (m_states)
            {
                m_states[state] = pState;
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.DataCenter))
            {
                using (WebChannelFactory<IHelloWorldService> myChannelFactory = 
                    new WebChannelFactory<IHelloWorldService>(
                        new Uri("http://" + Properties.Settings.Default.DataCenter + ":8000/DataCenter/")))
                {
                    IHelloWorldService client = null;

                    try
                    {
                        client = myChannelFactory.CreateChannel();
                        client.SetStatus(pState);
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry(ex.Message);

                        if (client != null)
                        {
                            ((ICommunicationObject)client).Abort();
                        }
                    }
                }
            }
        }
        protected override void OnStop()
        {
            SetState("OnStop");
            
            if (serviceHost != null)
            {
                try
                {
                    serviceHost.Close();
                    serviceHost = null;
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry(ex.Message);
                }
            }
        }

        protected override void OnShutdown()
        {
            SetState("OnShutdown");
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            SetState("OnPowerEvent");
            return true;
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            try
            {
                int sessionId = changeDescription.SessionId;

                switch (changeDescription.Reason)
                {
                    case SessionChangeReason.SessionLogon:
                        {
                            SetState("SessionLogon", sessionId);
                            break;
                        }

                    case SessionChangeReason.SessionLogoff:
                        {
                            SetState("SessionLogoff", sessionId);
                            break;
                        }

                    case SessionChangeReason.ConsoleConnect:
                        SetState("ConsoleConnect", sessionId);
                        break;

                    case SessionChangeReason.ConsoleDisconnect:
                        SetState("ConsoleDisconnect", sessionId);
                        break;

                    case SessionChangeReason.RemoteConnect:
                        SetState("RemoteConnect", sessionId);
                        break;

                    case SessionChangeReason.RemoteDisconnect:
                        SetState("RemoteDisconnect", sessionId);
                        break;

                    case SessionChangeReason.SessionLock:
                        SetState("SessionLock", sessionId);
                        break;

                    case SessionChangeReason.SessionUnlock:
                        SetState("SessionUnlock", sessionId);
                        break;

                    case SessionChangeReason.SessionRemoteControl:
                        SetState("SessionRemoteControl", sessionId);
                        break;

                }

            }
            catch (System.Exception ex)
            {
                EventLog.WriteEntry("Exception: IntellectRunSrv.OnSessionChange. " + ex.Message);
            }
        }

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);
        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pointer);

        private enum WtsInfoClass
        {
            WTSUserName = 5,
            WTSDomainName = 7,
        }

        private static string GetUsername(int sessionId, bool prependDomain = true)
        {
            if (sessionId < 0)
            {
                return string.Empty;
            }
            IntPtr buffer;
            int strLen;
            string username = "SYSTEM";
            if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) && strLen > 1)
            {
                username = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory(buffer);
                if (prependDomain)
                {
                    if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out buffer, out strLen) && strLen > 1)
                    {
                        username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;
                        WTSFreeMemory(buffer);
                    }
                }
            }
            return username;
        }
    }
}
