#include "StdAfx.h"
#include "HttpChannelWriter.h"
#include "HttpWriter.h"


HttpChannelWriter::HttpChannelWriter(CHttpWriter* pWriter,LPCTSTR stream_name):m_pWriter(pWriter)
{
	m_hHttpRequest = NULL;
	m_stream_name = stream_name;
}


HttpChannelWriter::~HttpChannelWriter(void)
{
}

void HttpChannelWriter::Delete()
{
	delete this;
}

BOOL HttpChannelWriter::Open()
{
	Close();

	USES_CONVERSION;
	std::stringstream streamname;
	std::string isml = ".isml";
	streamname << m_pWriter->m_pMuxer->GetPubPointName() << isml << "/Streams(" << m_stream_name <<")";
	LPCWSTR pwszObjectName = T2CW(streamname.str().c_str());
	m_hHttpRequest = WinHttpOpenRequest( m_pWriter->GetConnection(), L"POST", 
									pwszObjectName, 
									NULL, WINHTTP_NO_REFERER, 
									WINHTTP_DEFAULT_ACCEPT_TYPES,
									0);
	if(!m_hHttpRequest)
	{
		m_pWriter->m_pMuxer->LogEvent("Failed WinHttpOpenRequest");
		return FALSE;
	}


	BOOL fResult = FALSE;
	WCHAR szAdditionalHeaders[] = L"Transfer-Encoding: Chunked";
	fResult = WinHttpSendRequest(
			m_hHttpRequest,
			szAdditionalHeaders,
			ARRAYSIZE(szAdditionalHeaders)-1,
			WINHTTP_NO_REQUEST_DATA,
			0,
			0,
			NULL
			);


	if( !fResult )
	{
		HRESULT hr = HRESULT_FROM_WIN32( GetLastError() );
		m_pWriter->m_pMuxer->LogEvent("Failed WinHttpSendRequest");
		return FALSE;
	}
	return TRUE;
}


BOOL HttpChannelWriter::Close(void)
{
	BOOL bRet = TRUE;
	if(m_hHttpRequest)
	{
		bRet = WinHttpCloseHandle(m_hHttpRequest);
		m_hHttpRequest = NULL;
	}
	return bRet;
}

HRESULT HttpChannelWriter::WriteToSmoothStreamOutput(
        __in HINTERNET hHttpRequest,
        __in_ecount(cbData) LPCVOID pbData,
        __in ULONG cbData )
    {
        HRESULT hr = S_OK;
        BOOL fResult = FALSE;
        DWORD cbWritten;

        char szHttpChunkHeaderA[32];
        char szHttpChunkFooterA[] = "\r\n";

        //
        // Send the HTTP Chunk Transfer Encoding chunk-start mark
        // Observe the use of UTF-8 strings.
        //

        hr = StringCchPrintfA(
                szHttpChunkHeaderA,
                ARRAYSIZE(szHttpChunkHeaderA),
                "%X\r\n",
                cbData );
        if( FAILED(hr) )
        {
			m_pWriter->m_pMuxer->LogEvent("Failed StringCchPrintfA");
            goto done;
        }

        fResult = WinHttpWriteData(
                        hHttpRequest,
                        szHttpChunkHeaderA,
                        (DWORD)( strlen(szHttpChunkHeaderA) * sizeof(char) ),
                        &cbWritten
                        );
        if( !fResult )
        {
            hr = HRESULT_FROM_WIN32( GetLastError() );
			m_pWriter->m_pMuxer->LogEvent("Failed WinHttpWriteData");
            goto done;
        }

        //
        // Send the actual chunk data
        //

        if( cbData > 0 )
        {
            fResult = WinHttpWriteData(
                            hHttpRequest,
                            pbData,
                            cbData,
                            &cbWritten
                            );
            if( !fResult )
            {
                hr = HRESULT_FROM_WIN32( GetLastError() );
				m_pWriter->m_pMuxer->LogEvent("Failed WinHttpWriteData");
                goto done;
            }
        }

        //
        // Send the HTTP Chunk Transfer Encoding chunk-end mark
        //

        fResult = WinHttpWriteData(
                        hHttpRequest,
                        szHttpChunkFooterA,
                        (DWORD)( strlen(szHttpChunkFooterA) * sizeof(char) ),
                        &cbWritten
                        );
        if( !fResult )
        {
            hr = HRESULT_FROM_WIN32( GetLastError() );
			m_pWriter->m_pMuxer->LogEvent("Failed WinHttpWriteData");
            goto done;
        }

    done:
         return( hr );
}  

HRESULT HttpChannelWriter::WriteStreamToOutput(LPCVOID pbData, ULONG cbData, LPCWSTR pszMode)
{
	if(m_hHttpRequest != NULL)
	{
		HRESULT hr = WriteToSmoothStreamOutput(
			m_hHttpRequest,
			pbData, cbData);
	    if( FAILED(hr) )
		{
			char buf[1024];
			sprintf_s(buf, "Error 0x%08x: Failed WriteStreamToOutput \n", hr );
			m_pWriter->m_pMuxer->LogEvent(buf);
		}
		return hr;
	}
	else
	{
			char buf[1024];
			sprintf_s(buf, "Error %s: Failed WriteStreamToOutput No Request \n", m_stream_name );
			m_pWriter->m_pMuxer->LogEvent(buf);
	}
	return S_FALSE;
}
