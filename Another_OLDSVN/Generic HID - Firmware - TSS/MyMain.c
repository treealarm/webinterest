#include "MyMain.h"
#include "MyTypes.h"
#include <spi.h>
#include <Delays.h>

#include "./USB/usb.h"
#include "HardwareProfile.h"
#include "./USB/usb_function_hid.h"
#include "Compiler.h"


extern unsigned char ReceivedDataBuffer[64];
extern unsigned char ToSendDataBuffer[64];

unsigned char SPIReceivedDataBuffer[64];
unsigned char SPIToSendDataBuffer[64];

extern USB_HANDLE USBInHandle;

#pragma udata packets
my_packet m_packets_to_send[PACKETS_TO_SEND];

#pragma udata others
BYTE m_timer_cnt[MOTORS_COUNT] = {0,0,0,0};
key_data m_cur_reader_card[MAX_READERS];
rte_data m_rte_data[MAX_READERS];
open_close_data m_open_close_data[MAX_READERS];
relay_data m_relay_data[MAX_READERS];
data_header m_data_header;

//sr should be 0, it's active mode, so shottky not required
int g_counter = 0;
int relay_open[MAX_READERS] = {0,0};
int cur_bit[MAX_READERS] = {-1,-1};
BYTE bit_counter[MAX_READERS] = {0,0};
int cur_buffer = 1;
BYTE read_timer[MAX_READERS] = {0,0};
BYTE open_close_timer[MAX_READERS] = {0,0};
BYTE door_open[MAX_READERS] = {0,0};
BYTE reader_busy[MAX_READERS] = {0,0};	

UINT16 command_counter = 1;

int ProcessSteps(void)
{
	  int i = 0;
	  int j = 0;
      int retval = 0;


	for( i = 0; i < MAX_READERS;i++)
	{
		 if(door_open[i] || (!reader_busy[0] && !reader_busy[1]))
		{
		 ProcessOpenClose(i);
		}

	}
	
	if(!reader_busy[1])
	{
		if(!door_open[0] || !m_open_close_data[0].IsOpen)//если реле закрыто или датчик закрыт - можно читать
		{
			ProcessReadData(0);
		}
		
	}
	if(!reader_busy[0])
	{
		if(!door_open[1]|| !m_open_close_data[1].IsOpen)
		{
			ProcessReadData(1);
		}
		
	}
	
	if(!reader_busy[0] && !reader_busy[1])
	{
      ProcessRTE();
	}
	ProcessSPI();
      //if the timer has expired (PIC18 only)
      if(INTCONbits.TMR0IF == 0)
	  {
		return retval;
      }
      
 
          INTCONbits.TMR0IF = 0;          // Clear flag
		  TMR0L = m_do_timer_set.m_timer_res.tmr16.lo;
		  TMR0H = m_do_timer_set.m_timer_res.tmr16.hi;
//here we must set step to 0 for all motors//we've got at least 21 microsec period anyway
		g_counter++;
		if(g_counter > TIMER_FOR_BLINK)
		{
			g_counter = 0;
			PORTDbits.RD3 = !PORTDbits.RD3;
		}
		
		
		for(i = 1; i < PACKETS_TO_SEND; i++)
	    {
			if(m_packets_to_send[i].data[0] != 0)
			{//then we can use this buffer
	   			//PORTDbits.RD3 = 1;//
				break;//something to send
			}
	    }

   for(i=0;i<MOTORS_COUNT;i++)
   {
	if(m_do_timer_set.m_multiplier[i] == m_timer_cnt[i])
	{
		ProcessStep(i);
		m_timer_cnt[i] = 0;
	}
	else
	{
    	m_timer_cnt[i] = m_timer_cnt[i]+1;
	}
   }
	return retval;
}

