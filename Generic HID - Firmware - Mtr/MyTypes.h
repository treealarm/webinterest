
#ifndef MYTYPES_FSUSB_H
#define MYTYPES_FSUSB_H

#ifndef _WIN32_WINDOWS
#include "GenericTypeDefs.h"
#else
#pragma once
#endif

#define COMMAND_TOGGLE_LED  0x80
#define COMMAND_IS_AVAILABLE  0x81
#define COMMAND_SET_STEPS  0x82
#define COMMAND_SET_TIME  0x83
#define COMMAND_SET_CONTROL_SIGNALS  0x84
#define COMMAND_SET_PAUSE  0x86

#define MOTORS_COUNT 3
typedef struct _do_steps
{
	INT32  m_uSteps[MOTORS_COUNT];
}do_steps;

#define BITS 8
#define BIT_SET(  p, n) (p[(n)/BITS] |=  (0x80>>((n)%BITS)))
#define BIT_CLEAR(p, n) (p[(n)/BITS] &= ~(0x80>>((n)%BITS)))
#define BIT_ISSET(p, n) (p[(n)/BITS] &   (0x80>>((n)%BITS)))

typedef struct 
{
	union
	{
		struct    
		{ 
			BYTE lo;  
			BYTE hi;
		} tmr16;
		UINT16 u16;

	}m_timer_res;
    BYTE m_multiplier[MOTORS_COUNT];
}
do_timer_set;
do_timer_set m_do_timer_set;

struct do_control_signals
{
	BYTE ms1;
	BYTE ms2;
	BYTE reset;
	BYTE enable;
}
m_do_control_signals;

#endif