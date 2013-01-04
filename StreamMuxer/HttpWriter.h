#pragma once
#include "basewriter.h"
class CHttpWriter :
	public CBaseWriter
{
	HINTERNET m_hConnect;
	HINTERNET m_hSession;
public:
	HINTERNET GetConnection();
	CHttpWriter(IMuxerImp* pMuxer);
	virtual ~CHttpWriter(void);
	virtual BOOL OpenWriter();
	virtual BOOL CloseWriter();
	virtual IChannelWriter* CreateChannelWriter(LPCTSTR stream_name);
};

