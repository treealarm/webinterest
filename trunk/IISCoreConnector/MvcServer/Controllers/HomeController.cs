using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;
using System.IO;
using System.Web.UI;

namespace MvcServer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public void TreeNodeExpanded(object sender, TreeNodeEventArgs e)
        {
        }
        public void TreeNodeCollapsed(object sender, TreeNodeEventArgs e)
        {

        }
        public void SelectedNodeChanged(object sender, EventArgs e)
        {
        }
        public ActionResult Configurator(string SelectedItem = "")
        {
            ViewBag.Message = "Context";
            TreeView tw = new TreeView();
            tw.TreeNodeExpanded += TreeNodeExpanded;
            tw.TreeNodeCollapsed += TreeNodeCollapsed;
            tw.SelectedNodeChanged += SelectedNodeChanged;
   
            tw.ID = "UITree";
            tw.ShowLines = false;
            tw.ImageSet = TreeViewImageSet.Simple;
            //
            TreeNode tn = new TreeNode();
            tn.Text = "sss";
            tn.Value = "sss";
            tn.SelectAction = TreeNodeSelectAction.SelectExpand;
            tn.NavigateUrl = "Configurator?SelectedItem=test";
            
            //
            TreeNode tn1 = new TreeNode();
            tn1.Text = "111";
            tn1.Value = "111";
            tn1.SelectAction = TreeNodeSelectAction.SelectExpand;

            TreeNode tn2 = new TreeNode();
            tn2.Text = "222";
            tn2.Value = "222";
            tn2.SelectAction = TreeNodeSelectAction.SelectExpand;

            TreeNode tn3 = new TreeNode();
            tn3.Text = "333";
            tn3.Value = "333";
            tn3.SelectAction = TreeNodeSelectAction.SelectExpand;
            ///
            tn.ChildNodes.Add(tn1);
            tn.ChildNodes.Add(tn2);
            tn.ChildNodes.Add(tn3);
            //
            TreeNode tn4 = new TreeNode();
            tn4.Text = "444";
            tn4.Value = "4";
            tn3.ChildNodes.Add(tn4);
            tw.Nodes.Add(tn);
          
            HtmlForm hf = new HtmlForm();
            hf.Controls.Add(tw);

            Page page = new Page();
            string controlOutput = string.Empty;
            page.Controls.Add(hf);

            StringBuilder sb = new StringBuilder ();// carriage a
            StringWriter sw = new StringWriter (sb); // cart twenty

            HtmlTextWriter HTW = new HtmlTextWriter(sw); // carriage three

            Server.Execute(page, (TextWriter)HTW, false);// call this method will brief content convert output stream
            controlOutput = sb. ToString ();// will HTML output stream into a string.

            ViewBag.TreeView1 = controlOutput;
            return View();
        }
    }
}
