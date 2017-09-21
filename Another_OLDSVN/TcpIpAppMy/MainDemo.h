/*********************************************************************
 *
 *                  Headers for TCPIP Demo App
 *
 *********************************************************************
 * FileName:        MainDemo.h
 * Dependencies:    Compiler.h
 * Processor:       PIC18, PIC24F, PIC24H, dsPIC30F, dsPIC33F, PIC32
 * Compiler:        Microchip C32 v1.05 or higher
 *					Microchip C30 v3.12 or higher
 *					Microchip C18 v3.30 or higher
 *					HI-TECH PICC-18 PRO 9.63PL2 or higher
 * Company:         Microchip Technology, Inc.
 *
 * Software License Agreement
 *
 * Copyright (C) 2002-2010 Microchip Technology Inc.  All rights
 * reserved.
 *
 * Microchip licenses to you the right to use, modify, copy, and
 * distribute:
 * (i)  the Software when embedded on a Microchip microcontroller or
 *      digital signal controller product ("Device") which is
 *      integrated into Licensee's product; or
 * (ii) ONLY the Software driver source files ENC28J60.c, ENC28J60.h,
 *		ENCX24J600.c and ENCX24J600.h ported to a non-Microchip device
 *		used in conjunction with a Microchip ethernet controller for
 *		the sole purpose of interfacing with the ethernet controller.
 *
 * You should refer to the license agreement accompanying this
 * Software for additional information regarding your rights and
 * obligations.
 *
 * THE SOFTWARE AND DOCUMENTATION ARE PROVIDED "AS IS" WITHOUT
 * WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT
 * LIMITATION, ANY WARRANTY OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT SHALL
 * MICROCHIP BE LIABLE FOR ANY INCIDENTAL, SPECIAL, INDIRECT OR
 * CONSEQUENTIAL DAMAGES, LOST PROFITS OR LOST DATA, COST OF
 * PROCUREMENT OF SUBSTITUTE GOODS, TECHNOLOGY OR SERVICES, ANY CLAIMS
 * BY THIRD PARTIES (INCLUDING BUT NOT LIMITED TO ANY DEFENSE
 * THEREOF), ANY CLAIMS FOR INDEMNITY OR CONTRIBUTION, OR OTHER
 * SIMILAR COSTS, WHETHER ASSERTED ON THE BASIS OF CONTRACT, TORT
 * (INCLUDING NEGLIGENCE), BREACH OF WARRANTY, OR OTHERWISE.
 *
 *
 * Author               Date    Comment
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 * E. Wood				4/26/08 Copied from MainDemo.c
 ********************************************************************/
#ifndef _MAINDEMO_H
#define _MAINDEMO_H

#define BAUD_RATE       (19200)		// bps

#define FLASH_WRITE_SIZE	(64ul)
#define FLASH_ERASE_SIZE	(1024ul)

#define ANALOG_INPUTS 3
#define ANALOG_LEN 16
#define SETTING_LEN2 32

#include "GenericTypeDefs.h"

#if !defined(THIS_IS_STACK_APPLICATION)
extern BYTE AN0String[ANALOG_INPUTS][ANALOG_LEN];
#endif


#define READER_STARTED      0x0000
#define READER_READY        0x0001
#define READER_KEY        	0x0002
#define READER_OPEN       	0x0004
#define READER_CLOSE        0x0008
#define READER_RTE        	0x0010
#define READER_RELAY   		0x0020
//#define WS_CLIPCHILDREN    0x0040

#define TEMP_BUFFER_BIG_SIZE 1024

typedef enum
{
	E_NAME,
	E_PASS,
	E_HOST_CL,
	E_PORT_CL,

	E_LAST
}
E_EXTRA_SETTINGS;


typedef struct
{
	BYTE data[E_LAST+1][SETTING_LEN2];
	E_EXTRA_SETTINGS dest;
} EXTRA_SETTINGS;

typedef struct 
{
	DWORD_VAL wiegand26;//wiegand26//wiegand26
	BYTE bit_counter;
	DWORD relay_open_counter;//при получении команды "relay" выставляем в текущее время
	DWORD rte_open_counter;//при нажатии кнопки выставляем в текущее время
	DWORD key_read_counter;//при считывании карты выставляем в текущее время
	unsigned sensor:1;
	WORD reader_state;
}skd_data;

typedef struct 
{
	DWORD_VAL wiegand26;//wiegand26//wiegand26
	DWORD rte_push_timestamp;//при нажатии кнопки выставляем в текущее время
	DWORD rte_push_timestamp_sent;//ранее отосланный таймстемп нажатия rte

	DWORD key_new_timestamp;//при считывании выставляем в текущее время
	DWORD key_new_timestamp_sent;//ранее отосланный таймстемп момента считывания

	DWORD sensor_open_timestamp;
	DWORD sensor_open_timestamp_sent;

	DWORD sensor_close_timestamp;
	DWORD sensor_close_timestamp_sent;
}skd_data_to_send;

typedef struct 
{
	BYTE relay_open_time;//сколько секунд держать открытым после получения команды "RELAY"
	BYTE rte_open_time;//сколько секунд держать открытым после нажатия RTE
	BYTE key_read_time;//сколько секунд дается на обработку карточки
}skd_settings;


extern skd_data m_skd_data;
extern skd_settings m_skd_settings;
extern skd_data_to_send m_skd_data_to_send;
extern INT8 m_autonom;
extern EXTRA_SETTINGS AutonomSettings;

extern DWORD g_LedBlinkCount;
extern DWORD dwInternalTicks1;
extern WORD vTick1Reading[4];
extern DWORD_VAL edenica;

extern BYTE m_button_change[4];
extern BYTE m_cur_button_states[4];
void DoUARTConfig(void);

void SaveAppConfig(BYTE* pAppConfig);


void SMTPDemo(void);
void PingDemo(void);
void SNMPTrapDemo(void);
void SNMPV2TrapDemo(void);
void GenericTCPClient(void);
void GenericTCPServer(void);
void BerkeleyTCPClientDemo(void);
void BerkeleyTCPServerDemo(void);
void BerkeleyUDPClientDemo(void);

BOOL LoadAppConfig(void);
// An actual function defined in MainDemo.c for displaying the current IP 
// address on the UART and/or LCD.

void DisplayIPValue(IP_ADDR IPVal);


void TickInit1(void);
void Tick1Update(void);
void ProcessReader(void);
void ProcessReadData(int now_bit);
void ProcessSensor(BYTE bOpen);
void ProcessRTE(BYTE bButtonPushed);
void ProcessRELAY(BYTE bOpen);

void ProcessIO(void);

void ProcessTimeOuts(void);
void SetLed(int channel,int value);
void ftoa(float Value, char* Buffer, int buf_len);

void InitReaderCycle(void);
BYTE* GetAsWiegand(DWORD card);
BYTE* GetAsTouchMem(void);

BYTE* GetAsWiegandFacilityCode(void);
BYTE* GetAsWiegandCardCode(void);
DWORD Wiegand2TouchMemoryDword(char* card_number, char* facility_code);

DWORD timeGetTime(void);

void SaveCard(int number,DWORD card);

BOOL RawWriteFlashBlock(DWORD Address, BYTE *BlockData, BYTE *StatusData);
void RawEraseFlashBlock(DWORD dwAddress);

extern ROM BYTE RomConfBuff[FLASH_ERASE_SIZE];
extern ROM BYTE RomCardBuff[FLASH_ERASE_SIZE];
extern BYTE*  temp_buffer_big;

#endif // _MAINDEMO_H
