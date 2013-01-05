#include "StdAfx.h"
#include "ControlWrapper.h"

#pragma message("Macro linking:\"Setupapi.lib\"...")
#pragma comment (lib,"Setupapi.lib")


ControlWrapper::ControlWrapper(void)
{
	memset(&m_step_mult,1,sizeof(m_step_mult));
	m_WriteHandle = INVALID_HANDLE_VALUE;	//Need to get a write "handle" to our device before we can write to it.
	m_ReadHandle = INVALID_HANDLE_VALUE;	//Need to get a read "handle" to our device before we can read from it.

	unsigned char OutputPacketBuffer[LEN_OF_PACKET];
	ZeroMemory(OutputPacketBuffer, sizeof(OutputPacketBuffer));

	BIT_SET(OutputPacketBuffer,16);
	for(int i = 0; i < LEN_OF_PACKET*8; i++)
	{
		int test = BIT_ISSET(OutputPacketBuffer, i) != 0;
		TRACE2("%d)%d\n",i, test );
	}
}

ControlWrapper::~ControlWrapper(void)
{
	ClearCommandQueue();
	CloseController();
}

void ControlWrapper::ClearCommandQueue(void)
{
	m_sec_queue.Lock();
	while (!m_out_list.IsEmpty())
	{
		unsigned char* OutputPacketBuffer = m_out_list.RemoveHead();
		delete[] OutputPacketBuffer;
	}
	m_out_list.RemoveAll();
	m_sec_queue.Unlock();
}

BOOL ControlWrapper::SetTimer(do_timer_set& timer_set)
{
	int size1 = sizeof(timer_set);
	unsigned char* OutputPacketBuffer = new unsigned char[LEN_OF_PACKET];	
	//Allocate a memory buffer equal to our endpoint size + 1
	OutputPacketBuffer[0] = 0;				
	//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.
	OutputPacketBuffer[1] = COMMAND_SET_TIME;
	CopyMemory(&OutputPacketBuffer[2],&timer_set,sizeof(timer_set));
	AddCommand(OutputPacketBuffer);
	return TRUE;
}

BOOL ControlWrapper::SetControlSignals(do_control_signals& control_signals)
{
	unsigned char* OutputPacketBuffer = new unsigned char[LEN_OF_PACKET];	//Allocate a memory buffer equal to our endpoint size + 1
	OutputPacketBuffer[0] = 0;				//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.
	OutputPacketBuffer[1] = COMMAND_SET_CONTROL_SIGNALS;			
	CopyMemory(&OutputPacketBuffer[2],&control_signals,sizeof(control_signals));
	AddCommand(OutputPacketBuffer);
	return TRUE;
}

BOOL ControlWrapper::SetPause(BOOL bPause)
{
	unsigned char* OutputPacketBuffer = new unsigned char[LEN_OF_PACKET];	//Allocate a memory buffer equal to our endpoint size + 1
	OutputPacketBuffer[0] = 0;				//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.
	OutputPacketBuffer[1] = COMMAND_SET_PAUSE;			
	OutputPacketBuffer[2] = (BYTE)bPause;
	AddCommand(OutputPacketBuffer);
	return TRUE;
}
BOOL ControlWrapper::SetStepMultiplier(do_steps_multiplier& step_mult)
{
	CopyMemory(&m_step_mult,&step_mult,sizeof(m_step_mult));
	return TRUE;
}

BOOL ControlWrapper::SetSteps(do_steps& steps)
{
	for(BYTE motor = 0;motor < MOTORS_COUNT;motor++)
	{
		steps.m_uSteps[motor] = steps.m_uSteps[motor]*m_step_mult.m_uMult[motor];
	}
	unsigned char* OutputPacketBuffer = new unsigned char[LEN_OF_PACKET];	//Allocate a memory buffer equal to our endpoint size + 1
	OutputPacketBuffer[0] = 0;				//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.
	OutputPacketBuffer[1] = COMMAND_SET_STEPS;			
	CopyMemory(&OutputPacketBuffer[2],&steps,sizeof(steps));
	AddCommand(OutputPacketBuffer);
	return TRUE;
}

void ControlWrapper::AddCommand(BYTE* OutputPacketBuffer)
{
	m_sec_queue.Lock();
	m_out_list.AddTail(OutputPacketBuffer);
	m_sec_queue.Unlock();
}

