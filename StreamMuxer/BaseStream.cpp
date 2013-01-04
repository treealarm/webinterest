#include "StdAfx.h"
#include "BaseStream.h"


CBaseStream::CBaseStream(IMuxerImp* pMuxer):m_pMuxer(pMuxer)
{
	m_eMuxState = UNDEFINED;
	m_pChannelWriter = NULL;
	m_last_write = 0;
}


CBaseStream::~CBaseStream(void)
{
	if(m_pChannelWriter)
	{
		m_pChannelWriter->Close();
	}	
}


DWORD* CBaseStream::GetStreamIndex()
{
	return &m_Ctx.m_dwStreamIndex;
}


BOOL CBaseStream::OpenChannelWriter()
{
	if(!m_pMuxer)
	{
		return FALSE;
	}
	m_pChannelWriter = m_pMuxer->GetChannelWriter(GetStreamName().c_str());
	if(!m_pChannelWriter)
	{
		return FALSE;
	}
	return m_pChannelWriter->Open();
}

StreamContext* CBaseStream::GetCtx()
{
	return &m_Ctx;
}

LPCWSTR CBaseStream::GetStreamNameW()
{
	USES_CONVERSION;
	wcscpy_s(szStreamName,T2CW(GetStreamName().c_str()));
	return szStreamName;
}

HRESULT CBaseStream::WriteStreamToOutput(LPCVOID pbData, ULONG cbData, LPCWSTR pszMode)
{
	if(m_pChannelWriter != NULL)
	{
		return m_pChannelWriter->WriteStreamToOutput(pbData, cbData, pszMode);
	}
	else
	{
			char buf[1024];
			sprintf_s(buf, "Error %s: Failed WriteStreamToOutput No Request \n", GetStreamName().c_str() );
			m_pMuxer->LogEvent(buf);
	}
	return S_FALSE;
}

E_StreamState CBaseStream::GetState()
{
	return m_eMuxState;
}
void CBaseStream::SetState(E_StreamState state)
{
	m_eMuxState = state;
}