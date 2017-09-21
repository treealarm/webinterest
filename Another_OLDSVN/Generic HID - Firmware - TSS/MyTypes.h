
#ifndef MYTYPES_FSUSB_H
#define MYTYPES_FSUSB_H

#ifndef _WIN32_WINDOWS
#include "GenericTypeDefs.h"
#else
#pragma once
#endif

#define MOTORS_COUNT 4
#define TIMER_RTE    0
#define TIMER_READER    1
#define TIMER_OPEN_CLOSE    2
#define MULTIPL_FOR_SECOND  10
#define RELAY_DEFAULT_OPEN  1
#define MAX_READERS 2

#define PACKETS_TO_SEND    8
#define READ_TIMER_FOR_BIT_READ 10
#define TIMER_FOR_BLINK 10
#define PACKET_MAX_LEN 32

#define READ_CLOSE_DELAY 10

#define COMMAND_RELAY 0x82

struct do_timer_set
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
m_do_timer_set;

typedef struct 
{
	BYTE command_id;
	BYTE cont_id;
	BYTE reader_id;
	BYTE data_size;
	UINT16 command_counter;
}
data_header;

typedef struct 
{
	BYTE command_id;
	BYTE cont_id;
	BYTE reader_id;
}
data_for_cont_header;

typedef struct 
{
#ifndef _WIN32_WINDOWS
DWORD_VAL wiegand26;//wiegand26
#else
DWORD wiegand26;//wiegand26
#endif
}key_data;

typedef struct 
{
	BYTE RTE;
}rte_data;

typedef struct 
{
	BYTE IsOpen;
}open_close_data;

typedef struct 
{
	unsigned char data[PACKET_MAX_LEN];
}my_packet;

typedef struct 
{
	UINT16 duration;
	BYTE mode;//0 - relay,1 default value
}relay_data;
#endif