using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using ConsoleApplication1;
using System.ServiceModel.Web;
using System.ServiceModel;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:52323/api/WorkstationServiceLog");
            //request.Method = "POST";
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.AllowAutoRedirect = false;

            ////Put the post data into the request
            //byte[] data = (new ASCIIEncoding()).GetBytes("workstation=efjcnwmc&User=voemk&Event=cfhwb&time=1-1-2030");
            //request.ContentLength = data.Length;
            //Stream reqStream = request.GetRequestStream();
            //reqStream.Write(data, 0, data.Length);
            //reqStream.Close();

            using (WebChannelFactory<IWorkstationServiceLog> myChannelFactory =
                                                new WebChannelFactory<IWorkstationServiceLog>(
                                                    new Uri("http://localhost:52323/api")))
            {
                IWorkstationServiceLog client = null;

                try
                {
                    client = myChannelFactory.CreateChannel();
                    MemoryStream stream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(stream, Encoding.ASCII);
                    string s = string.Format("workstation={0}&User={1}&Event={2}&time={3}",
                            "MachineName", "UserName", "State", DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"));
                    
                    writer.Write(s);
                    writer.Flush();
                    stream.Position = 0;

                    client.WorkstationServiceLog(stream);
                    
                }
                catch (Exception ex)
                {
                    if (client != null)
                    {
                        ((ICommunicationObject)client).Abort();
                    }
                }
            }
        }
    }
}
