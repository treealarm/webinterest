#include "MyMain.h"
#include "MyTypes.h"
#include <spi.h>
#include <Delays.h>

#include "./USB/usb.h"
#include "HardwareProfile.h"
#include "./USB/usb_function_hid.h"
#include "Compiler.h"

unsigned char SPIReceivedDataBuffer[64];
unsigned char SPIToSendDataBuffer[64];

extern unsigned char ReceivedDataBuffer[64];
extern unsigned char ToSendDataBuffer[64];
extern USB_HANDLE USBInHandle;

#pragma udata packets
my_packet m_packets_to_send[PACKETS_TO_SEND];

#pragma udata others
BYTE m_timer_cnt[MOTORS_COUNT] = {0,0,0,0};
data_header m_data_header;

//sr should be 0, it's active mode, so shottky not required
int g_counter = 0;

UINT16 command_counter = 1;

void ProcessSPI ()
{
	`int i = 0;
	PORTAbits.RA5 = 0;
	if( 0xF5 == ReadSPI()) 
	{
		Delay10TCYx(10);
		memset((void*)(&SPIReceivedDataBuffer),0,sizeof(SPIReceivedDataBuffer));

		for(i = 0;i<64;i++)
		{
			WriteSPI(SPIToSendDataBuffer[i]);
			Delay10TCYx(10);
		}
//clean sent buffer
		memset((void*)(&SPIToSendDataBuffer),0,sizeof(SPIToSendDataBuffer));

		for(i=0;i<64;i++)
		{
			SPIReceivedDataBuffer[i] = ReadSPI();
			Delay10TCYx(10);
		}
		AddCommandToSend(SPIReceivedDataBuffer);
	}
	PORTAbits.RA5 = 1;
}

int ProcessSteps(void)
{
   
	  int i = 0;
	  int j = 0;
      int retval = 0;
	  ProcessSPI ();
//********** WRITE THE STRING TO SPI ***************************************
	//putsSPI(SPI_Send);					//send the string of data to be sent to slave

//********* Read the initial flag id **********************************************
	  
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
			if(PORTDbits.RD3)
			SetErrorToSend("ping");
			else
			SetErrorToSend("pong");
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
	
}
void MyProcessSend(void)
{
	INT32 i = 0;   
	data_header* pHeader = 0;
    int packet_to_send_num = PACKETS_TO_SEND;
    int max_counter = UINT_MAX;
	memset((void*)(&ToSendDataBuffer),0,sizeof(ToSendDataBuffer));
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
		 if(!HIDTxHandleBusy(USBInHandle))
            {
			memcpy(
        		(void*)(&ToSendDataBuffer[0]),
        		(void*)(&m_packets_to_send[packet_to_send_num]),
        		PACKET_MAX_LEN );
			memset((void*)(&m_packets_to_send[packet_to_send_num]),0,sizeof(m_packets_to_send[packet_to_send_num]));
				
              return;
            }
	}
	else
	{
		if(command_counter > 10000)
		{
			command_counter = 1;
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

void MyProcessIO(void)
{ 
   INT32 i = 0;   
data_for_cont_header temp_in_header;
memcpy(
    		(void*)(&temp_in_header),
    		(void*)(&ReceivedDataBuffer[0]),
    		sizeof(temp_in_header) );

		memset((void*)(&SPIToSendDataBuffer),0,sizeof(SPIToSendDataBuffer));

 switch(temp_in_header.command_id)
 {
   			case 0x80:  //Toggle LEDs
                PORTDbits.RD3 = 1;// !PORTDbits.RD3;
                break;
            case 0x81:  //Get push button state (available state)
				MyProcessSend();
				if(!HIDTxHandleBusy(USBInHandle))
                {
                    USBInHandle = HIDTxPacket(HID_EP,(BYTE*)&ToSendDataBuffer,64);
                }
                break;
	
	case 0x84:
		WriteEEPROM(ReceivedDataBuffer[1]);
		//PORTAbits.RA5 = ReadEEPROM();
	break;
default:
    {
//resend usb to spi
memcpy(
           		(void*)(SPIToSendDataBuffer),
           		(void*)(ReceivedDataBuffer),
           		sizeof(SPIToSendDataBuffer) );
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
	PORTDbits.RD3 = 1;

	TRISAbits.TRISA5 = 0;//MEMORY

    T0CON = 0b10010111;
// Set Timer0 to trigger interrupt
    INTCONbits.TMR0IF = 0;          // Clear flag
	INTCONbits.TMR0IE = 1;
// Set TO to be a 16bits timer

  	memset((void*)(&m_packets_to_send),0,sizeof(m_packets_to_send));
    //m_cur_reader_card.Val = 0b01010101010101010101010101;


	
	//PORTAbits.RA5 = ReadEEPROM();

	SetErrorToSend("init");
   
   CloseSPI();							// Turn off SPI modules  if was previosly on
     
//***********Configure SPI MASTER  module to transmit in master mode ************
   
   OpenSPI(SPI_FOSC_64,MODE_01,SMPMID );
   
}//end MyUserInit

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
        		(void*)(pData),
        		PACKET_MAX_LEN );

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
void SetErrorToSend1(char* pData)
{
	PrepareErrorHeader();
	memcpy(
      		(void*)(&m_packets_to_send[0].data[0]),
      		(void*)(&m_data_header),
      		sizeof(m_data_header) );

	strncpy(
      		(char*)(&m_packets_to_send[0].data[sizeof(m_data_header)+1]),
			pData,
			(sizeram_t)m_data_header.data_size-1);
}