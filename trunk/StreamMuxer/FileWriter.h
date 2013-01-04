#pragma once
#include "basewriter.h"
class CFileWriter :
	public CBaseWriter
{
public:
	CFileWriter(IMuxerImp* pMuxer);
	virtual ~CFileWriter(void);
	virtual BOOL OpenWriter();
	virtual BOOL CloseWriter();
	virtual IChannelWriter* CreateChannelWriter(LPCTSTR stream_name);
};

