
#ifndef MYMAIN_FSUSB_H
#define MYMAIN_FSUSB_H


void ProcessSteps(void);
void RestartTimer(void);
void ProcessStep(int motor);
void SetupDirs(void);
void SetupCtrlSignals(void);
void MyProcessIO(void);
void MyUserInit(void);

void InitTestSettings(void);	

void ProcessInkImpuls(int new_impuls);
int CheckIfMotorsOnZero(void);
int CheckIfBufferOnZero(void);
void CopyBufferToMotor(void);

#endif