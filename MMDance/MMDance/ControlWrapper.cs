//-------------------------------------------------------------------------------------------------------------------------------------------------------------------
//-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------
/********************************************************************
FileName:		Form1.cs
Dependencies:	When compiled, needs .NET framework 2.0 redistributable to run.
Hardware:		Need a free USB port to connect USB peripheral device
			programmed with appropriate Generic HID firmware.  VID and
			PID in firmware must match the VID and PID in this
			program.
Compiler:  	Microsoft Visual C# 2005 Express Edition (or better)
Company:		Microchip Technology, Inc.

Software License Agreement:

The software supplied herewith by Microchip Technology Incorporated
(the “Company”) for its PIC® Microcontroller is intended and
supplied to you, the Company’s customer, for use solely and
exclusively with Microchip PIC Microcontroller products. The
software is owned by the Company and/or its supplier, and is
protected under applicable copyright laws. All rights are reserved.
Any use in violation of the foregoing restrictions may subject the
user to criminal sanctions under applicable laws, as well as to
civil liability for the breach of the terms and conditions of this
license.

THIS SOFTWARE IS PROVIDED IN AN “AS IS” CONDITION. NO WARRANTIES,
WHETHER EXPRESS, IMPLIED OR STATUTORY, INCLUDING, BUT NOT LIMITED
TO, IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
PARTICULAR PURPOSE APPLY TO THIS SOFTWARE. THE COMPANY SHALL NOT,
IN ANY CIRCUMSTANCES, BE LIABLE FOR SPECIAL, INCIDENTAL OR
CONSEQUENTIAL DAMAGES, FOR ANY REASON WHATSOEVER.

********************************************************************
File Description:

Change History:
Rev   Date         Description
2.5a	07/17/2009	 Initial Release.  Ported from HID PnP Demo
                    application source, which was originally 
                    written in MSVC++ 2005 Express Edition.
********************************************************************
NOTE:	All user made code contained in this project is in the Form1.cs file.
	All other code and files were generated automatically by either the
	new project wizard, or by the development environment (ex: code is
	automatically generated if you create a new button on the form, and
	then double click on it, which creates a click event handler
	function).  User developed code (code not developed by the IDE) has
    been marked in "cut and paste blocks" to make it easier to identify.
********************************************************************/

//NOTE: In order for this program to "find" a USB device with a given VID and PID, 
//both the VID and PID in the USB device descriptor (in the USB firmware on the 
//microcontroller), as well as in this PC application source code, must match.
//To change the VID/PID in this PC application source code, scroll down to the 
//CheckIfPresentAndGetUSBDevicePath() function, and change the line that currently
//reads:

//   String DeviceIDToFind = "Vid_04d8&Pid_003f";


//NOTE 2: This C# program makes use of several functions in setupapi.dll and
//other Win32 DLLs.  However, one cannot call the functions directly in a 
//32-bit DLL if the project is built in "Any CPU" mode, when run on a 64-bit OS.
//When configured to build an "Any CPU" executable, the executable will "become"
//a 64-bit executable when run on a 64-bit OS.  On a 32-bit OS, it will run as 
//a 32-bit executable, and the pointer sizes and other aspects of this 
//application will be compatible with calling 32-bit DLLs.

//Therefore, on a 64-bit OS, this application will not work unless it is built in
//"x86" mode.  When built in this mode, the exectuable always runs in 32-bit mode
//even on a 64-bit OS.  This allows this application to make 32-bit DLL function 
//calls, when run on either a 32-bit or 64-bit OS.

//By default, on a new project, C# normally wants to build in "Any CPU" mode.  
//To switch to "x86" mode, open the "Configuration Manager" window.  In the 
//"Active solution platform:" drop down box, select "x86".  If this option does
//not appear, select: "<New...>" and then select the x86 option in the 
//"Type or select the new platform:" drop down box.  

//-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using MMDance;


public class ControlWrapper
{
    public const int MOTORS_COUNT = 4;
    public const int LEN_OF_PACKET = 65;
    public const byte COMMAND_TOGGLE_LED = 0x80;
    public const byte COMMAND_IS_AVAILABLE = 0x81;
    public const byte COMMAND_SET_STEPS = 0x82;
    public const byte COMMAND_SET_TIME = 0x83;
    public const byte COMMAND_SET_CONTROL_SIGNALS = 0x84;
    public const byte COMMAND_SET_PAUSE = 0x86;
    public const byte COMMAND_SET_INK = 0x87;
    public const byte COMMAND_ERRORS = 0x88;
    

