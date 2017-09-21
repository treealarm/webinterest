/*********************************************************************
 *
 *	Generic TCP Server Example Application
 *  Module for Microchip TCP/IP Stack
 *   -Implements an example "ToUpper" TCP server on port 9760 and 
 *	  should be used as a basis for creating new TCP server 
 *    applications
 *	 -Reference: None.  Hopefully AN833 in the future.
 *
 *********************************************************************
 * FileName:        GenericTCPServer.c
 * Dependencies:    TCP
 * Processor:       PIC18, PIC24F, PIC24H, dsPIC30F, dsPIC33F, PIC32
 * Compiler:        Microchip C32 v1.05 or higher
 *					Microchip C30 v3.12 or higher
 *					Microchip C18 v3.30 or higher
 *					HI-TECH PICC-18 PRO 9.63PL2 or higher
 * Company:         Microchip Technology, Inc.
 *
 * Software License Agreement
 *
 * Copyright (C) 2002-2009 Microchip Technology Inc.  All rights
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
 * Author               Date    	Comment
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 * Howard Schlunder     10/19/06	Original
 ********************************************************************/
#define __GENERICTCPSERVER_C

#include "TCPIPConfig.h"
#if defined(STACK_USE_GENERIC_TCP_SERVER_EXAMPLE)

#include "TCPIP Stack/TCPIP.h"


// Defines which port the server will listen on
#define SERVER_PORT	9760


#include "MainDemo.h"
#include <string.h>
/*****************************************************************************
  Function:
	void GenericTCPServer(void)

  Summary:
	Implements a simple ToUpper TCP Server.

  Description:
	This function implements a simple TCP server.  The function is invoked
	periodically by the stack to listen for incoming connections.  When a 
	connection is made, the server reads all incoming data, transforms it
	to uppercase, and echos it back.
	
	This example can be used as a model for many TCP server applications.

  Precondition:
	TCP is initialized.

  Parameters:
	None

  Returns:
  	None
  ***************************************************************************/

typedef enum
{
	E_ANALOG,
	E_DIGITAL_IN,
	E_DIGITAL_OUT,
	E_CUR_DATA_LAST
}
E_CUR_DATA;

int g_cur_data = E_CUR_DATA_LAST;
DWORD g_PrevLedBlinkCount = 0;
BYTE g_val[4][16];