void ProcessStep(int motor)
{
	int i = 0;
	BYTE port[MAX_READERS];
	if(motor == TIMER_RTE)
   {
		for(i = 0; i < MAX_READERS;i++)
		{
	        if(relay_open[i] == 0)
	       {
	             RelayClose(i);
	       }
			else if(relay_open[i] != 0)
			{
				relay_open[i] = relay_open[i] - 1;
			}
		}
   }
	else
	if(motor == TIMER_READER)
   {
		for(i = 0; i < MAX_READERS;i++)
		{
			if(read_timer[i] == 0)
	       {
				bit_counter[i] = 0;
				cur_bit[i] = -1;
	  			reader_busy[i] = 0;
	       }
			else if(read_timer[i] != 0)
			{
				read_timer[i] = read_timer[i] - 1;
			}
		}

   }
	else
	if(motor == TIMER_OPEN_CLOSE)
    {
		port[0] = PORTDbits.RD2;
		port[1] = PORTEbits.RE0;
		for(i = 0; i < MAX_READERS;i++)
		{
			if(open_close_timer[i] == 0)
	       {
				if(i == 0)
				{
					PORTDbits.RD2 = 1;
				}
				if(i == 1)
				{
	            	PORTEbits.RE0 = 1;
				}
	            

	       }
			else if(open_close_timer[i] != 0)
			{
				if(i == 0)
				{
	            	PORTDbits.RD2 = 0;
				}
				if(i == 1)
				{
	            	PORTEbits.RE0 = 0;
				}

				open_close_timer[i] = open_close_timer[i] - 1;
			}
		}
	}
	else
	if(motor == 3)
	{
    }

}

void ProcessRelayCommand(int i)
{

	if(m_relay_data[i].duration == 0)
	{
		RelayClose(i);
	}
	else
	{
  		relay_open[i] = m_relay_data[i].duration*MULTIPL_FOR_SECOND;
		if(m_relay_data[i].mode == 0)
		{
			RelayOpen(i);
		}
		
	}
}

char ReadEEPROM(void)
{
	char direction = 0;
  /*
   * The first thing to do is to read
   * the start direction from data EEPROM.
   */
  EECON1bits.CFGS = 0;					/* Select Data EEPROM */

  EECON1bits.EEPGD = 0;  /* READ step #1 */
  EEADR = 0;             /* READ step #2 */
  EECON1bits.RD = 1;     /* READ step #3 */
  direction = EEDATA;    /* READ step #4 */
  return direction;
}

void WriteEEPROM (char direction)
{
  /*
   * Store the new direction in EEDATA.
   * Note that since we are already
   * in the high priority interrupt, we do
   * not need to explicitly enable/disable
   * interrupts around the write cycle
   */
  EECON1bits.CFGS = 0;					/* Select Data EEPROM */
  EECON1bits.EEPGD = 0;  /* WRITE step #1 */
  EECON1bits.WREN = 1;   /* WRITE step #2 */
  EEADR = 0;             /* WRITE step #3 */
  EEDATA = direction;    /* WRITE step #4 */
  EECON2 = 0x55;         /* WRITE step #5 */
  EECON2 = 0xaa;         /* WRITE step #6 */
  EECON1bits.WR = 1;     /* WRITE step #7 */
  while (!PIR2bits.EEIF) /* WRITE step #8 */
    ;
  PIR2bits.EEIF = 0;     /* WRITE step #9 */
}

void SPIPrepareSendBuf(void)
{
	INT32 i = 0;   
	data_header* pHeader = 0;
    int packet_to_send_num = PACKETS_TO_SEND;
    int max_counter = UINT_MAX;
	memset((void*)(&SPIToSendDataBuffer),0,sizeof(SPIToSendDataBuffer));
	for(i = 0; i < PACKETS_TO_SEND; i++)
    {
		
		if(m_packets_to_send[i].data[0] != 0)
		{
			pHeader = (data_header*)&m_packets_to_send[i].data[0];
			if(max_counter > pHeader->command_counter)
			{
				max_counter = pHeader->command_counter;
				packet_to_send_num = i;
				if(i == 0)
				{//error high priority to send
					break;
				}
			}
        }
       }
	if( packet_to_send_num < PACKETS_TO_SEND)
	{
		memcpy(
	      		(void*)(&SPIToSendDataBuffer[0]),
	      		(void*)(&m_packets_to_send[packet_to_send_num]),
	      		PACKET_MAX_LEN );
		memset((void*)(&m_packets_to_send[packet_to_send_num]),0,sizeof(m_packets_to_send[packet_to_send_num]));
	}
	else
	{
		if(command_counter > 10000)
		{
			command_counter = 1;
		}
	}
}


