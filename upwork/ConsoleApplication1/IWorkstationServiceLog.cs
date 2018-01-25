using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using System.IO;

//[21:13:28] David Fried: http://localhost:52323/api/WorkstationServiceLog/?workstation=efjcnwmc&User=voemk&Event=cfhwb&time=1-1-2030

//[21:14:49] cubicles97: m-d-yyyy
namespace ConsoleApplication1
{
    [DataContract]
    public class WorkstationServiceEventLogEntity
    {
        [DataMember]
        public string Workstation { get; set; }
        [DataMember]
        public string User { get; set; }
        [DataMember]
        public string Event { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
    }

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IWorkstationServiceLog" in both code and config file together.
    [ServiceContract(Namespace = "")]
    public interface IWorkstationServiceLog
    {
        [OperationContract]
        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/WorkstationServiceLog")]
        void WorkstationServiceLog(Stream data);
    }
}
