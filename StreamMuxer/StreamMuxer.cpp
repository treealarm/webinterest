// StreamMuxer.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "StreamMuxer.h"
#include "StreamMuxerImp1.h"

namespace PPTCreator
{
STREAMMUXER_API BOOL CreatePPT(LPCTSTR ip, WORD port, LPCTSTR ppt_name)
{
	BOOL bRet = FALSE;
	//http://localhost:52878/PPCreateHandler.service?name=1_.isml&stop=true
  DWORD dwSize = 0;
  DWORD dwDownloaded = 0;
  LPSTR pszOutBuffer;
  BOOL  bResults = FALSE;
  HINTERNET  hSession = NULL, 
             hConnect = NULL,
             hRequest = NULL;

  USES_CONVERSION;
  // Use WinHttpOpen to obtain a session handle.
  hSession = WinHttpOpen( L"WinHTTP Example/1.0",  
                          WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
                          WINHTTP_NO_PROXY_NAME, 
                          WINHTTP_NO_PROXY_BYPASS, 0 );

  // Specify an HTTP server.
  if( hSession )
    hConnect = WinHttpConnect( hSession, T2CW(ip),
                               port, 0 );

  std::string request = "/PPCreateHandler.service?stop=true&name=" + std::string(ppt_name);
  // Create an HTTP request handle.
  if( hConnect )
	  hRequest = WinHttpOpenRequest( hConnect, L"GET", T2CW(request.c_str()),
                                   NULL, WINHTTP_NO_REFERER, 
                                   WINHTTP_DEFAULT_ACCEPT_TYPES, 
                                   0 );

  // Send a request.
  if( hRequest )
    bResults = WinHttpSendRequest( hRequest,
                                   WINHTTP_NO_ADDITIONAL_HEADERS, 0,
                                   WINHTTP_NO_REQUEST_DATA, 0, 
                                   0, 0 );


  // End the request.

  if( bResults )
    bResults = WinHttpReceiveResponse( hRequest, NULL );

  // Keep checking for data until there is nothing left.
  if( bResults )
  {
    do 
    {
      // Check for available data.
      dwSize = 0;
      if( !WinHttpQueryDataAvailable( hRequest, &dwSize ) )
        printf_s( "Error %u in WinHttpQueryDataAvailable.\n",
                GetLastError( ) );

      // Allocate space for the buffer.
      pszOutBuffer = new char[dwSize+1];
      if( !pszOutBuffer )
      {
        printf_s( "Out of memory\n" );
        dwSize=0;
      }
      else
      {
        // Read the data.
        ZeroMemory( pszOutBuffer, dwSize+1 );

        if( !WinHttpReadData( hRequest, (LPVOID)pszOutBuffer, 
                              dwSize, &dwDownloaded ) )
          printf_s( "Error %u in WinHttpReadData.\n", GetLastError( ) );
        else
          printf_s( "%s", pszOutBuffer );
		if(strcmp("<h1>true</h1>",pszOutBuffer) == 0)
		{
			bRet = TRUE;
		}
        // Free the memory allocated to the buffer.
        delete [] pszOutBuffer;
      }
    } while( dwSize > 0 );
  }


  // Report any errors.
  if( !bResults )
    printf_s( "Error %d has occurred.\n", GetLastError( ) );

  // Close any open handles.
  if( hRequest ) WinHttpCloseHandle( hRequest );
  if( hConnect ) WinHttpCloseHandle( hConnect );
  if( hSession ) WinHttpCloseHandle( hSession );

  return bRet;
}
}

// This is the constructor of a class that has been exported.
// see StreamMuxer.h for the class definition
CStreamMuxer::CStreamMuxer(IMuxerLogger* pLogger):
m_pLogger(pLogger)
{
	m_pMuxer = NULL;
}

IMuxerLogger* CStreamMuxer::GetLogger()
{
	return m_pLogger;
}

int CStreamMuxer::DestroyStreamMuxer()
{
	if(m_pMuxer)
	{
		int ret =  m_pMuxer->DestroyStreamMuxer();
		delete m_pMuxer;
		m_pMuxer = NULL;
		return ret;
	}
	return 0;
}
    

int CStreamMuxer::PushVideoStream(VF_STRUCT& vf)
{
	if(m_pMuxer)
	{
		m_pMuxer->SetWH(vf);
		return m_pMuxer->PushStream(vf.pData, vf.len, vf.isSyncPoint, vf.ts);
	}
	return 0;
}

BOOL CStreamMuxer::IsVideoStreamAdded()
{
	if(m_pMuxer)
	{
		return m_pMuxer->IsVideoStreamAdded();
	}
	return FALSE;
}

BOOL CStreamMuxer::IsAudioStreamAdded()
{
	if(m_pMuxer)
	{
		return m_pMuxer->IsAudioStreamAdded();
	}
	return FALSE;
}

int CStreamMuxer::CreateStreamMuxer(LPCTSTR PublishIp, LPCTSTR PublPoint, BOOL bLive, LPCTSTR DstDir)
{
	if(m_pMuxer)
	{
		DestroyStreamMuxer();
	}

	m_pMuxer = new CStreamMuxerImp1(this);

	return m_pMuxer->CreateStreamMuxer(PublishIp, PublPoint, bLive, DstDir);
}

BOOL CStreamMuxer::AddAudioStream(int samplerate, int bitrate, unsigned __int8 *extradata, int extradata_size)
{
	if(!m_pMuxer)
	{
		return FALSE;
	}
	return m_pMuxer->AddAudioStream(samplerate, bitrate, extradata, extradata_size);
}

int CStreamMuxer::PushAudioStream(BYTE* pData, int len, LONGLONG ts)
{
	if(!m_pMuxer)
	{
		return 0;
	}
	return m_pMuxer->PushAudioStream(pData, len, ts);
}

 