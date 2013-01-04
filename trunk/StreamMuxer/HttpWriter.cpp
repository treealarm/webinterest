#include "StdAfx.h"
#include "HttpWriter.h"
#include "HttpChannelWriter.h"


CHttpWriter::CHttpWriter(IMuxerImp* pMuxer):CBaseWriter(pMuxer)
{
	m_hConnect = NULL;
	m_hSession = NULL;
}


CHttpWriter::~CHttpWriter(void)
{
}

HINTERNET CHttpWriter::GetConnection()
{
	return m_hConnect;
}

BOOL CHttpWriter::OpenWriter()
{
	// Use WinHttpOpen to obtain an HINTERNET handle.
	HRESULT hr = S_OK;
    m_hSession = WinHttpOpen(L"A WinHTTP Example Program/1.0", 
                                    WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
                                    WINHTTP_NO_PROXY_NAME, 
                                    WINHTTP_NO_PROXY_BYPASS, 0);

	if(!m_hSession)
	{
		m_pMuxer->LogEvent("Failed WinHttpOpen");
		CloseWriter();
		return FALSE;
	}
	 USES_CONVERSION;
	 m_hConnect = WinHttpConnect( m_hSession, T2CW(m_pMuxer->GetPubPointIp().c_str()),
                                   INTERNET_DEFAULT_HTTP_PORT, 0);

	if(!m_hConnect)
	{
		m_pMuxer->LogEvent("Failed WinHttpConnect");
		CloseWriter();
		return FALSE;
	}

	return TRUE;
}

BOOL CHttpWriter::CloseWriter()
{
	BOOL bRet = TRUE;
	if(m_hConnect)
	{
		bRet = WinHttpCloseHandle(m_hConnect);
		m_hConnect = NULL;
	}
	if(m_hSession)
	{
		bRet = WinHttpCloseHandle(m_hSession);
		m_hSession = NULL;
	}
	return bRet;
}
IChannelWriter* CHttpWriter::CreateChannelWriter(LPCTSTR stream_name)
{
	return new HttpChannelWriter(this,stream_name);
}
