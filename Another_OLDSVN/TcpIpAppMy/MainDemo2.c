//#define THIS_IS_STACK_APPLICATION

#include "TCPIPConfig.h"
#include "TCPIP Stack/StackTsk.h"
#include "MainDemo.h"


#pragma romdata ROMAPPCONFIG = 0x17400
ROM BYTE RomConfBuff[FLASH_ERASE_SIZE]={'1','2','3','4','5'}; 
ROM BYTE RomCardBuff[FLASH_ERASE_SIZE]; 
#pragma romdata 

#pragma udata temp_buffer_big
BYTE temp_buffer_big0[TEMP_BUFFER_BIG_SIZE];
#pragma udata


BYTE*  temp_buffer_big = &temp_buffer_big0[0];

EXTRA_SETTINGS AutonomSettings;
skd_data m_skd_data;
skd_settings m_skd_settings;
skd_data_to_send m_skd_data_to_send;
INT8 m_autonom = 1;

DWORD_VAL edenica;


void SaveCard(int number,DWORD card)
{
	int i = 0;

	memcpypgm2ram (temp_buffer_big, (const rom void *)&RomCardBuff[0],TEMP_BUFFER_BIG_SIZE);

	memcpy(temp_buffer_big + number*sizeof(card),(void*)&card,sizeof(card));

	RawEraseFlashBlock((DWORD)&RomCardBuff[0]);
	for( i = 0; i < 1024; i+=FLASH_WRITE_SIZE)
	{
		RawWriteFlashBlock((DWORD)&RomCardBuff[i], temp_buffer_big + i, NULL);
	}
}

#define LOBYTE1(w)           ((BYTE)(((WORD)(w)) & 0xff))
#define HIBYTE1(w)           ((BYTE)((((WORD)(w)) >> 8) & 0xff))

int getBit(BYTE data, int pos) 
{
	static unsigned char mask_table[] = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
	int bit_n = ( data & mask_table[ pos ] ) != 0x00;
	return bit_n;
}

DWORD Wiegand2TouchMemoryDword(char* card_number, char* facility_code)
{//прогнать эмулятором 95 24799==000000BEC1BE==12501438
	BYTE fc = (0);
	WORD card = (0);
	int l = (0);
	int r = (0);
	int cl = (0);
	int cr = (0);
	int i = 0;
	int j = 0;
	int even_l = 0;
	int even_f = 0;
	DWORD code = 0;
	int curbit = 1;

	fc = (BYTE)atoi(facility_code);
	card = (WORD)atoi(card_number);


	for(i = 0; i < sizeof(card);i++)
	{
		for(j=0;j<8;j++)
		{
			int bit = getBit(*(((BYTE*)&card)+i),j);

			if(bit)
			{
				code |=  ((DWORD)1) << curbit;
			}
			curbit++;
		}
	}

	for(j=0;j<8;j++)
	{
		int bit = getBit(fc,j);

		if(bit)
		{
			code |=  ((DWORD)1) << curbit;
		}
		curbit++;
	}

	r = code & 0x1fff;
	l = (code >> 13) & 0x1fff;

	while( r > 0 )
	{
		if( r & 1 > 0 )
			cr++;
		r = r >> 1;
	}

	while( l > 0 )
	{
		if( l & 1 > 0 )
			cl++;
		l = l >> 1;
	}

	even_l	= cl % 2 == 0 ? 0 : 1;
	even_f	= cr % 2 == 0 ? 1 : 0;					
	if(even_l)
	{
		code |=  ((DWORD)1);
	}
	if(even_f)
	{
		code |=  ((DWORD)1) << curbit;
	}
	return code;
}