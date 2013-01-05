#include "MyMain.h"
#include "MyTypes.h"

#include "GenericTypeDefs.h"
#include "Compiler.h"

#include "./USB/usb.h"
#include "HardwareProfile.h"
#include "./USB/usb_function_hid.h"


extern unsigned char ReceivedDataBuffer[64];
extern unsigned char ToSendDataBuffer[64];
extern USB_HANDLE USBInHandle;

do_steps m_do_cur_steps;

BYTE m_timer_cnt[MOTORS_COUNT] = {0,0,0};
BYTE m_timer_ink_impuls = 0;
BYTE m_b_Pause = FALSE;
//sr should be 0, it's active mode, so shottky not required

#define ms1_0           PORTDbits.RD2
#define ms2_0           PORTCbits.RC6
#define sr_0            PORTCbits.RC7
#define reset_0         PORTDbits.RD6
#define enable_0        PORTDbits.RD4
#define sleep_0         PORTDbits.RD5

//#define step_0          PORTBbits.RB0
//#define dir_0           PORTBbits.RB1
//#define led_0           PORTBbits.RB2
//#define home_0          PORTDbits.RD7

#define ink_impuls      PORTBbits.RB0
#define ink_sensor      PORTBbits.RB1
#define led_3           PORTBbits.RB2

#define step_0          PORTCbits.RC0
#define dir_0           PORTCbits.RC1
#define led_0           PORTCbits.RC2
#define home_0          PORTAbits.RA5

#define step_1          PORTEbits.RE0
#define dir_1           PORTEbits.RE1
#define led_1           PORTEbits.RE2
#define home_1          PORTAbits.RA4

#define step_2          PORTAbits.RA0
#define dir_2           PORTAbits.RA1
#define led_2           PORTAbits.RA2
#define home_2          PORTAbits.RA3

#define btn_1			PORTBbits.RB4

INT32 g_forvard = 1;
do_timer_set start_speed = {0};

void RestartTimer(void)
{

	TMR0H = m_do_timer_set.m_timer_res.tmr16.hi;
	TMR0L = m_do_timer_set.m_timer_res.tmr16.lo;

	INTCONbits.TMR0IF = 0;          // Clear flag
}