void ProcessSPI(void)
{
   INT32 i = 0;   
	data_for_cont_header temp_in_header;
    if(0!=WriteSPI(0xF5))
	{	
		return;		
	}
  		memset((void*)(SPIReceivedDataBuffer),0,sizeof(SPIReceivedDataBuffer));
  		memset((void*)(SPIReceivedDataBuffer),0,sizeof(SPIReceivedDataBuffer));
		//************ WRITE THE STRING TO SPI *************************************
		for(i=0;i<64;i++)
		{
			SPIReceivedDataBuffer[i] = ReadSPI();
		}
		SPIPrepareSendBuf();
		for(i=0;i<64;i++)
		{
			WriteSPI(SPIToSendDataBuffer[i]);
		}
	
////Process received buf
	memcpy(
		(void*)(&temp_in_header),
		(void*)(&SPIReceivedDataBuffer[0]),
		sizeof(temp_in_header) );
	 switch(temp_in_header.command_id)
	 {
	   	case 0x80:  //Toggle LEDs
	       PORTDbits.RD3 = 1;// !PORTDbits.RD3;
	    break;
		case COMMAND_RELAY:
	    {
			if(temp_in_header.reader_id >= MAX_READERS)
			{
				SetErrorToSend("bad in com");
				return;
			}
			memcpy(
	           		(void*)(&m_relay_data[temp_in_header.reader_id]),
	           		(void*)(&SPIReceivedDataBuffer[sizeof(temp_in_header)]),
	           		sizeof(m_relay_data[0]) );
			ProcessRelayCommand(temp_in_header.reader_id);
	    }
		break;
	 }
}

void MyProcessIO(void)
{ 
   INT32 i = 0;   
data_for_cont_header temp_in_header;
memcpy(
    		(void*)(&temp_in_header),
    		(void*)(&ReceivedDataBuffer[0]),
    		sizeof(temp_in_header) );
 switch(temp_in_header.command_id)
 {
   			case 0x80:  //Toggle LEDs
                PORTDbits.RD3 = 1;// !PORTDbits.RD3;
                break;
            case 0x81:  //Get push button state (available state)
				if(!HIDTxHandleBusy(USBInHandle))
                {
                    USBInHandle = HIDTxPacket(HID_EP,(BYTE*)&ToSendDataBuffer,64);
                }
                break;
	case COMMAND_RELAY:
    {
		if(temp_in_header.reader_id >= MAX_READERS)
		{
			SetErrorToSend("bad in com");
			return;
		}
		memcpy(
           		(void*)(&m_relay_data[temp_in_header.reader_id]),
           		(void*)(&ReceivedDataBuffer[sizeof(temp_in_header)]),
           		sizeof(m_relay_data[0]) );
		ProcessRelayCommand(temp_in_header.reader_id);
    }
	break;
	case 0x84:
		WriteEEPROM(ReceivedDataBuffer[1]);
		//PORTAbits.RA5 = ReadEEPROM();
	break;
 }
  
}//end MyProcessIO

void RelayOpen(BYTE reader)
{
	if(door_open[reader])
	{
		return;
	}
	  if(reader == 0)
	 {
       PORTDbits.RD6 = 1;
     }
	  if(reader == 1)
	 {
       PORTDbits.RD7 = 1;
     }
	door_open[reader] = 1;
}
void RelayClose(BYTE reader)
{
	if(!door_open[reader])
	{
		return;
	}

 	if(reader == 0)
	 {
       PORTDbits.RD6 = 0;
     }
 	if(reader == 1)
	 {
       PORTDbits.RD7 = 0;
     }
	door_open[reader] = 0;
}

void ProcessOpenClose(BYTE reader)
{
	int i = 0;
	BYTE port;
if(reader == 0)
{
	port = PORTDbits.RD5;
}
else
{
	port = PORTEbits.RE2;
}




  if((!m_open_close_data[reader].IsOpen) != port)//0 - open 1 closed (all inverted)
	{
		m_open_close_data[reader].IsOpen = !port;
		PrepareOpenCloseHeader(reader);
		AddCommandToSend(&m_open_close_data[reader]);
	}

}

