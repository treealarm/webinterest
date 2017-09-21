
// W26Convertor.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CW26ConvertorApp:
// See W26Convertor.cpp for the implementation of this class
//

class CW26ConvertorApp : public CWinApp
{
public:
	CW26ConvertorApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CW26ConvertorApp theApp;