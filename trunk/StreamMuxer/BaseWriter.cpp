#include "StdAfx.h"
#include "BaseWriter.h"

CBaseWriter::CBaseWriter(IMuxerImp* pMuxer):m_pMuxer(pMuxer)
{
}


CBaseWriter::~CBaseWriter(void)
{
	for(map_channels_t::iterator it = m_channels.begin(); it != m_channels.end();it++)
	{
		it->second->Delete();
	}
	m_channels.clear();
}

IChannelWriter* CBaseWriter::GetChannelWriter(LPCTSTR stream_name)
{
	map_channels_t::iterator it = m_channels.find(stream_name);
	if(it == m_channels.end())
	{
		IChannelWriter* pNew = CreateChannelWriter(stream_name);
		m_channels[stream_name] = pNew;
		return pNew;
	}
	return it->second;
}
