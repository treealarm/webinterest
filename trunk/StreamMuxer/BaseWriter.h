#pragma once

#include "MuxerInterface.h"

typedef std::map<std::string, IChannelWriter*> map_channels_t;
class CBaseWriter
{
	map_channels_t m_channels; 
public:
	IMuxerImp* m_pMuxer;
	CBaseWriter(IMuxerImp* pMuxer);
	virtual ~CBaseWriter(void);
	virtual BOOL OpenWriter() = 0;
	virtual BOOL CloseWriter() = 0;
	virtual IChannelWriter* CreateChannelWriter(LPCTSTR stream_name) = 0;
	virtual IChannelWriter* GetChannelWriter(LPCTSTR stream_name);	
};

