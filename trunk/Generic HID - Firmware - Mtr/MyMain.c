#include "MyMain.h"
#include "MyTypes.h"

#include "GenericTypeDefs.h"
#include "Compiler.h"

#include "./USB/usb.h"
#include "HardwareProfile.h"
#include "./USB/usb_function_hid.h"
#include <delays.h >

extern unsigned char ReceivedDataBuffer[64];
extern unsigned char ToSendDataBuffer[64];
extern USB_HANDLE USBInHandle;

do_steps m_do_cur_steps;
do_steps m_do_cur_steps_buf;

BYTE m_timer_cnt[MOTORS_COUNT] = {0,0,0,0};
UINT16 m_timer_ink_impuls = 0;
BYTE m_b_Pause = FALSE;
BYTE m_b_InkOn = FALSE;
//sr should be 0, it's active mode, so shottky not required


#define ink_impuls      PORTDbits.RD2
#define led_3           PORTBbits.RB2
////////////////////////////////////////////
#define step_0          PORTAbits.RA5
#define dir_0           PORTAbits.RA4

#define step_1          PORTAbits.RA0
#define dir_1           PORTCbits.RC0

#define step_2          PORTCbits.RC2
#define dir_2           PORTCbits.RC1

#define step_3          PORTAbits.RA1
#define dir_3           PORTAbits.RA2

#define enable_0        PORTAbits.RA3
/////////////////////////////////////////////
#define led_main        PORTDbits.RD3
#define btn_1			PORTBbits.RB4


void RestartTimer(void)
{

	TMR0H = m_do_timer_set.m_timer_res.tmr16.hi;
	TMR0L = m_do_timer_set.m_timer_res.tmr16.lo;

	INTCONbits.TMR0IF = 0;          // Clear flag
}

void CopyBufferToMotor(void)
{
	if(CheckIfBufferOnZero())
	{
		return;
	}
	memcpy(
		(void*)(&m_do_cur_steps),
		(void*)(&m_do_cur_steps_buf),
		sizeof(m_do_cur_steps_buf) );
	memset(&m_do_cur_steps_buf,0,sizeof(m_do_cur_steps_buf));


	SetupDirs();
}

int CheckIfMotorsOnZero(void)
{
	int i = 0;
	for(i = 0;i < MOTORS_COUNT;i++)
	{
		if(m_do_cur_steps.m_uSteps[i] != 0)
		{
			return 0;
		}
	}
	return 1;
}

int CheckIfBufferOnZero(void)
{
	int i = 0;
	for(i = 0;i < MOTORS_COUNT;i++)
	{
		if(m_do_cur_steps_buf.m_uSteps[i] != 0)
		{
			return 0;
		}
	}
	return 1;
}


void ProcessSteps(void)
{
	int i = 0;
	int motors_on_zero = 0;
	int motors_on_zero_1 = 0;
	int new_impuls = 0;

	if(btn_1 != 0)
	{
		enable_0 = 0; 
		for(i = 0;i < MOTORS_COUNT;i++)
		{
			m_do_timer_set.m_multiplier[i] = 1;
			m_do_cur_steps_buf.m_uSteps[i] = 32*200;
			m_do_cur_steps.m_uSteps[i] = -32*200;
		}
	}

	if(m_b_Pause)
	{
	  led_main = 1;
	  return;
	}


	led_main = !led_main;
	motors_on_zero = CheckIfMotorsOnZero();
	for(i = 0;i < MOTORS_COUNT;i++)
	{
		if(m_timer_cnt[i] >= m_do_timer_set.m_multiplier[i])
		{
			ProcessStep(i);
			m_timer_cnt[i] = 0;
		}
		else
		{
			m_timer_cnt[i] = m_timer_cnt[i]+1;
		}
	
	}

	motors_on_zero_1 = CheckIfMotorsOnZero();
	new_impuls = !motors_on_zero && motors_on_zero_1;

	if(m_do_timer_set.m_ink_impuls > 0)
	{
		ProcessInkImpuls(new_impuls);
	}
	else
	{
		if(motors_on_zero_1)
		{
			ink_impuls = m_b_InkOn;
			led_3 = ink_impuls;//ink_sensor;
		}
	}

	if(motors_on_zero_1)
	{
		CopyBufferToMotor();
	}
}



