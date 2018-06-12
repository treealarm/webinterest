
using MacGen;
namespace GCode
{
    public class clsMachine
    {
        //Store all settings in this class 
        public string Name = string.Empty;    //file name 
        public string Description = string.Empty;    //file name 
        public string ProgramId = string.Empty;
        public string Subcall = string.Empty;
        //call sub 
        public string SubRepeats = string.Empty;
        public string SubReturn = string.Empty;    //return from sub 
        public string Endmain = string.Empty;    //End of main program 
        public string BlockSkip = string.Empty;    //do not process lines that start with this 
        public string Comments = string.Empty;    //Comments 
        public MachineType MachineType = MachineType.MILL;    //lathe ,mill etc.. 
        public bool LatheMinus =false;    //this is for minus lathes 
        public bool HelixPitch = false;    //helix check box setting 
        public bool AbsArcCenter = false;    //Arc center chkbox 
        public int Precision;    //output precision 0.0001 
        public string Searchstring = string.Empty;    //a string that determines the setup record 
        public string[] Drills = new string[10];    //10 drilling cycles 0 index is the cancel code 
        public string[] ReturnLevel = new string[2];
        public string DrillRapid = string.Empty;
        public string Rapid = string.Empty;
        public string Linear = string.Empty;
        public string CCArc = string.Empty;
        public string CWArc = string.Empty;
        public string Incremental = string.Empty;
        public string Absolute = string.Empty;
        public string XYplane = string.Empty;
        public string XZplane = string.Empty;
        public string YZplane = string.Empty;
        public float[] ViewAngles = new float[3];    //Store pitch,roll,yaw 
        public string Rotary = string.Empty;    //Rotary axis code ABC 
        public RotaryDirection RotaryDir = RotaryDirection.CW;    //+1 or -1 
        public Axis RotaryAxis = Axis.Z;    //XYZ 
        public RotaryMotionType RotaryType = RotaryMotionType.BMC;
        public int RotPrecision = 1;    //output precision 0.0001 
        public float[] ViewShift = new float[3];    //Shift the view for viewing 

        public clsMachine(string name)
        {
            this.Name = name;
        }
        public clsMachine()
        {
        }
    }
}