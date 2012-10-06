// MStudio.h : main header file for the MStudio application
//
#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"       // main symbols


// CMStudioApp:
// See MStudio.cpp for the implementation of this class
//

class CMStudioApp : public CWinApp
{
public:
	CMStudioApp();


// Overrides
public:
	virtual BOOL InitInstance();

// Implementation
	afx_msg void OnAppAbout();
	DECLARE_MESSAGE_MAP()
	GdiplusStartupInput gdiplusStartupInput;
	ULONG_PTR           gdiplusToken;
	virtual int ExitInstance();
};

extern CMStudioApp theApp;