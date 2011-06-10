using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Runtime.InteropServices;

namespace WizCoreInterface
{

    public class WizCoreInterfaceImp
    {
        const int BUF_SIZE = 1024 * 1024 * 10;
        const string m_name_mem_file_result = "Global\\CORE_REMOTE";
        MemoryMappedFile m_hMapFile = null;
        MemoryMappedViewAccessor m_pBuf = null;
        MessageHelper msg = new MessageHelper();

        public WizCoreInterfaceImp()
        {
            OpenMemFile();
        }
        void CloseMemFile()
        {
            if (m_pBuf != null)
            {
                m_pBuf = null;
            }

            if (m_hMapFile != null)
            {
                m_hMapFile = null;
            }
        }
        void OpenMemFile()
	    {
		    CloseMemFile();
            m_hMapFile = MemoryMappedFile.CreateOrOpen(m_name_mem_file_result,
                BUF_SIZE,
                MemoryMappedFileAccess.ReadWrite);        // name of mapping object 

            m_pBuf = m_hMapFile.CreateViewAccessor();
	    }
        string SendCommand(string data, IntPtr dw_data)
	    {
		    int hwnd = msg.getWindowId(null,"!CORE_REMOTE!");
            msg.sendWindowsStringMessage(hwnd,0,data,dw_data);
		    string str = ReceiveParam();
		    return str;
	    }
	    string ReceiveParam()
	    {
		    if(m_pBuf == null)
		    {
			    OpenMemFile();
			    if(m_pBuf == null)
			    {
                    return string.Empty;
			    }			
		    }
            Int32 len = 0;
            int lenSize = Marshal.SizeOf(typeof(Int32));
            m_pBuf.Read(0, out len);
            using (MemoryMappedViewStream stream = m_hMapFile.CreateViewStream())
            {
                BinaryReader reader = new BinaryReader(stream,Encoding.Default);
                char[] asciiChars = new char[len];
                stream.Position = lenSize;
                int readed = reader.Read(asciiChars, 0, len);

                if (readed == len)
                {
                    string asciiString = new string(asciiChars);
                    return asciiString;
                }

            }
		    return string.Empty;
	    }

        public string NotifyEvent(string request)
        {
           return SendCommand(request, (IntPtr)2);
        }
    
        string DoRequestWrap(string request)
	    {
            string result = SendCommand(request,(IntPtr)1);
		    return result;
	    }
        ITV.Misc.Msg DoRequestWrap(ITV.Misc.Msg request)
	    {
		    string s = request.ToString();
		    //LLOG("--> %s", s);
		    s = DoRequestWrap(s);
		    //LLOG("<-- %s", s);
            ITV.Misc.Msg ret = new ITV.Misc.Msg(s);
		    return ret;
	    }
        void MsgToArray(List<string> ar, ITV.Misc.Msg msg)
	    {
		    int count = msg.GetParamAsInt32("count");
		
		    for(int i = 0; i < count; i++)
		    {
			    string s;
			    s = string.Format("{0}",i);
			    s = msg.GetParam(s);
			    ar.Add(s);
		    }
	    }
        public int GetObjectIds(string objtype, List<string> list, string main_id = "")
	    {
            ITV.Misc.Msg request = new ITV.Misc.Msg(objtype, "", "GetObjectIds");
		    ITV.Misc.Msg msg = DoRequestWrap(request);
		    MsgToArray(list,msg);
		    return list.Count;
	    }
       	public int GetObjectChildIds(string objtype, string objid, string childtype, List<string> list)
	    {
		    ITV.Misc.Msg request = new ITV.Misc.Msg(objtype,objid,"GetObjectChildIds");
		    request.SetParam("childtype",childtype);
		    ITV.Misc.Msg msg = DoRequestWrap(request);
		    MsgToArray(list,msg);
		    return list.Count;
	    }
        public ITV.Misc.Msg GetObjectParams(string objtype, string id)
	    {
		    ITV.Misc.Msg request = new ITV.Misc.Msg(objtype,id,"GetObjectParams");
		    return DoRequestWrap(request);
	    }
        public int GetObjectChildTypes(string objtype,List<string> list, int all = 0)
	    {
            ITV.Misc.Msg request = new ITV.Misc.Msg(objtype, "", "GetObjectChildTypes");
		    request.SetParam("all",all);
		    ITV.Misc.Msg msg = DoRequestWrap(request);
		    MsgToArray(list,msg);
		    return list.Count;
	    }
    
    }
}