void ProcessRTE(void)
{
	int i = 0;
	BYTE port[MAX_READERS];
	port[0] = PORTDbits.RD4;
	port[1] = PORTEbits.RE1;
	for(i = 0 ; i<MAX_READERS;i++)
	{
	   if(m_rte_data[i].RTE != port[i])
		{
	      m_rte_data[i].RTE = port[i];
		  if(m_rte_data[i].RTE == 0 &&  0 == relay_open[i])
			{
				PrepareRteHeader(i);
	          	AddCommandToSend(&m_rte_data[i]);
			}
		}
	
		if(port[i] == 0)
	    {
	      relay_open[i] = m_relay_data[i].duration*MULTIPL_FOR_SECOND;
	      RelayOpen(i);
	    }
	}
}
int ProcessReadData(BYTE reader)
{
    int d0 = 0;
    int d1 = 0;
    int now_bit = -1;
    DWORD_VAL edenica;

    edenica.Val = 0;
	if(reader == 0)
	{
	    d0 = PORTDbits.RD0;//DATA0
	    d1 = PORTDbits.RD1;//DATA1
	}
	else
	{
	    d0 = PORTBbits.RB3;//DATA0
	    d1 = PORTBbits.RB2;//DATA1
	}
 
    if(d1 == d0)
	{
      cur_bit[reader] = -1;
      now_bit = -1;
	  return 0;
	}
	else if (d0 == 1)
	{
      now_bit = 0;
	}
	else if (d1 == 1)
	{
      now_bit = 1;
	}

    if(cur_bit[reader] != now_bit)
    {
      read_timer[reader] = READ_TIMER_FOR_BIT_READ;
      edenica.Val = 1;
      cur_bit[reader] = now_bit;
      if(now_bit == 0)
      {
        m_cur_reader_card[reader].wiegand26.Val &= ~(edenica.Val << bit_counter[reader]); 
      }
      else
		{
	          m_cur_reader_card[reader].wiegand26.Val |= (edenica.Val << bit_counter[reader]); 
		}
      bit_counter[reader]++;
	  reader_busy[reader] = 1;
      if(bit_counter[reader] == 26)
		{
	  		reader_busy[reader] = 0;
		  	open_close_timer[reader] = READ_CLOSE_DELAY;
          	bit_counter[reader] = 0;
			cur_bit[reader] = -1;
			PrepareHeader(reader);
          	AddCommandToSend(&m_cur_reader_card[reader]);
		}
		return 1;
    }
	return 1;
}
void MyUserInit(void)
{
// Timer0 - 1 second interval setup.
// Fosc/4 = 12MHz
// Use /256 prescalar, this brings counter freq down to 46,875 Hz
// Timer0 should = 65536 - 46875 = 18661 or 0x48E5
	int i = 0;
	//m_do_timer_set.m_timer_res.tmr16.lo   =      0xE5;
	//m_do_timer_set.m_timer_res.tmr16.hi   =      0x48;
	m_do_timer_set.m_timer_res.tmr16.lo   =      0xB0;//~100 millisec
	m_do_timer_set.m_timer_res.tmr16.hi   =      0xED;

	TMR0L = m_do_timer_set.m_timer_res.tmr16.lo;
	TMR0H = m_do_timer_set.m_timer_res.tmr16.hi;

	for(i=0;i<MOTORS_COUNT;i++)
	{
		m_do_timer_set.m_multiplier[i] = 0;
	}
    

///////////////////////////////////.TRIS/ OUTPUTS///////////////
    TRISDbits.TRISD3 = 0;//LED BOARD
	PORTDbits.RD3 = 0;

    TRISDbits.TRISD2 = 0;//LED READER
	PORTDbits.RD2 = 1;
    TRISEbits.TRISE0 = 0;//LED READER
	PORTEbits.RE0 = 1;

    TRISDbits.TRISD6 = 0;//RELAY
    TRISDbits.TRISD7 = 0;//RELAY
	for(i=0;i<MAX_READERS;i++)
	{
	    m_relay_data[i].duration = RELAY_DEFAULT_OPEN;
		RelayClose(i);
  		memset((void*)(&m_cur_reader_card[i]),0,sizeof(m_cur_reader_card[i]));
	}
    
///////////////////////////////////.TRIS/ INPUTS///////////////
    TRISDbits.TRISD0 = 1;//DATA0
	TRISDbits.TRISD1 = 1;//DATA1

    TRISBbits.TRISB3 = 1;//DATA0
	TRISBbits.TRISB2 = 1;//DATA1

	TRISDbits.TRISD4 = 1;//R - RTE
	TRISEbits.TRISE1 = 1;//R - RTE
	TRISDbits.TRISD5 = 1;//D - DATA SENSOR
	TRISEbits.TRISE2 = 1;//D - DATA SENSOR


	TRISAbits.TRISA5 = 1;//MEMORY

    T0CON = 0b10010111;
// Set Timer0 to trigger interrupt
    INTCONbits.TMR0IF = 0;          // Clear flag
	INTCONbits.TMR0IE = 1;
// Set TO to be a 16bits timer

  	memset((void*)(&m_packets_to_send),0,sizeof(m_packets_to_send));
    //m_cur_reader_card.Val = 0b01010101010101010101010101;


	
	//PORTAbits.RA5 = ReadEEPROM();

	SetErrorToSend("init");

	
   CloseSPI();					// Turn off SPI modules  if was previosly on
    
//***********Configure SPI SLAVE  module  ***********

   OpenSPI(SLV_SSON,MODE_01,SMPMID);
   
}//end MyUserInit
void PrepareHeader(BYTE reader)
{
    m_data_header.command_counter = command_counter;
	m_data_header.cont_id = 1;
	m_data_header.reader_id = reader;
	m_data_header.command_id = 'k';
	m_data_header.data_size = sizeof(key_data);
	command_counter++;
}

