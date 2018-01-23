using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace WindowsService1
{
    public partial class AService1 : ServiceBase
    {
        public AService1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            EventLog.WriteEntry("OnStart");
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("OnStop");
        }

        protected override void OnShutdown()
        {
            EventLog.WriteEntry("OnShutdown");
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            EventLog.WriteEntry("OnPowerEvent");
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
                            EventLog.WriteEntry("SessionChangeReason.SessionLogon, SessionId: " + changeDescription.SessionId);
                            break;
                        }

                    case SessionChangeReason.SessionLogoff:
                        {
                            EventLog.WriteEntry("SessionChangeReason.SessionLogoff, SessionId: " + changeDescription.SessionId);

                            break;
                        }

                    case SessionChangeReason.ConsoleConnect:
                        EventLog.WriteEntry("SessionChangeReason.ConsoleConnect, SessionId: " + changeDescription.SessionId);
                        break;

                    case SessionChangeReason.ConsoleDisconnect:
                        EventLog.WriteEntry("SessionChangeReason.ConsoleDisconnect, SessionId: " + changeDescription.SessionId);
                        break;

                    case SessionChangeReason.RemoteConnect:
                        EventLog.WriteEntry("SessionChangeReason.RemoteConnect, SessionId: " + changeDescription.SessionId);
                        break;

                    case SessionChangeReason.RemoteDisconnect:
                        EventLog.WriteEntry("SessionChangeReason.RemoteDisconnect, SessionId: " + changeDescription.SessionId);
                        break;

                    case SessionChangeReason.SessionLock:
                        EventLog.WriteEntry("SessionChangeReason.SessionLock, SessionId: " + changeDescription.SessionId);
                        break;

                    case SessionChangeReason.SessionUnlock:
                        EventLog.WriteEntry("SessionChangeReason.SessionUnlock, SessionId: " + changeDescription.SessionId);
                        break;

                    case SessionChangeReason.SessionRemoteControl:
                        EventLog.WriteEntry("SessionChangeReason.SessionRemoteControl, SessionId: " + changeDescription.SessionId);
                        break;


                }

            }
            catch (System.Exception ex)
            {
                EventLog.WriteEntry("Exception: IntellectRunSrv.OnSessionChange. " + ex.Message);
            }
        }
    }
}
