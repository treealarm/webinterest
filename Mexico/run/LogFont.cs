using System.Runtime.InteropServices;
using System.Text;

namespace StateStat.Common
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class LogFont
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lfFaceName = string.Empty;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("LOGFONT\n");
            sb.AppendFormat("   lfHeight: {0}\n", lfHeight);
            sb.AppendFormat("   lfWidth: {0}\n", lfWidth);
            sb.AppendFormat("   lfEscapement: {0}\n", lfEscapement);
            sb.AppendFormat("   lfOrientation: {0}\n", lfOrientation);
            sb.AppendFormat("   lfWeight: {0}\n", lfWeight);
            sb.AppendFormat("   lfItalic: {0}\n", lfItalic);
            sb.AppendFormat("   lfUnderline: {0}\n", lfUnderline);
            sb.AppendFormat("   lfStrikeOut: {0}\n", lfStrikeOut);
            sb.AppendFormat("   lfCharSet: {0}\n", lfCharSet);
            sb.AppendFormat("   lfOutPrecision: {0}\n", lfOutPrecision);
            sb.AppendFormat("   lfClipPrecision: {0}\n", lfClipPrecision);
            sb.AppendFormat("   lfQuality: {0}\n", lfQuality);
            sb.AppendFormat("   lfPitchAndFamily: {0}\n", lfPitchAndFamily);
            sb.AppendFormat("   lfFaceName: {0}\n", lfFaceName);
            return sb.ToString();
        }
    }
}