void PrepareRteHeader(BYTE reader)
{
    m_data_header.command_counter = command_counter;
	m_data_header.cont_id = 1;
	m_data_header.reader_id = reader;
	m_data_header.command_id = 'r';
	m_data_header.data_size = sizeof(rte_data);
	command_counter++;
}

void PrepareOpenCloseHeader(BYTE reader)
{
    m_data_header.command_counter = command_counter;
	m_data_header.cont_id = 1;
	m_data_header.reader_id = reader;
	m_data_header.command_id = 'd';
	m_data_header.data_size = sizeof(open_close_data);
	command_counter++;
}

void PrepareErrorHeader(void)
{
    m_data_header.command_counter = command_counter;
	m_data_header.cont_id = 1;
	m_data_header.reader_id = 1;
	m_data_header.command_id = 'e';
	m_data_header.data_size = PACKET_MAX_LEN - sizeof(m_data_header);
	command_counter++;
}
void AddCommandToSend(void* pData)
{
	data_header* pHeader = 0;
    int i = 0;
	for(i = 1; i < PACKETS_TO_SEND; i++)
    {
		pHeader = (data_header*)&m_packets_to_send[i].data[0];
		if(pHeader->command_counter == 0)
		{//then we can use this buffer

			memcpy(
        		(void*)(&m_packets_to_send[i].data[0]),
        		(void*)(&m_data_header),
        		sizeof(m_data_header) );

			memcpy(
        		(void*)(&m_packets_to_send[i].data[sizeof(m_data_header)+1]),
        		(void*)pData,
        		m_data_header.data_size);
			return;
		}
    }
	SetErrorToSend("queue is full");
}
void SetErrorToSend(auto const rom char* pData)
{
	PrepareErrorHeader();
	memcpy(
      		(void*)(&m_packets_to_send[0].data[0]),
      		(void*)(&m_data_header),
      		sizeof(m_data_header) );

	strncpypgm2ram(
      		(char*)(&m_packets_to_send[0].data[sizeof(m_data_header)+1]),
			pData,
			(sizeram_t)m_data_header.data_size-1);

}