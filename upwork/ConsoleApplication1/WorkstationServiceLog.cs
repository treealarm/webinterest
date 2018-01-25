using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ConsoleApplication1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "WorkstationServiceLog" in both code and config file together.
    public class WorkstationServiceLogImp : IWorkstationServiceLog
    {
        public void WorkstationServiceLog(string workstation, string User, string Event, DateTime time)
        {
        }
    }
}
