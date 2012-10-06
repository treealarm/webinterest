
#ifndef MYTYPES_FSUSB_H
#define MYTYPES_FSUSB_H

#ifndef _WIN32_WINDOWS
#include "GenericTypeDefs.h"
#else
#pragma once
#endif

#define MOTORS_COUNT 4
typedef struct _do_steps
{
	INT32  m_uSteps[MOTORS_COUNT];
}do_steps;


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