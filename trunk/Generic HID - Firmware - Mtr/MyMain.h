
#ifndef MYMAIN_FSUSB_H
#define MYMAIN_FSUSB_H


void ProcessSteps(void);
void RestartTimer(void);
void ProcessStep(int motor);
void SetupDirs(void);
void SetupCtrlSignals(void);
void MyProcessIO(void);
void MyUserInit(void);

void ProcessHome(void);
void InitTestSettings(void);	

#endif