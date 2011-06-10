using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using System.Text;
using System.IO;

namespace MvcServer.Controllers
{
    public class CoreController : Controller
    {
        //
        // GET: /Core/
        ITV.Serialization.XmlStringSerializer<List<string>> xml_list = 
            new ITV.Serialization.XmlStringSerializer<List<string>>();
        
        ITV.Serialization.XmlStringSerializer<ITV.Misc.Msg> xml_msg =
            new ITV.Serialization.XmlStringSerializer<ITV.Misc.Msg>();
        WizCoreInterface.WizCoreInterfaceImp core = new WizCoreInterface.WizCoreInterfaceImp();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NotifyEvent(string request = "")
        {
            core.NotifyEvent(request);
            return this.Content("OK");
        }

        public ActionResult GetObjectIds(string objtype)
        {
            List<string> list = new List<string>();
            core.GetObjectIds(objtype, list);
            xml_list.Serialize(list);
            return this.Content(xml_list.XML, "text/xml");
        }
        public ActionResult GetObjectChildIds(string objtype,string objid,string childtype)
        {
            List<string> list = new List<string>();
            core.GetObjectChildIds(objtype,objid,childtype, list);
            xml_list.Serialize(list);
            return this.Content(xml_list.XML, "text/xml");
        }
        public ActionResult GetObjectParams(string objtype, string objid)
        {
            ITV.Misc.Msg resp = core.GetObjectParams(objtype, objid);
            WizCoreInterface.MsgSerializer sr = new WizCoreInterface.MsgSerializer(resp);
            return this.Content(sr.XML, "text/xml");
        }
        public ActionResult GetObjectChildTypes(string objtype, int all = 0)
        {
            List<string> list = new List<string>();
            core.GetObjectChildTypes(objtype,list, all);
            xml_list.Serialize(list);
            return this.Content(xml_list.XML, "text/xml");
        }

    }
    
}