void OnVarChanged(void)
{
	int val = 0;
	int channel = 0;
	if(strcmppgm2ram(g_val[0],"relay")==0)
	{
		channel = atoi((g_val[1]));
		val = atoi((g_val[2]));//duration
		m_skd_settings.relay_open_time = val;
		ProcessRELAY(1);
	}
	if(strcmppgm2ram(g_val[0],"do")==0)
	{
		channel = atoi((g_val[1]));
		val = atoi((g_val[2]));
		SetLed(channel,val);
	}

}
void GenericTCPServer(void)
{
	int i = 0;	
	int j = 0;
	WORD w, w2;
    WORD pstart = 0;
    WORD pend = 0;
	BYTE AppBuffer[64];
	WORD wMaxGet, wMaxPut, wCurrentChunk;
	static TCP_SOCKET	MySocket;
	static enum _TCPServerState
	{
		SM_HOME = 0,
		SM_LISTENING,
	} TCPServerState = SM_HOME;

	switch(TCPServerState)
	{
		case SM_HOME:
			// Allocate a socket for this server to listen and accept connections on
			MySocket = TCPOpen(0, TCP_OPEN_SERVER, SERVER_PORT, TCP_PURPOSE_GENERIC_TCP_SERVER);
			if(MySocket == INVALID_SOCKET)
				return;

			TCPServerState = SM_LISTENING;
			break;

		case SM_LISTENING:
			// See if anyone is connected to us
			if(!TCPIsConnected(MySocket))
			{
				m_autonom = 1;
				return;
			}

			m_autonom = 0;
			// Figure out how many bytes have been received and how many we can transmit.
			wMaxGet = TCPIsGetReady(MySocket);	// Get TCP RX FIFO byte count
			wMaxPut = TCPIsPutReady(MySocket);	// Get TCP TX FIFO free space


			// Make sure we don't take more bytes out of the RX FIFO than we can put into the TX FIFO
			//if(wMaxPut < wMaxGet)
			//	wMaxGet = wMaxPut;
			pstart = TCPFindROMArrayEx(
			    MySocket, 
			    (ROM BYTE*)"<", 
			    1, 
			    0, 
			    wMaxGet, 
			    FALSE);
		
			pend = TCPFindROMArrayEx(
			    MySocket, 
			    (ROM BYTE*)">", 
			    1, 
			    0, 
			    wMaxGet, 
			    FALSE);

			if(pstart == 0xFFFF && wMaxGet > 0)
			{
				TCPGetArray(MySocket, AppBuffer,64);
			}
		
			
	
			if(pstart > 0 && pstart != 0xFFFF)
			{
				TCPGetArray(MySocket, AppBuffer,pstart);
			}
			if(pstart != 0xFFFF && pend != 0xFFFF)
			{
				if(pstart >=0 && pend > pstart)
				{
					w2 = pend-pstart+1;
					TCPGetArray(MySocket, AppBuffer,w2);
					i = 0;
					j = 0;
					memset(&g_val[0][0],0,sizeof(g_val));
					for(w=1;w < (w2-1);w++)
					{
						if(AppBuffer[w]==';')
						{
							i++;
							j = 0;
							continue;
						}

						g_val[i][j] = AppBuffer[w];
						j++;
					}
					OnVarChanged();
				}
			}
			

			if(wMaxPut < 64)
			{
				return;
			}

			if(m_skd_data_to_send.key_new_timestamp != m_skd_data_to_send.key_new_timestamp_sent)
			{
				TCPPutROMString(MySocket, (ROM BYTE*)"%CMD:KEY;0:");

				memset(AppBuffer,0, sizeof(AppBuffer));
				ultoa(m_skd_data_to_send.key_new_timestamp, AppBuffer);
				TCPPutArray(MySocket, AppBuffer, strlen(AppBuffer));
				
				TCPPutROMString(MySocket, (ROM BYTE*)";");
				TCPPutString(MySocket, GetAsWiegand(-1));

				TCPPutROMString(MySocket, (ROM BYTE*)";");
				TCPPutString(MySocket, GetAsTouchMem());

				TCPPutROMString(MySocket, (ROM BYTE*)";%");

			 	m_skd_data_to_send.key_new_timestamp_sent = m_skd_data_to_send.key_new_timestamp;
				return;
			}
		
			if(m_skd_data_to_send.sensor_open_timestamp != m_skd_data_to_send.sensor_open_timestamp_sent)
			{
				TCPPutROMString(MySocket, (ROM BYTE*)"%CMD:OPEN;0:");

				memset(AppBuffer,0, sizeof(AppBuffer));
				ultoa(m_skd_data_to_send.sensor_open_timestamp, AppBuffer);
				TCPPutArray(MySocket, AppBuffer, strlen(AppBuffer));

				TCPPutROMString(MySocket, (ROM BYTE*)";%");

			 	m_skd_data_to_send.sensor_open_timestamp_sent = m_skd_data_to_send.sensor_open_timestamp;
				return;
			}
		
			if(m_skd_data_to_send.sensor_close_timestamp != m_skd_data_to_send.sensor_close_timestamp_sent)
			{
				TCPPutROMString(MySocket, (ROM BYTE*)"%CMD:CLOSE;0:");

				memset(AppBuffer,0, sizeof(AppBuffer));
				ultoa(m_skd_data_to_send.sensor_close_timestamp, AppBuffer);
				TCPPutArray(MySocket, AppBuffer, strlen(AppBuffer));

				TCPPutROMString(MySocket, (ROM BYTE*)";%");

			 	m_skd_data_to_send.sensor_close_timestamp_sent = m_skd_data_to_send.sensor_close_timestamp;
				m_skd_data.reader_state|=READER_READY;
				m_skd_data.reader_state&=~READER_OPEN;
				m_skd_data.reader_state&=~READER_CLOSE;
				m_skd_data.reader_state&=~READER_KEY;
				return;
			}

			if(m_skd_data_to_send.rte_push_timestamp != m_skd_data_to_send.rte_push_timestamp_sent)
			{
				TCPPutROMString(MySocket, (ROM BYTE*)"%CMD:RTE;0:");

				memset(AppBuffer,0, sizeof(AppBuffer));
				ultoa(m_skd_data_to_send.rte_push_timestamp, AppBuffer);
				TCPPutArray(MySocket, AppBuffer, strlen(AppBuffer));

				TCPPutROMString(MySocket, (ROM BYTE*)";%");

			 	m_skd_data_to_send.rte_push_timestamp_sent = m_skd_data_to_send.rte_push_timestamp;
				return;
			}
return;
			if((g_LedBlinkCount - g_PrevLedBlinkCount) < 2)
			{
				return;
			}
			g_PrevLedBlinkCount = g_LedBlinkCount;
			g_cur_data++;
			if(g_cur_data >= E_CUR_DATA_LAST)
			{
				g_cur_data = E_ANALOG;
			}
			if(g_cur_data == E_ANALOG)
			{
				memcpypgm2ram(AppBuffer, (ROM void *)"%CMD:AI;", 8);
				TCPPutArray(MySocket, AppBuffer, 8);
	
				for(i=0; i < ANALOG_INPUTS;i++)
				{
					sprintf(AppBuffer,"%d:%s;",i,AN0String[i]);
					TCPPutArray(MySocket, AppBuffer, strlen(AppBuffer));
				}
			}
			else
			if(g_cur_data == E_DIGITAL_IN)
			{
				memcpypgm2ram(AppBuffer, (ROM void *)"%CMD:DI;", 8);
				TCPPutArray(MySocket, AppBuffer, 8);

				sprintf(AppBuffer,"0:%d;1:%d;2:%d;3:%d;",BUTTON0_IO,BUTTON1_IO,BUTTON2_IO,BUTTON3_IO);
				TCPPutArray(MySocket, AppBuffer, strlen(AppBuffer));
			}
			else
			if(g_cur_data == E_DIGITAL_OUT)
			{
				memcpypgm2ram(AppBuffer, (ROM void *)"%CMD:DO;", 8);
				TCPPutArray(MySocket, AppBuffer,8);

				sprintf(AppBuffer,"0:%d;1:%d;2:%d;3:%d;4:%d;5:%d;6:%d;7:%d;",LED0_IO,LED1_IO,LED2_IO,LED3_IO,LED4_IO,LED5_IO,LED6_IO,LED7_IO);
				TCPPutArray(MySocket, AppBuffer, strlen(AppBuffer));
			}
			
			memcpypgm2ram(AppBuffer, (ROM void *)"%", 1);
			TCPPutArray(MySocket, AppBuffer, 1);

			// No need to perform any flush.  TCP data in TX FIFO will automatically transmit itself after it accumulates for a while.  If you want to decrease latency (at the expense of wasting network bandwidth on TCP overhead), perform and explicit flush via the TCPFlush() API.

			break;
	}
}

#endif //#if defined(STACK_USE_GENERIC_TCP_SERVER_EXAMPLE)