    UInt16 m_timer_ink_impuls;

    [StructLayout(LayoutKind.Sequential, Size = 4 * ControlWrapper.MOTORS_COUNT), Serializable]
    internal class do_steps
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = ControlWrapper.MOTORS_COUNT)]
        public Int32[] m_uSteps;
    }
    do_steps m_cur_steps = new do_steps();

    public string GetCurText()
    {
        if (m_cur_steps.m_uSteps == null)
        {
            return string.Empty;
        }
        string ret;
        ret = string.Format("{0},{1},{2},{3},{4}",
            m_cur_steps.m_uSteps[0], m_cur_steps.m_uSteps[1], m_cur_steps.m_uSteps[2], m_cur_steps.m_uSteps[3],
            m_timer_ink_impuls);
        return ret;
    }
    public static void StructureToByteArray(object obj, byte[] bytearray, int position)
    {
        int len = Marshal.SizeOf(obj);

        IntPtr ptr = Marshal.AllocHGlobal(len);

        Marshal.StructureToPtr(obj, ptr, true);

        Marshal.Copy(ptr, bytearray, position, len);

        Marshal.FreeHGlobal(ptr);
        if (position + len > bytearray.Length - 2)
        {
            MessageBox.Show("too long data");
        }
    }

    public static void ByteArrayToStructure<T>(byte[] bytearray, ref T structureObj, int position)
    {
        int length = Marshal.SizeOf(structureObj);
        IntPtr ptr = Marshal.AllocHGlobal(length);
        Marshal.Copy(bytearray, position, ptr, length);
        structureObj = (T)Marshal.PtrToStructure(ptr, structureObj.GetType());
        Marshal.FreeHGlobal(ptr);
    }

    public bool IsControllerAvailable()
    {
        if (!IsOpen())
        {
            return false;
        }
        Byte[] OUTBuffer = new byte[LEN_OF_PACKET];	//Allocate a memory buffer equal to the OUT endpoint size + 1
        Byte[] INBuffer = new byte[LEN_OF_PACKET];		//Allocate a memory buffer equal to the IN endpoint size + 1
        uint BytesWritten = 0;
        uint BytesRead = 0;
        INBuffer[0] = 0;				//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.
        OUTBuffer[0] = 0;				//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.
        OUTBuffer[1] = COMMAND_IS_AVAILABLE;

        ushort crc16 = CRCHelper.crc16_ccitt(OUTBuffer, OUTBuffer.Length - 2);
        byte[] byteArray = BitConverter.GetBytes(crc16);
        OUTBuffer[OUTBuffer.Length - 2] = byteArray[0];
        OUTBuffer[OUTBuffer.Length - 1] = byteArray[1];

        if (WriteFile(WriteHandleToUSBDevice, OUTBuffer, LEN_OF_PACKET, ref BytesWritten, IntPtr.Zero))	//Blocking function, unless an "overlapped" structure is used
        {
            INBuffer[0] = 0;
            //Now get the response packet from the firmware.
            if (ReadFileManagedBuffer(ReadHandleToUSBDevice, INBuffer, 65, ref BytesRead, IntPtr.Zero))		//Blocking function, unless an "overlapped" structure is used	
            {
                do_steps buf = new do_steps();
                ByteArrayToStructure(INBuffer, ref m_cur_steps, 3);
                ByteArrayToStructure(INBuffer, ref buf, 3 + Marshal.SizeOf(m_cur_steps));
                ByteArrayToStructure(INBuffer, ref m_timer_ink_impuls, 3 + Marshal.SizeOf(m_cur_steps) * 2);

                for (int i = 0; i < MOTORS_COUNT; i++)
                {
                    if (Math.Abs(buf.m_uSteps[i]) > 1000000)
                    {
                        Debug.WriteLine("Something wrong");
                    }
                    if (Math.Abs(m_cur_steps.m_uSteps[i]) > 100000)
                    {
                        Debug.WriteLine("Something wrong");
                    }
                }

                if (INBuffer[2] == 0x01)
                {
                    return true;
                }
            }
        }
        else
        {
            return false;
        }
        return false;
    }

    public bool IsError()
    {
        if (!IsOpen())
        {
            return true;
        }

        Byte[] INBuffer = new byte[LEN_OF_PACKET];		//Allocate a memory buffer equal to the IN endpoint size + 1
        
        uint BytesRead = 0;
        INBuffer[0] = 0;				//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.
        //Now get the response packet from the firmware.
        if (ReadFileManagedBuffer(ReadHandleToUSBDevice, INBuffer, 65, ref BytesRead, IntPtr.Zero))		//Blocking function, unless an "overlapped" structure is used	
        {
            if (INBuffer[1] == COMMAND_ERRORS)
            {
                ushort received_crc = BitConverter.ToUInt16(INBuffer, LEN_OF_PACKET - 4);
                ushort cur_crc = BitConverter.ToUInt16(INBuffer, LEN_OF_PACKET - 2);
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public bool WriteCommandToController(Byte[] OUTBuffer)
    {
        if (!IsOpen())
        {
            return false;
        }
        
        uint BytesWritten = 0;

        bool bOk = WriteFile(WriteHandleToUSBDevice, OUTBuffer, LEN_OF_PACKET, ref BytesWritten, IntPtr.Zero);
        if (BytesWritten < LEN_OF_PACKET)
        {
            Debug.WriteLine("wrong len");
        }
        //while (IsError() && bOk)
        //{
        //    Thread.Sleep(1);
        //    bOk = WriteFile(WriteHandleToUSBDevice, OUTBuffer, LEN_OF_PACKET, ref BytesWritten, IntPtr.Zero);
        //}
        return bOk;
    }

    public bool IsOpen()
    {
        if (WriteHandleToUSBDevice == null)
        {
            return false;
        }
	    return !WriteHandleToUSBDevice.IsInvalid;
    }
    public bool Connect()
    {
        if (CheckIfPresentAndGetUSBDevicePath())	//Check and make sure at least one device with matching VID/PID is attached
        {
            uint ErrorStatusWrite;
            uint ErrorStatusRead;


            //We now have the proper device path, and we can finally open read and write handles to the device.
            WriteHandleToUSBDevice = CreateFile(DevicePath, GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            ErrorStatusWrite = (uint)Marshal.GetLastWin32Error();
            ReadHandleToUSBDevice = CreateFile(DevicePath, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            ErrorStatusRead = (uint)Marshal.GetLastWin32Error();

            if ((ErrorStatusWrite == ERROR_SUCCESS) && (ErrorStatusRead == ERROR_SUCCESS))
            {
                return true;
            }
            else //for some reason the device was physically plugged in, but one or both of the read/write handles didn't open successfully...
            {
                CloseController();
            }
        }
        return false;
    }

    public void CloseController()
    {
        if (!WriteHandleToUSBDevice.IsInvalid)
        {
            WriteHandleToUSBDevice.Close();
        }
        if (!ReadHandleToUSBDevice.IsInvalid)
        {
            ReadHandleToUSBDevice.Close();
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------

    //Constant definitions from setupapi.h, which we aren't allowed to include directly since this is C#
    internal const uint DIGCF_PRESENT = 0x02;
    internal const uint DIGCF_DEVICEINTERFACE = 0x10;
    //Constants for CreateFile() and other file I/O functions
    internal const short FILE_ATTRIBUTE_NORMAL = 0x80;
    internal const short INVALID_HANDLE_VALUE = -1;
    internal const uint GENERIC_READ = 0x80000000;
    internal const uint GENERIC_WRITE = 0x40000000;
    internal const uint CREATE_NEW = 1;
    internal const uint CREATE_ALWAYS = 2;
    internal const uint OPEN_EXISTING = 3;
    internal const uint FILE_SHARE_READ = 0x00000001;
    internal const uint FILE_SHARE_WRITE = 0x00000002;
    //Constant definitions for certain WM_DEVICECHANGE messages
    internal const uint WM_DEVICECHANGE = 0x0219;
    internal const uint DBT_DEVICEARRIVAL = 0x8000;
    internal const uint DBT_DEVICEREMOVEPENDING = 0x8003;
    internal const uint DBT_DEVICEREMOVECOMPLETE = 0x8004;
    internal const uint DBT_CONFIGCHANGED = 0x0018;
    //Other constant definitions
    internal const uint DBT_DEVTYP_DEVICEINTERFACE = 0x05;
    internal const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0x00;
    internal const uint ERROR_SUCCESS = 0x00;
    internal const uint ERROR_NO_MORE_ITEMS = 0x00000103;
    internal const uint SPDRP_HARDWAREID = 0x00000001;

    //Various structure definitions for structures that this code will be using
    internal struct SP_DEVICE_INTERFACE_DATA
    {
        internal uint cbSize;               //DWORD
        internal Guid InterfaceClassGuid;   //GUID
        internal uint Flags;                //DWORD
        internal uint Reserved;             //ULONG_PTR MSDN says ULONG_PTR is "typedef unsigned __int3264 ULONG_PTR;"  
    }

    internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
    {
        internal uint cbSize;               //DWORD
        internal char[] DevicePath;         //TCHAR array of any size
    }
        
    internal struct SP_DEVINFO_DATA
    {
        internal uint cbSize;       //DWORD
        internal Guid ClassGuid;    //GUID
        internal uint DevInst;      //DWORD
        internal uint Reserved;     //ULONG_PTR  MSDN says ULONG_PTR is "typedef unsigned __int3264 ULONG_PTR;"  
    }

    //DLL Imports.  Need these to access various C style unmanaged functions contained in their respective DLL files.
    //--------------------------------------------------------------------------------------------------------------
    //Returns a HDEVINFO type for a device information set.  We will need the 
    //HDEVINFO as in input parameter for calling many of the other SetupDixxx() functions.
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr SetupDiGetClassDevs(
        ref Guid ClassGuid,     //LPGUID    Input: Need to supply the class GUID. 
        IntPtr Enumerator,      //PCTSTR    Input: Use NULL here, not important for our purposes
        IntPtr hwndParent,      //HWND      Input: Use NULL here, not important for our purposes
        uint Flags);            //DWORD     Input: Flags describing what kind of filtering to use.

	//Gives us "PSP_DEVICE_INTERFACE_DATA" which contains the Interface specific GUID (different
	//from class GUID).  We need the interface GUID to get the device path.
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool SetupDiEnumDeviceInterfaces(
        IntPtr DeviceInfoSet,           //Input: Give it the HDEVINFO we got from SetupDiGetClassDevs()
        IntPtr DeviceInfoData,          //Input (optional)
        ref Guid InterfaceClassGuid,    //Input 
        uint MemberIndex,               //Input: "Index" of the device you are interested in getting the path for.
        ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);    //Output: This function fills in an "SP_DEVICE_INTERFACE_DATA" structure.

    //SetupDiDestroyDeviceInfoList() frees up memory by destroying a DeviceInfoList
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool SetupDiDestroyDeviceInfoList(
        IntPtr DeviceInfoSet);          //Input: Give it a handle to a device info list to deallocate from RAM.

    //SetupDiEnumDeviceInfo() fills in an "SP_DEVINFO_DATA" structure, which we need for SetupDiGetDeviceRegistryProperty()
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool SetupDiEnumDeviceInfo(
        IntPtr DeviceInfoSet,
        uint MemberIndex,
        ref SP_DEVINFO_DATA DeviceInterfaceData);

    //SetupDiGetDeviceRegistryProperty() gives us the hardware ID, which we use to check to see if it has matching VID/PID
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool SetupDiGetDeviceRegistryProperty(
        IntPtr DeviceInfoSet,
        ref SP_DEVINFO_DATA DeviceInfoData,
        uint Property,
        ref uint PropertyRegDataType,
        IntPtr PropertyBuffer,
        uint PropertyBufferSize,
        ref uint RequiredSize);

    //SetupDiGetDeviceInterfaceDetail() gives us a device path, which is needed before CreateFile() can be used.
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool SetupDiGetDeviceInterfaceDetail(
        IntPtr DeviceInfoSet,                   //Input: Wants HDEVINFO which can be obtained from SetupDiGetClassDevs()
        ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,                    //Input: Pointer to an structure which defines the device interface.  
        IntPtr  DeviceInterfaceDetailData,      //Output: Pointer to a SP_DEVICE_INTERFACE_DETAIL_DATA structure, which will receive the device path.
        uint DeviceInterfaceDetailDataSize,     //Input: Number of bytes to retrieve.
        ref uint RequiredSize,                  //Output (optional): The number of bytes needed to hold the entire struct 
        IntPtr DeviceInfoData);                 //Output (optional): Pointer to a SP_DEVINFO_DATA structure

    //Overload for SetupDiGetDeviceInterfaceDetail().  Need this one since we can't pass NULL pointers directly in C#.
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool SetupDiGetDeviceInterfaceDetail(
        IntPtr DeviceInfoSet,                   //Input: Wants HDEVINFO which can be obtained from SetupDiGetClassDevs()
        ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,               //Input: Pointer to an structure which defines the device interface.  
        IntPtr DeviceInterfaceDetailData,       //Output: Pointer to a SP_DEVICE_INTERFACE_DETAIL_DATA structure, which will contain the device path.
        uint DeviceInterfaceDetailDataSize,     //Input: Number of bytes to retrieve.
        IntPtr RequiredSize,                    //Output (optional): Pointer to a DWORD to tell you the number of bytes needed to hold the entire struct 
        IntPtr DeviceInfoData);                 //Output (optional): Pointer to a SP_DEVINFO_DATA structure

    //Need this function for receiving all of the WM_DEVICECHANGE messages.  See MSDN documentation for
    //description of what this function does/how to use it. Note: name is remapped "RegisterDeviceNotificationUM" to
    //avoid possible build error conflicts.
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr RegisterDeviceNotification(
        IntPtr hRecipient,
        IntPtr NotificationFilter,
        uint Flags);

    //Takes in a device path and opens a handle to the device.
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern SafeFileHandle CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode, 
        IntPtr lpSecurityAttributes, 
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes, 
        IntPtr hTemplateFile);

    //Uses a handle (created with CreateFile()), and lets us write USB data to the device.
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool WriteFile(
        SafeFileHandle hFile,
        byte[] lpBuffer,
        uint nNumberOfBytesToWrite,
        ref uint lpNumberOfBytesWritten,
        IntPtr lpOverlapped);

    //Uses a handle (created with CreateFile()), and lets us read USB data from the device.
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool ReadFile(
        SafeFileHandle hFile,
        IntPtr lpBuffer,
        uint nNumberOfBytesToRead,
        ref uint lpNumberOfBytesRead,
        IntPtr lpOverlapped);



	//--------------- Global Varibles Section ------------------
	    
    SafeFileHandle WriteHandleToUSBDevice = null;
    SafeFileHandle ReadHandleToUSBDevice = null;
    String DevicePath = null;   //Need the find the proper device path before you can open file handles.
	    

    //Globally Unique Identifier (GUID) for HID class devices.  Windows uses GUIDs to identify things.
    Guid InterfaceClassGuid = new Guid(0x4d1e55b2, 0xf16f, 0x11cf, 0x88, 0xcb, 0x00, 0x11, 0x11, 0x00, 0x00, 0x30); 
	//--------------- End of Global Varibles ------------------

    //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------


    //Need to check "Allow unsafe code" checkbox in build properties to use unsafe keyword.  Unsafe is needed to
    //properly interact with the unmanged C++ style APIs used to find and connect with the USB device.
    public ControlWrapper()
    {
    }


    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------

    //FUNCTION:	CheckIfPresentAndGetUSBDevicePath()
    //PURPOSE:	Check if a USB device is currently plugged in with a matching VID and PID
    //INPUT:	Uses globally declared String DevicePath, globally declared GUID, and the MY_DEVICE_ID constant.
    //OUTPUT:	Returns BOOL.  TRUE when device with matching VID/PID found.  FALSE if device with VID/PID could not be found.
    //			When returns TRUE, the globally accessable "DetailedInterfaceDataStructure" will contain the device path
    //			to the USB device with the matching VID/PID.

    bool CheckIfPresentAndGetUSBDevicePath()
    {
		/* 
		Before we can "connect" our application to our USB embedded device, we must first find the device.
		A USB bus can have many devices simultaneously connected, so somehow we have to find our device only.
		This is done with the Vendor ID (VID) and Product ID (PID).  Each USB product line should have
		a unique combination of VID and PID.  

		Microsoft has created a number of functions which are useful for finding plug and play devices.  Documentation
		for each function used can be found in the MSDN library.  We will be using the following functions (unmanaged C functions):

		SetupDiGetClassDevs()					//provided by setupapi.dll, which comes with Windows
		SetupDiEnumDeviceInterfaces()			//provided by setupapi.dll, which comes with Windows
		GetLastError()							//provided by kernel32.dll, which comes with Windows
		SetupDiDestroyDeviceInfoList()			//provided by setupapi.dll, which comes with Windows
		SetupDiGetDeviceInterfaceDetail()		//provided by setupapi.dll, which comes with Windows
		SetupDiGetDeviceRegistryProperty()		//provided by setupapi.dll, which comes with Windows
		CreateFile()							//provided by kernel32.dll, which comes with Windows

        In order to call these unmanaged functions, the Marshal class is very useful.
             
		We will also be using the following unusual data types and structures.  Documentation can also be found in
		the MSDN library:

		PSP_DEVICE_INTERFACE_DATA
		PSP_DEVICE_INTERFACE_DETAIL_DATA
		SP_DEVINFO_DATA
		HDEVINFO
		HANDLE
		GUID

		The ultimate objective of the following code is to get the device path, which will be used elsewhere for getting
		read and write handles to the USB device.  Once the read/write handles are opened, only then can this
		PC application begin reading/writing to the USB device using the WriteFile() and ReadFile() functions.

		Getting the device path is a multi-step round about process, which requires calling several of the
		SetupDixxx() functions provided by setupapi.dll.
		*/

        try
        {
		    IntPtr DeviceInfoTable = IntPtr.Zero;
		    SP_DEVICE_INTERFACE_DATA InterfaceDataStructure = new SP_DEVICE_INTERFACE_DATA();
            SP_DEVICE_INTERFACE_DETAIL_DATA DetailedInterfaceDataStructure = new SP_DEVICE_INTERFACE_DETAIL_DATA();
            SP_DEVINFO_DATA DevInfoData = new SP_DEVINFO_DATA();

		    uint InterfaceIndex = 0;
		    uint dwRegType = 0;
		    uint dwRegSize = 0;
            uint dwRegSize2 = 0;
		    uint StructureSize = 0;
		    IntPtr PropertyValueBuffer = IntPtr.Zero;
		    bool MatchFound = false;
		    uint ErrorStatus;
		    uint LoopCounter = 0;

            //Use the formatting: "Vid_xxxx&Pid_xxxx" where xxxx is a 16-bit hexadecimal number.
            //Make sure the value appearing in the parathesis matches the USB device descriptor
            //of the device that this aplication is intending to find.
            String DeviceIDToFind = "Vid_04d8&Pid_003f";

		    //First populate a list of plugged in devices (by specifying "DIGCF_PRESENT"), which are of the specified class GUID. 
		    DeviceInfoTable = SetupDiGetClassDevs(ref InterfaceClassGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

            if(DeviceInfoTable != IntPtr.Zero)
            {
		        //Now look through the list we just populated.  We are trying to see if any of them match our device. 
		        while(true)
		        {
                    InterfaceDataStructure.cbSize = (uint)Marshal.SizeOf(InterfaceDataStructure);
			        if(SetupDiEnumDeviceInterfaces(DeviceInfoTable, IntPtr.Zero, ref InterfaceClassGuid, InterfaceIndex, ref InterfaceDataStructure))
			        {
                        ErrorStatus = (uint)Marshal.GetLastWin32Error();
                        if (ErrorStatus == ERROR_NO_MORE_ITEMS)	//Did we reach the end of the list of matching devices in the DeviceInfoTable?
				        {	//Cound not find the device.  Must not have been attached.
					        SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
					        return false;		
				        }
			        }
			        else	//Else some other kind of unknown error ocurred...
			        {
                        ErrorStatus = (uint)Marshal.GetLastWin32Error();
				        SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
				        return false;	
			        }

			        //Now retrieve the hardware ID from the registry.  The hardware ID contains the VID and PID, which we will then 
			        //check to see if it is the correct device or not.

			        //Initialize an appropriate SP_DEVINFO_DATA structure.  We need this structure for SetupDiGetDeviceRegistryProperty().
                    DevInfoData.cbSize = (uint)Marshal.SizeOf(DevInfoData);
			        SetupDiEnumDeviceInfo(DeviceInfoTable, InterfaceIndex, ref DevInfoData);

			        //First query for the size of the hardware ID, so we can know how big a buffer to allocate for the data.
			        SetupDiGetDeviceRegistryProperty(DeviceInfoTable, ref DevInfoData, SPDRP_HARDWAREID, ref dwRegType, IntPtr.Zero, 0, ref dwRegSize);

			        //Allocate a buffer for the hardware ID.
                    //Should normally work, but could throw exception "OutOfMemoryException" if not enough resources available.
                    PropertyValueBuffer = Marshal.AllocHGlobal((int)dwRegSize);

			        //Retrieve the hardware IDs for the current device we are looking at.  PropertyValueBuffer gets filled with a 
			        //REG_MULTI_SZ (array of null terminated strings).  To find a device, we only care about the very first string in the
			        //buffer, which will be the "device ID".  The device ID is a string which contains the VID and PID, in the example 
			        //format "Vid_04d8&Pid_003f".
                    SetupDiGetDeviceRegistryProperty(DeviceInfoTable, ref DevInfoData, SPDRP_HARDWAREID, ref dwRegType, PropertyValueBuffer, dwRegSize, ref dwRegSize2);

			        //Now check if the first string in the hardware ID matches the device ID of the USB device we are trying to find.
			        String DeviceIDFromRegistry = Marshal.PtrToStringUni(PropertyValueBuffer); //Make a new string, fill it with the contents from the PropertyValueBuffer

			        Marshal.FreeHGlobal(PropertyValueBuffer);		//No longer need the PropertyValueBuffer, free the memory to prevent potential memory leaks

			        //Convert both strings to lower case.  This makes the code more robust/portable accross OS Versions
			        DeviceIDFromRegistry = DeviceIDFromRegistry.ToLowerInvariant();	
			        DeviceIDToFind = DeviceIDToFind.ToLowerInvariant();				
			        //Now check if the hardware ID we are looking at contains the correct VID/PID
			        MatchFound = DeviceIDFromRegistry.Contains(DeviceIDToFind);		
			        if(MatchFound == true)
			        {
                        //Device must have been found.  In order to open I/O file handle(s), we will need the actual device path first.
				        //We can get the path by calling SetupDiGetDeviceInterfaceDetail(), however, we have to call this function twice:  The first
				        //time to get the size of the required structure/buffer to hold the detailed interface data, then a second time to actually 
				        //get the structure (after we have allocated enough memory for the structure.)
                        DetailedInterfaceDataStructure.cbSize = (uint)Marshal.SizeOf(DetailedInterfaceDataStructure);
				        //First call populates "StructureSize" with the correct value
				        SetupDiGetDeviceInterfaceDetail(DeviceInfoTable, ref InterfaceDataStructure, IntPtr.Zero, 0, ref StructureSize, IntPtr.Zero);
                        //Need to call SetupDiGetDeviceInterfaceDetail() again, this time specifying a pointer to a SP_DEVICE_INTERFACE_DETAIL_DATA buffer with the correct size of RAM allocated.
                        //First need to allocate the unmanaged buffer and get a pointer to it.
                        IntPtr pUnmanagedDetailedInterfaceDataStructure = IntPtr.Zero;  //Declare a pointer.
                        pUnmanagedDetailedInterfaceDataStructure = Marshal.AllocHGlobal((int)StructureSize);    //Reserve some unmanaged memory for the structure.
                        DetailedInterfaceDataStructure.cbSize = 6; //Initialize the cbSize parameter (4 bytes for DWORD + 2 bytes for unicode null terminator)
                        Marshal.StructureToPtr(DetailedInterfaceDataStructure, pUnmanagedDetailedInterfaceDataStructure, false); //Copy managed structure contents into the unmanaged memory buffer.

                        //Now call SetupDiGetDeviceInterfaceDetail() a second time to receive the device path in the structure at pUnmanagedDetailedInterfaceDataStructure.
                        if (SetupDiGetDeviceInterfaceDetail(DeviceInfoTable, ref InterfaceDataStructure, pUnmanagedDetailedInterfaceDataStructure, StructureSize, IntPtr.Zero, IntPtr.Zero))
                        {
                            //Need to extract the path information from the unmanaged "structure".  The path starts at (pUnmanagedDetailedInterfaceDataStructure + sizeof(DWORD)).
                            IntPtr pToDevicePath = new IntPtr((uint)pUnmanagedDetailedInterfaceDataStructure.ToInt32() + 4);  //Add 4 to the pointer (to get the pointer to point to the path, instead of the DWORD cbSize parameter)
                            DevicePath = Marshal.PtrToStringUni(pToDevicePath); //Now copy the path information into the globally defined DevicePath String.
                                
                            //We now have the proper device path, and we can finally use the path to open I/O handle(s) to the device.
                            SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
                            Marshal.FreeHGlobal(pUnmanagedDetailedInterfaceDataStructure);  //No longer need this unmanaged SP_DEVICE_INTERFACE_DETAIL_DATA buffer.  We already extracted the path information.
                            return true;    //Returning the device path in the global DevicePath String
                        }
                        else //Some unknown failure occurred
                        {
                            uint ErrorCode = (uint)Marshal.GetLastWin32Error();
                            SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure.
                            Marshal.FreeHGlobal(pUnmanagedDetailedInterfaceDataStructure);  //No longer need this unmanaged SP_DEVICE_INTERFACE_DETAIL_DATA buffer.  We already extracted the path information.
                            return false;    
                        }
                    }

			        InterfaceIndex++;	
			        //Keep looping until we either find a device with matching VID and PID, or until we run out of devices to check.
			        //However, just in case some unexpected error occurs, keep track of the number of loops executed.
			        //If the number of loops exceeds a very large number, exit anyway, to prevent inadvertent infinite looping.
			        LoopCounter++;
			        if(LoopCounter == 10000000)	//Surely there aren't more than 10 million devices attached to any forseeable PC...
			        {
				        return false;
			        }
		        }//end of while(true)
            }
            return false;
        }//end of try
        catch
        {
            //Something went wrong if PC gets here.  Maybe a Marshal.AllocHGlobal() failed due to insufficient resources or something.
			return false;	
        }
    }



    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------

    //--------------------------------------------------------------------------------------------------------------------------
    //FUNCTION:	ReadFileManagedBuffer()
    //PURPOSE:	Wrapper function to call ReadFile()
    //
    //INPUT:	Uses managed versions of the same input parameters as ReadFile() uses.
    //
    //OUTPUT:	Returns boolean indicating if the function call was successful or not.
    //          Also returns data in the byte[] INBuffer, and the number of bytes read. 
    //
    //Notes:    Wrapper function used to call the ReadFile() function.  ReadFile() takes a pointer to an unmanaged buffer and deposits
    //          the bytes read into the buffer.  However, can't pass a pointer to a managed buffer directly to ReadFile().
    //          This ReadFileManagedBuffer() is a wrapper function to make it so application code can call ReadFile() easier
    //          by specifying a managed buffer.
    //--------------------------------------------------------------------------------------------------------------------------
    public bool ReadFileManagedBuffer(SafeFileHandle hFile, byte[] INBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, IntPtr lpOverlapped)
    {
        IntPtr pINBuffer = IntPtr.Zero;

        try
        {
            pINBuffer = Marshal.AllocHGlobal((int)nNumberOfBytesToRead);    //Allocate some unmanged RAM for the receive data buffer.

            if (ReadFile(hFile, pINBuffer, nNumberOfBytesToRead, ref lpNumberOfBytesRead, lpOverlapped))
            {
                Marshal.Copy(pINBuffer, INBuffer, 0, (int)lpNumberOfBytesRead);    //Copy over the data from unmanged memory into the managed byte[] INBuffer
                Marshal.FreeHGlobal(pINBuffer);
                return true;
            }
            else
            {
                Marshal.FreeHGlobal(pINBuffer);
                return false;
            }

        }
        catch
        {
            if (pINBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pINBuffer);
            }
            return false;
        }
    }
    //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------


} //public partial class Form1 : Form
