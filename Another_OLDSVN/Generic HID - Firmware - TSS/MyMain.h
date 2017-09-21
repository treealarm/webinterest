
#ifndef MYMAIN_FSUSB_H
#define MYMAIN_FSUSB_H

#include "GenericTypeDefs.h"

void SPIPrepareSendBuf(void);
int ProcessSteps(void);
void ProcessStep(int motor);
void MyProcessIO(void);
void MyUserInit(void);
void ProcessRTE(void);
void RelayOpen(BYTE reader);
void RelayClose(BYTE reader);
int ProcessReadData(BYTE reader);
void AddCommandToSend(void* pData);
void PrepareHeader(BYTE reader);
void PrepareRteHeader(BYTE reader);
void ProcessOpenClose(BYTE reader);
void PrepareOpenCloseHeader(BYTE reader);
void ProcessRelayCommand(int i);
void PrepareErrorHeader(void);
void SetErrorToSend(auto const rom char* pData);

char ReadEEPROM(void);
void WriteEEPROM (char direction);

void ProcessSPI(void);
#endif