BOOL ControlWrapper::Connect(void)
{
	ControllerLock sec(m_sec_read_write);
	/* 
	Before we can "connect" our application to our USB embedded device, we must first find the device.
	A USB bus can have many devices simultaneously connected, so somehow we have to find our device, and only
	our device.  This is done with the Vendor ID (VID) and Product ID (PID).  Each USB product line should have
	a unique combination of VID and PID.  

	Microsoft has created a number of functions which are useful for finding plug and play devices.  Documentation
	for each function used can be found in the MSDN library.  We will be using the following functions:

	SetupDiGetClassDevs()					//provided by setupapi.dll, which comes with Windows
	SetupDiEnumDeviceInterfaces()			//provided by setupapi.dll, which comes with Windows
	GetLastError()							//provided by kernel32.dll, which comes with Windows
	SetupDiDestroyDeviceInfoList()			//provided by setupapi.dll, which comes with Windows
	SetupDiGetDeviceInterfaceDetail()		//provided by setupapi.dll, which comes with Windows
	SetupDiGetDeviceRegistryProperty()		//provided by setupapi.dll, which comes with Windows
	malloc()								//part of C runtime library, msvcrt.dll?
	CreateFile()							//provided by kernel32.dll, which comes with Windows

	We will also be using the following unusual data types and structures.  Documentation can also be found in
	the MSDN library:

	PSP_DEVICE_INTERFACE_DATA
	PSP_DEVICE_INTERFACE_DETAIL_DATA
	SP_DEVINFO_DATA
	HDEVINFO
	HANDLE
	GUID

	The ultimate objective of the following code is to call CreateFile(), which opens a communications
	pipe to a specific device (such as a HID class USB device endpoint).  CreateFile() returns a "handle" 
	which is needed later when calling ReadFile() or WriteFile().  These functions are used to actually 
	send and receive application related data to/from the USB peripheral device.

	However, in order to call CreateFile(), we first need to get the device path for the USB device
	with the correct VID and PID.  Getting the device path is a multi-step round about process, which
	requires calling several of the SetupDixxx() functions provided by setupapi.dll.
	*/


	//Globally Unique Identifier (GUID) for HID class devices.  Windows uses GUIDs to identify things.
	GUID InterfaceClassGuid = {0x4d1e55b2, 0xf16f, 0x11cf, 0x88, 0xcb, 0x00, 0x11, 0x11, 0x00, 0x00, 0x30}; 

	HDEVINFO DeviceInfoTable = INVALID_HANDLE_VALUE;
	SP_DEVICE_INTERFACE_DATA sp_d_i_d;
	PSP_DEVICE_INTERFACE_DATA InterfaceDataStructure0 = &sp_d_i_d;

	SP_DEVINFO_DATA DevInfoData;

	DWORD InterfaceIndex = 0;
	DWORD StatusLastError = 0;
	DWORD dwRegType;
	DWORD dwRegSize;
	DWORD StructureSize = 0;
	PBYTE PropertyValueBuffer;
	bool MatchFound = false;
	DWORD ErrorStatus;

	CString DeviceIDToFind = MY_DEVICE_ID;

	//First populate a list of plugged in devices (by specifying "DIGCF_PRESENT"), which are of the specified class GUID. 
	DeviceInfoTable = SetupDiGetClassDevs(&InterfaceClassGuid, NULL, NULL, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

	//Now look through the list we just populated.  We are trying to see if any of them match our device. 
	while(true)
	{
		InterfaceDataStructure0->cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
		SetupDiEnumDeviceInterfaces(DeviceInfoTable, NULL, &InterfaceClassGuid, InterfaceIndex, InterfaceDataStructure0);
		ErrorStatus = GetLastError();
		if(ERROR_NO_MORE_ITEMS == GetLastError())	//Did we reach the end of the list of matching devices in the DeviceInfoTable?
		{	//Cound not find the device.  Must not have been attached.
			SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
			return FALSE;		
		}

		//Now retrieve the hardware ID from the registry.  The hardware ID contains the VID and PID, which we will then 
		//check to see if it is the correct device or not.

		//Initialize an appropriate SP_DEVINFO_DATA structure.  We need this structure for SetupDiGetDeviceRegistryProperty().
		DevInfoData.cbSize = sizeof(SP_DEVINFO_DATA);
		SetupDiEnumDeviceInfo(DeviceInfoTable, InterfaceIndex, &DevInfoData);

		//First query for the size of the hardware ID, so we can know how big a buffer to allocate for the data.
		SetupDiGetDeviceRegistryProperty(DeviceInfoTable, &DevInfoData, SPDRP_HARDWAREID, &dwRegType, NULL, 0, &dwRegSize);

		//Allocate a buffer for the hardware ID.
		PropertyValueBuffer = (BYTE *) new BYTE[dwRegSize];
		if(PropertyValueBuffer == NULL)	//if null, error, couldn't allocate enough memory
		{	//Can't really recover from this situation, just exit instead.
			SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
			return FALSE;		
		}

		//Retrieve the hardware IDs for the current device we are looking at.  PropertyValueBuffer gets filled with a 
		//REG_MULTI_SZ (array of null terminated strings).  To find a device, we only care about the very first string in the
		//buffer, which will be the "device ID".  The device ID is a string which contains the VID and PID, in the example 
		//format "Vid_04d8&Pid_003f".
		SetupDiGetDeviceRegistryProperty(DeviceInfoTable, &DevInfoData, SPDRP_HARDWAREID, &dwRegType, PropertyValueBuffer, dwRegSize, NULL);

		//Now check if the first string in the hardware ID matches the device ID of my USB device.
		CString DeviceIDFromRegistry = CString((char *)PropertyValueBuffer);

		delete [] PropertyValueBuffer;

		//Now check if the hardware ID we are looking at contains the correct VID/PID
		//DeviceIDToFind = "ITV FS5/6";
		DeviceIDFromRegistry.MakeLower();
		DeviceIDToFind.MakeLower();
		MatchFound = DeviceIDFromRegistry.Find(DeviceIDToFind)>=0;		
		if(MatchFound == true)
		{
			//Device must have been found.  Open read and write handles.  In order to do this, we will need the actual device path first.
			//We can get the path by calling SetupDiGetDeviceInterfaceDetail(), however, we have to call this function twice:  The first
			//time to get the size of the required structure/buffer to hold the detailed interface data, then a second time to actually 
			//get the structure (after we have allocated enough memory for the structure.)

			//First call populates "StructureSize" with the correct value
			SetupDiGetDeviceInterfaceDetail(DeviceInfoTable, InterfaceDataStructure0, NULL, NULL, &StructureSize, NULL);	

			PSP_DEVICE_INTERFACE_DETAIL_DATA DetailedInterfaceDataStructure = (PSP_DEVICE_INTERFACE_DETAIL_DATA)(new BYTE[StructureSize]);		//Allocate enough memory
			if(DetailedInterfaceDataStructure == NULL)	//if null, error, couldn't allocate enough memory
			{	//Can't really recover from this situation, just exit instead.
				SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
				return FALSE;		
			}
			DetailedInterfaceDataStructure->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
			//Now call SetupDiGetDeviceInterfaceDetail() a second time to receive the goods.  
			SetupDiGetDeviceInterfaceDetail(DeviceInfoTable, InterfaceDataStructure0, DetailedInterfaceDataStructure, StructureSize, NULL, NULL); 

			//We now have the proper device path, and we can finally open read and write handles to the device.
			//We store the handles in the global variables "WriteHandle" and "ReadHandle", which we will use later to actually communicate.
			m_WriteHandle = CreateFile((DetailedInterfaceDataStructure->DevicePath), GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, 0);

			ErrorStatus = GetLastError();
			if(ErrorStatus == ERROR_SUCCESS)
			{
			
			}
			m_ReadHandle = CreateFile((DetailedInterfaceDataStructure->DevicePath), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, 0);
			ErrorStatus = GetLastError();
			BOOL retval = FALSE;
			if(ErrorStatus == ERROR_SUCCESS)
			{
				retval = TRUE;
			}
			SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
			delete DetailedInterfaceDataStructure;
			return retval;
		}

		InterfaceIndex++;	
		//Keep looping until we either find a device with matching VID and PID, or until we run out of items.
	}//end of while(true)	
	return FALSE;
}

BOOL ControlWrapper::IsControllerAvailable(void)
{
	ControllerLock sec(m_sec_read_write);

	DWORD BytesWritten = 0;
	DWORD BytesRead = 0;
	unsigned char OutputPacketBuffer[LEN_OF_PACKET];	//Allocate a memory buffer equal to our endpoint size + 1
	unsigned char InputPacketBuffer[LEN_OF_PACKET];	//Allocate a memory buffer equal to our endpoint size + 1

	InputPacketBuffer[0] = 0;				//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.
	OutputPacketBuffer[0] = 0;				//The first byte is the "Report ID" and does not get transmitted over the USB bus.  Always set = 0.

	OutputPacketBuffer[1] = COMMAND_IS_AVAILABLE;			
	//For simplicity, we will leave the rest of the buffer uninitialized, but you could put real
	//data in it if you like.

	//The basic Windows I/O functions WriteFile() and ReadFile() can be used to read and write to HID class USB devices 
	//(once we have the read and write handles to the device, which are obtained with CreateFile()).

	//To get the pushbutton state, first, we send a packet with our "Get Pushbutton State" command in it.
	//The following call to WriteFile() sends 64 bytes of data to the USB device.
	BOOL bRet = WriteFile(m_WriteHandle, &OutputPacketBuffer, LEN_OF_PACKET, &BytesWritten, 0);	//Blocking function, unless an "overlapped" structure is used
	if(!bRet)
		return FALSE;
	//Now get the response packet from the firmware.
	//The following call to ReadFIle() retrieves 64 bytes of data from the USB device.
	ZeroMemory(InputPacketBuffer,LEN_OF_PACKET);
	bRet = ReadFile(m_ReadHandle, &InputPacketBuffer, LEN_OF_PACKET, &BytesRead, 0);		//Blocking function, unless an "overlapped" structure is used	
	if(!bRet)
		return FALSE;

	//InputPacketBuffer[0] is the report ID, which we don't care about.
	//InputPacketBuffer[1] is an echo back of the command.
	//InputPacketBuffer[2] contains the I/O port pin value for the pushbutton.  

	CopyMemory(&m_cur_steps,&InputPacketBuffer[3],sizeof(m_cur_steps));
	CopyMemory(&m_timer_ink_impuls,&InputPacketBuffer[3+sizeof(m_cur_steps)],sizeof(m_timer_ink_impuls));
	
	//if(m_steps0%32 == 0)
	/*{
	CString s;
	s.Format("step=%d,%d,%d,%d  home=%d,%d,%d,%d\n",
		InputPacketBuffer[4],InputPacketBuffer[5],InputPacketBuffer[6],InputPacketBuffer[7],
		InputPacketBuffer[8],InputPacketBuffer[9],InputPacketBuffer[10],InputPacketBuffer[11]);
	TRACE0(s);
	}*/
	if(InputPacketBuffer[2] == 0x01)	
	{
		return TRUE;
	}
	
	return FALSE;
}

BOOL ControlWrapper::WriteCommandFromQueueToController(void)
{
	unsigned char* OutputPacketBuffer = NULL;
	m_sec_queue.Lock();
	if(m_out_list.IsEmpty())
	{
		m_sec_queue.Unlock();
		return FALSE;
	}

	if(!IsOpen() || !IsControllerAvailable())
	{
		m_sec_queue.Unlock();
		return FALSE;
	}
	OutputPacketBuffer = m_out_list.RemoveHead();
	m_sec_queue.Unlock();

	ControllerLock sec(m_sec_read_write);
	DWORD BytesWritten = 0;
	BOOL retw = WriteFile(m_WriteHandle, OutputPacketBuffer, LEN_OF_PACKET, &BytesWritten, 0);	//Blocking function, unless an "overlapped" structure is used
	for(int i=0;i<LEN_OF_PACKET;i++)
	{
		CString s;
		s.Format("0x%x,",OutputPacketBuffer[i]);
		//TRACE0(s);
	}
	TRACE0("\n");
	delete[] OutputPacketBuffer;

	return retw;
}

BOOL ControlWrapper::IsOpen(void)
{
	return m_WriteHandle != INVALID_HANDLE_VALUE;
}

BOOL ControlWrapper::CloseController(void)
{
	ControllerLock sec(m_sec_read_write);
	if(m_WriteHandle!=INVALID_HANDLE_VALUE)
	{
		CloseHandle(m_WriteHandle);
	}
	if(m_ReadHandle!=INVALID_HANDLE_VALUE)
	{
		CloseHandle(m_ReadHandle);
	}
	m_WriteHandle = INVALID_HANDLE_VALUE;	//Need to get a write "handle" to our device before we can write to it.
	m_ReadHandle = INVALID_HANDLE_VALUE;
	return TRUE;
}

void ControlWrapper::ToggleLed()
{
	DWORD BytesWritten = 0;
	unsigned char* OutputPacketBuffer = new unsigned char[65];	//Allocate a memory buffer equal to our endpoint size + 1

	OutputPacketBuffer[0] = 0;			//The first byte is the "Report ID".  This number is used by the USB driver, but does not
	//get tranmitted accross the USB bus.  The custom HID class firmware is only configured for
	//one type of report, therefore, we must always initializate this byte to "0" before sending
	//a data packet to the device.

	OutputPacketBuffer[1] = COMMAND_TOGGLE_LED;		//0x80 is the "Toggle LED(s)" command in the firmware

	AddCommand(OutputPacketBuffer);
}