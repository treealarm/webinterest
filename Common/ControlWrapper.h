#pragma once

//find our plug and play device.

//Modify this value to match the VID and PID in your USB device descriptor.
//Use the formatting: "Vid_xxxx&Pid_xxxx" where xxxx is a 16-bit hexadecimal number.
#define MY_DEVICE_ID  "Vid_04d8&Pid_003F"


typedef struct
{
	int x;
	int y;
	int z;
	int a;
}
xyz_coord;

typedef struct 
{
	INT32  m_uMult[MOTORS_COUNT];
}do_steps_multiplier;

static const int LEN_OF_PACKET = 65;

class ControllerLock
{
	CCriticalSection* psec;
public:
	ControllerLock(CCriticalSection& sec) { (psec=&sec)->Lock(); }
	~ControllerLock() { psec->Unlock(); }
};


class ControlWrapper
{
protected:
	CList<unsigned char*> m_out_list;
	CCriticalSection m_sec_queue;
	CCriticalSection m_sec_read_write;
	HANDLE m_WriteHandle;	//Need to get a write "handle" to our device before we can write to it.
	HANDLE m_ReadHandle;	//Need to get a read "handle" to our device before we can read from it.
	void AddCommand(BYTE* OutputPacketBuffer);
	do_steps_multiplier m_step_mult;
public:
	INT32 m_steps0;
	ControlWrapper(void);
	virtual ~ControlWrapper(void);
	BOOL SetTimer(do_timer_set& timer_set);
	BOOL SetControlSignals(do_control_signals& control_signals);
	BOOL SetStepMultiplier(do_steps_multiplier& step_mult);
	BOOL SetSteps(do_steps& steps);
	BOOL SetPause(BOOL bPause);
	BOOL Connect(void);
	BOOL IsControllerAvailable(void);
	BOOL WriteCommandFromQueueToController(void);
	BOOL IsOpen(void);
	BOOL CloseController(void);
	void ToggleLed();
	void ClearCommandQueue(void);
	int GetCountInQueue()
	{
		return (int)m_out_list.GetCount();
	}
};