void ProcessStep(int motor)
{
	if(motor == 0)
	{
		if(step_0 != 0)
		{
	    	step_0 = 0;
			return;
		}
	}

	if(motor == 1)
	{
		if(step_1 != 0)
		{
	    	step_1 = 0;
			return;
		}
	}

	if(motor == 2)
	{
	    if(step_2 != 0)
		{
	    	step_2 = 0;
			return;
		}
	}
	if(motor == 3)
	{
	    if(step_3 != 0)
		{
	    	step_3 = 0;
			return;
		}
	}


	if(m_do_cur_steps.m_uSteps[motor] == 0)
	{
		return;
	}
	if(motor == 0)
	{
	    step_0 = 1;
	}

	if(motor == 1)
	{
	    step_1 = 1;
	}

	if(motor == 2)
	{
	    step_2 = 1;
	}

	if(motor == 3)
	{
	    step_3 = 1;
	}

	if(m_do_cur_steps.m_uSteps[motor] > 0)
	{
    	m_do_cur_steps.m_uSteps[motor]--;
	}
	else
	{
    	m_do_cur_steps.m_uSteps[motor]++;
	}
}

void SetupDirs(void)
{
	dir_0 = (m_do_cur_steps.m_uSteps[0] > 0);
  	dir_1 = (m_do_cur_steps.m_uSteps[1] > 0);
  	dir_2 = (m_do_cur_steps.m_uSteps[2] > 0);
	dir_3 = (m_do_cur_steps.m_uSteps[3] > 0);


	memset((void*)(&m_timer_cnt),0,sizeof(m_timer_cnt));
	Delay10KTCYx(1);
	RestartTimer();
}

void SetupCtrlSignals(void)
{
	enable_0 = m_do_control_signals.enable;
	RestartTimer();
}

void MyProcessIO(void)
{   
 switch(ReceivedDataBuffer[0])
 {
    INT32 i = 0; 
	case COMMAND_TOGGLE_LED:  //Toggle LEDs
		led_main = 1;
	break;
	case COMMAND_IS_AVAILABLE:  //Get push button state (available state)
		ToSendDataBuffer[0] = COMMAND_IS_AVAILABLE;				//Echo back to the host PC the command we are fulfilling in the first byte.  In this case, the Get Pushbutton State command.
	    
		if(!CheckIfBufferOnZero())
		{
			ToSendDataBuffer[1] = 0x00;
		}
		else
		{
			ToSendDataBuffer[1] = 0x01;
		}
		if(m_b_Pause)
		{
			ToSendDataBuffer[1] = 0x01;
	//We are paused, so we can setup anything we need, actually Pause is for settings
		}
		memcpy(
		(void*)(&ToSendDataBuffer[2]),
		(void*)(&m_do_cur_steps),
		sizeof(m_do_cur_steps) );

		memcpy(
		(void*)(&ToSendDataBuffer[2])+sizeof(m_do_cur_steps),
		(void*)(&m_timer_ink_impuls),
		sizeof(m_timer_ink_impuls) );

		if(!HIDTxHandleBusy(USBInHandle))
		{
			USBInHandle = HIDTxPacket(HID_EP,(BYTE*)&ToSendDataBuffer,64);
		}
		break;
	case COMMAND_SET_STEPS:
	{
		memcpy(
       			(void*)(&m_do_cur_steps_buf),
       			(void*)(&ReceivedDataBuffer[1]),
       			sizeof(m_do_cur_steps_buf) );

	}
	break;
	

	case COMMAND_SET_TIME:
		memcpy(
           		(void*)(&m_do_timer_set),
           		(void*)(&ReceivedDataBuffer[1]),
           		sizeof(m_do_timer_set) );
  		memset((void*)(&m_timer_cnt),0,sizeof(m_timer_cnt));
		m_timer_ink_impuls = m_do_timer_set.m_ink_impuls;
	break;
	case COMMAND_SET_CONTROL_SIGNALS:
   		memcpy(
           		(void*)&m_do_control_signals,
           		(void*)&ReceivedDataBuffer[1],
           		sizeof(m_do_control_signals) );
		SetupCtrlSignals();

	break;

	case COMMAND_SET_PAUSE:
    {
		m_b_Pause = ReceivedDataBuffer[1];

    }
	break;
	case COMMAND_SET_INK:
    {
		m_b_InkOn = ReceivedDataBuffer[1];
    }
	break;
 }
  
}//end MyProcessIO

