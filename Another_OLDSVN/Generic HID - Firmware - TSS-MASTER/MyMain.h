
#ifndef MYMAIN_FSUSB_H
#define MYMAIN_FSUSB_H

#include "GenericTypeDefs.h"

void MyProcessSend(void);
int ProcessSteps(void);
void ProcessStep(int motor);
void MyProcessIO(void);
void MyUserInit(void);
void AddCommandToSend(void* pData);

void PrepareErrorHeader(void);
void SetErrorToSend(auto const rom char* pData);
void SetErrorToSend1(char* pData);

char ReadEEPROM(void);
void WriteEEPROM (char direction);

void ProcessSPI ();
#endif