void ProcessSteps(void)
{
	int i;
	int motors_on_zero = 0;
	int new_impuls = 0;
	ProcessHome();

	if(m_b_Pause)
	{
	  PORTDbits.RD3 = 1;
	  return;
	}

	PORTDbits.RD3 = !PORTDbits.RD3;

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
	new_impuls = !motors_on_zero && CheckIfMotorsOnZero();
	ProcessInkImpuls(new_impuls);

	ProcessHome();
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
void ProcessStep(int motor)
{
	if(m_do_cur_steps.m_uSteps[motor] == 0)
	{
		if(motor == 0)
		{
		  step_0 = 0;
		}
		else
		if(motor == 1)
		{
			step_1 = 0;
		}
		else
		if(motor == 2)
		{
		    step_2 = 0;
		}
		return;
	}

	if(motor == 0)
	{
		if(step_0 != 0)
		{
	    	step_0 = 0;
			return;
		}
	}
	else
	if(motor == 1)
	{
		if(step_1 != 0)
		{
	    	step_1 = 0;
			return;
		}
	}
	else
	if(motor == 2)
	{
	    if(step_2 != 0)
		{
	    	step_2 = 0;
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
	else
	if(motor == 1)
	{
	    step_1 = 1;
	}
	else
	if(motor == 2)
	{
	    step_2 = 1;
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
	if(m_do_cur_steps.m_uSteps[0] != 0)
	{
  		dir_0 = (m_do_cur_steps.m_uSteps[0] > 0);
	}
	if(m_do_cur_steps.m_uSteps[1] != 0)
	{
  		dir_1 = (m_do_cur_steps.m_uSteps[1] > 0);
	}
	if(m_do_cur_steps.m_uSteps[2] != 0)
	{
  		dir_2 = (m_do_cur_steps.m_uSteps[2] > 0);
	}
}

void SetupCtrlSignals(void)
{
	ms1_0 = m_do_control_signals.ms1;
	ms2_0 = m_do_control_signals.ms2;
	reset_0 = m_do_control_signals.reset;
	enable_0 = m_do_control_signals.enable;
}

void MyProcessIO(void)
{   
 switch(ReceivedDataBuffer[0])
 {
    INT32 i = 0; 
	case COMMAND_TOGGLE_LED:  //Toggle LEDs
		PORTDbits.RD3 = 1;
	break;
	case COMMAND_IS_AVAILABLE:  //Get push button state (available state)
		ToSendDataBuffer[0] = COMMAND_IS_AVAILABLE;				//Echo back to the host PC the command we are fulfilling in the first byte.  In this case, the Get Pushbutton State command.
	    
		if(!CheckIfMotorsOnZero())
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

		if(!HIDTxHandleBusy(USBInHandle))
		{
			USBInHandle = HIDTxPacket(HID_EP,(BYTE*)&ToSendDataBuffer,64);
		}
		break;
	case COMMAND_SET_STEPS:
	{
		memcpy(
       			(void*)(&m_do_cur_steps),
       			(void*)(&ReceivedDataBuffer[1]),
       			sizeof(m_do_cur_steps) );

		SetupDirs();
		memset((void*)(&m_timer_cnt),0,sizeof(m_timer_cnt));
	}
	break;
	

	case COMMAND_SET_TIME:
		memcpy(
           		(void*)(&m_do_timer_set),
           		(void*)(&ReceivedDataBuffer[1]),
           		sizeof(m_do_timer_set) );
  		memset((void*)(&m_timer_cnt),0,sizeof(m_timer_cnt));
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
 }
  
}//end MyProcessIO

void MyUserInit(void)
{
// Timer0 - 1 second interval setup.
// Fosc/4 = 12MHz
// Use /256 prescalar, this brings counter freq down to 46,875 Hz
// Timer0 should = 65536 - 46875 = 18661 or 0x48E5
	int i = 0;

	m_do_timer_set.m_timer_res.tmr16.lo   =      0xE5;
	m_do_timer_set.m_timer_res.tmr16.hi   =      0x48;

//	m_do_timer_set.m_timer_res.u16 = 65536 - 2;

	TMR0H = m_do_timer_set.m_timer_res.tmr16.hi;
	TMR0L = m_do_timer_set.m_timer_res.tmr16.lo;


	for(i = 0;i < MOTORS_COUNT;i++)
	{
		m_do_timer_set.m_multiplier[i] = 1;
	}


    TRISDbits.TRISD3 = 0;
	PORTDbits.RD3 = 1;

	TRISBbits.TRISB4 = 1;

    memset(&m_do_cur_steps,0,sizeof(m_do_cur_steps));
	memset(&m_do_control_signals,0,sizeof(m_do_control_signals));


///////////////////////////////////.TRIS////////////////

/* ink_impuls  */           TRISBbits.TRISB0 = 0;
/* ink_sensor   */           TRISBbits.TRISB1 = 1;


/* #define ms1_0*/              TRISDbits.TRISD2 = 0;
/* #define ms2_0*/              TRISCbits.TRISC6 = 0;
/* #define sr_0  */             TRISCbits.TRISC7 = 0;
/* #define reset_0*/            TRISDbits.TRISD6 = 0;

/* #define enable_0*/           TRISDbits.TRISD4 = 0;
/* #define sleep_0 */           TRISDbits.TRISD5 = 0;

/*#define step_1*/             TRISCbits.TRISC0 = 0;
/*#define dir_1*/              TRISCbits.TRISC1 = 0;
/*#define step_2*/             TRISEbits.TRISE0 = 0;
/*#define dir_2  */            TRISEbits.TRISE1 = 0;
/*#define step_3*/             TRISAbits.TRISA0 = 0;
/*#define dir_3*/              TRISAbits.TRISA1 = 0;

/*#define led_0*/              TRISBbits.TRISB2 = 0;
/*#define home_0*/             TRISDbits.TRISD7 = 1;
/*#define led_1*/              TRISCbits.TRISC2 = 0;
/*#define home_1*/             TRISAbits.TRISA3 = 1;
/*#define led_2*/              TRISEbits.TRISE2 = 0;
/*#define home_2*/             TRISAbits.TRISA4 = 1;
/*#define led_3*/              TRISAbits.TRISA2 = 0;
/*#define home_3*/             TRISAbits.TRISA5 = 1;
///////////////////////////////////////////////////
	reset_0 = 0;

    dir_0 = 0;
    dir_1 = 0;
    dir_2 = 0;

    step_0 = 0;
    step_1 = 0;
    step_2 = 0;


	ms1_0 = 1;
	ms2_0 = 1;
	sr_0 = 0;
	enable_0 = 1;
	sleep_0 = 1;

	reset_0 = 1;

	INTCONbits.GIEL = 1;
	INTCON2bits.TMR0IP = 0;
    T0CON = 0b10010111;
// Set Timer0 to trigger interrupt
	
    INTCONbits.TMR0IF = 0;          // Clear flag
	INTCONbits.TMR0IE = 1;

// Set TO to be a 16bits timer

	INTCON2bits.NOT_RBPU = 0;
}//end MyUserInit

void ProcessHome(void)
{
	led_0 = !home_0;
	led_1 = !home_1;
	led_2 = !home_2;
}
void ProcessInkImpuls(int new_impuls)
{
	if(new_impuls)
	{
		m_timer_ink_impuls = 0;
	}
	if(ink_sensor == 0)
	{
		m_timer_ink_impuls = m_do_timer_set.m_ink_impuls;
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