void MyUserInit(void)
{

// Timer0 - 1 second interval setup.
// Fosc/4 = 12MHz
// Use /256 prescalar, this brings counter freq down to 46,875 Hz
// Timer0 should = 65536 - 46875 = 18661 or 0x48E5
	int i = 0 ;

	memset(&m_do_timer_set,0,sizeof(m_do_timer_set));
	m_do_timer_set.m_timer_res.tmr16.lo   =      0xE5;
	m_do_timer_set.m_timer_res.tmr16.hi   =      0x48;

	//m_do_timer_set.m_timer_res.u16 = 65536 - 4;
	

	TMR0H = m_do_timer_set.m_timer_res.tmr16.hi;
	TMR0L = m_do_timer_set.m_timer_res.tmr16.lo;

    memset(&m_do_cur_steps,0,sizeof(m_do_cur_steps));
	memset(&m_do_cur_steps_buf,0,sizeof(m_do_cur_steps_buf));
	memset(&m_do_control_signals,0,sizeof(m_do_control_signals));

	
///////////////////////////////////.TRIS////////////////
/*

*/


	TRISDbits.TRISD2 = 0;
	TRISDbits.TRISD3 = 0;
	
	
	TRISCbits.TRISC0 = 0;
	TRISCbits.TRISC2 = 0;
	TRISCbits.TRISC1 = 0;


	TRISAbits.TRISA5 = 0;
	TRISAbits.TRISA4 = 0;
	TRISAbits.TRISA3 = 0;
	TRISAbits.TRISA2 = 0;
	TRISAbits.TRISA1 = 0;
	TRISAbits.TRISA0 = 0;

	TRISBbits.TRISB2 = 0;

	led_main = 1;
	TRISBbits.TRISB4 = 1;
///////////////////////////////////////////////////

	ink_impuls = 0;

    step_0 = 0;
    step_1 = 0;
    step_2 = 0;
    step_3 = 0;

	enable_0 = 1;//(1 -disable, 0 - enable)

    dir_0 = 0;
    dir_1 = 0;
    dir_2 = 0;
    dir_3 = 0;

	INTCONbits.GIEL = 1;
	INTCON2bits.TMR0IP = 0;
    T0CON = 0b10010111;
// Set Timer0 to trigger interrupt
	
    INTCONbits.TMR0IF = 0;          // Clear flag
	INTCONbits.TMR0IE = 1;

// Set TO to be a 16bits timer

	INTCON2bits.NOT_RBPU = 0;

	//for(i = 0;i < MOTORS_COUNT;i++)
	{
		//m_do_timer_set.m_multiplier[i] = 1;
		//m_do_cur_steps_buf.m_uSteps[i] = 32*500;
		//m_do_cur_steps.m_uSteps[i] = -32*500;
	}
	//CopyBufferToMotor();
	//SetupDirs();
	RestartTimer();

}//end MyUserInit


void ProcessInkImpuls(int new_impuls)
{
	if(new_impuls)
	{
		m_timer_ink_impuls = 0;
	}

	if(m_timer_ink_impuls < m_do_timer_set.m_ink_impuls)
	{
		ink_impuls = 1;
		m_timer_ink_impuls += 1;
	}
	else
	{
		ink_impuls = 0;
	}
	led_3 = ink_impuls;//ink_sensor;
}