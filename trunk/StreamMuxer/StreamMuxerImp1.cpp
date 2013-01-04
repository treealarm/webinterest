#include "StdAfx.h"
#include "StreamMuxer.h"
#include "StreamMuxerImp1.h"
#include "AudioStream.h"
#include "VideoStream.h"
#include "HttpWriter.h"
#include "FileWriter.h"

CStreamMuxerImp1::CStreamMuxerImp1(CStreamMuxer* pParent):
m_pParent(pParent)
{
	m_bLive = TRUE;
	m_hSSFMux = NULL;
	m_pAudioStream = NULL;
	m_pWriter = NULL;
	m_pVideoStream = new CVideoStream(this);
	m_ArchiveFolder = "";
	ZeroMemory(&m_VF_STRUCT,sizeof(m_VF_STRUCT));
}


CStreamMuxerImp1::~CStreamMuxerImp1(void)
{
	if(m_pAudioStream)
	{
		delete m_pAudioStream;
	}
	if(m_pVideoStream)
	{
		delete m_pVideoStream;
	}
	if(m_pWriter)
	{
		delete m_pWriter;
	}
}

void CStreamMuxerImp1::SetWH(VF_STRUCT& vf)
{
	if(vf.w > 0 && vf.h > 0)
	{
		CopyMemory(&m_VF_STRUCT,&vf,sizeof(m_VF_STRUCT));
		m_pVideoStream->SetWH(vf.w, vf.h);
	}
}

void CStreamMuxerImp1::LogEvent(LPCTSTR message)
{
	m_pParent->GetLogger()->Log(message);
}

CBaseWriter* CStreamMuxerImp1::GetWritter()
{
	if(!m_pWriter)
	{
		if(m_bLive)
		{
			m_pWriter = new CHttpWriter(this);
		}
		else
		{
			m_pWriter = new CFileWriter(this);
		}
	}
	else
	{
		return m_pWriter;
	}

	if(!m_pWriter->OpenWriter())
	{
		delete m_pWriter;
		m_pWriter = NULL;
		return NULL;
	}

	return m_pWriter;
}

int CStreamMuxerImp1::DoCreateStreamMuxer(BOOL fLive)
{
	//fLive = FALSE;
	m_bLive = fLive;
	
	if(!GetWritter())
	{
		return -1;
	}
	HRESULT hr = S_OK;
    
	
	//////////////////////////////////////////////////
	hr = SSFMuxCreate( &m_hSSFMux );
    if( FAILED(hr) )
    {
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to create SSF muxer\n", hr );
		LogEvent(buf);
        goto done;
    }

    hr = SSFMuxSetOption( m_hSSFMux, SSF_MUX_OPTION_LIVE_MODE, &fLive, sizeof(fLive) );
    if( FAILED(hr) )
    {
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to set SSF_MUX_OPTION_LIVE_MODE\n", hr );
		LogEvent(buf);
        goto done;
    }

	
    UINT64 timeScale = 10000000; // time scale in HNS means 10000000 HNS units in one second
    hr = SSFMuxSetOption( m_hSSFMux, SSF_MUX_OPTION_TIME_SCALE, &timeScale, sizeof(timeScale) );
    if( FAILED(hr) )
    {
		char buf[1024];
        sprintf_s( buf,"Error 0x%08x: Failed to set SSF_MUX_OPTION_TIME_SCALE\n", hr );
		LogEvent(buf);
        goto done;
    }

	BOOL fVariableBitRate = TRUE; 

    hr = SSFMuxSetOption( m_hSSFMux, SSF_MUX_OPTION_VARIABLE_RATE, &fVariableBitRate, sizeof(fVariableBitRate) ); 
    if( FAILED(hr) ) 
    { 
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to set SSF_MUX_OPTION_VARIABLE_RATE\n", hr ); 
		LogEvent(buf);
        goto done; 
    }
	//////////////////////////////////


	return 0;

	//////////////////////////////////
done:
    if( NULL != m_hSSFMux )
    {
        SSFMuxDestroy( m_hSSFMux );
		m_hSSFMux = NULL;
    }
	return -1;
}
int CStreamMuxerImp1::CreateStreamMuxer(LPCTSTR PublishIp, LPCTSTR PublPoint, BOOL bLive, LPCTSTR DstDir)
{
	m_PublishIp = PublishIp;
	m_PublPoint = PublPoint;
	m_ArchiveFolder = DstDir;
	if(m_ArchiveFolder[m_ArchiveFolder.size()-1] != '\\')
	{
		m_ArchiveFolder += "\\";
	}
	return DoCreateStreamMuxer(bLive);
}

int CStreamMuxerImp1::CloseStream(void)
{
	HRESULT hr = S_OK;
	SSF_BUFFER outputBuffer;
	ZeroMemory(&outputBuffer, sizeof(outputBuffer));


	/////////Update the headers for each fMP4 file///////////////
	///Video
	if(m_pVideoStream)
	{
		m_pVideoStream->FlushOutput();
		hr = SSFMuxGetHeader( m_hSSFMux, m_pVideoStream->GetStreamIndex(), 1, &outputBuffer );
		if( FAILED(hr) )
		{
			char buf[1024];
			sprintf_s(buf, "Error 0x%08x: Failed to get FMP4 header\n", hr );
			LogEvent(buf);
			return FALSE;
		}
		//////////////////////////////////////////////////

		hr = m_pVideoStream->WriteStreamToOutput( outputBuffer.pbBuffer, outputBuffer.cbBuffer, L"r+b");
		 if( FAILED(hr) )
		{
			char buf[1024];
			sprintf_s(buf, "Error 0x%08x: WriteToSmoothStreamOutput\n", hr );
			LogEvent(buf);
			return FALSE;
		}
	}
	if(m_pAudioStream)
	{	 ////Audio
		m_pAudioStream->FlushOutput();
	    hr = SSFMuxGetHeader( m_hSSFMux, m_pAudioStream->GetStreamIndex(), 1, &outputBuffer );
		if( FAILED(hr) )
		{
			char buf[1024];
			sprintf_s(buf, "Error 0x%08x: Failed to get FMP4 header\n", hr );
			LogEvent(buf);
			return FALSE;
		}
		//////////////////////////////////////////////////
		hr = m_pAudioStream->WriteStreamToOutput( outputBuffer.pbBuffer, outputBuffer.cbBuffer, L"r+b");
		 if( FAILED(hr) )
		{
			char buf[1024];
			sprintf_s(buf, "Error 0x%08x: WriteToSmoothStreamOutput\n", hr );
			LogEvent(buf);
			return FALSE;
		}
	}
	///////////////////////
	int retval = 0;
	hr = SSFMuxGetIndex( m_hSSFMux,m_pVideoStream->GetStreamIndex(), 1, &outputBuffer );
    if( FAILED(hr) )
    {
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to get FMP4 index\n", hr );
		LogEvent(buf);
		retval = -1;
    }


	hr = m_pVideoStream->WriteStreamToOutput(
				outputBuffer.pbBuffer,
				outputBuffer.cbBuffer, L"ab");
	if( FAILED(hr) )
    {
		retval = -1;
    }

	if(m_pAudioStream)
	{
		hr = SSFMuxGetIndex( m_hSSFMux,m_pAudioStream->GetStreamIndex(), 1, &outputBuffer );
		if( FAILED(hr) )
		{
			char buf[1024];
			sprintf_s(buf, "Error 0x%08x: Failed to get Audio FMP4 index\n", hr );
			LogEvent(buf);
			retval = -1;
		}


		hr = m_pAudioStream->WriteStreamToOutput(
					outputBuffer.pbBuffer,
					outputBuffer.cbBuffer, L"ab");
		if( FAILED(hr) )
		{
			retval = -1;
		}       
	}
	WriteManifests();
	return retval;
}

int CStreamMuxerImp1::DestroyStreamMuxer(void)
{
	CloseStream();
	if(m_pWriter)
	{
		m_pWriter->CloseWriter();
	}
	if( NULL != m_hSSFMux )
    {
        SSFMuxDestroy( m_hSSFMux );
		m_hSSFMux = NULL;
    }
	return 0;
}

BOOL CStreamMuxerImp1::WriteHeader(CBaseStream* pStream)
{
	HRESULT hr = S_OK;
	SSF_BUFFER outputBuffer;
	hr = SSFMuxGetHeader( m_hSSFMux, pStream->GetStreamIndex(), 1, &outputBuffer );
    if( FAILED(hr) )
    {
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to get FMP4 header\n", hr );
		LogEvent(buf);
        return FALSE;
    }
	//////////////////////////////////////////////////
	hr = pStream->WriteStreamToOutput( outputBuffer.pbBuffer, outputBuffer.cbBuffer, L"wb");
	 if( FAILED(hr) )
    {
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: WriteToSmoothStreamOutput\n", hr );
		LogEvent(buf);
        return FALSE;
    }
	 return TRUE;
}

BOOL CStreamMuxerImp1::AddStream(CBaseStream* pStream)
{
	SSF_STREAM_INFO streamInfo;

	BOOL ret = pStream->Create(streamInfo);
    HRESULT hr = SSFMuxAddStream( m_hSSFMux, &streamInfo, pStream->GetStreamIndex() );
    if( FAILED(hr) )
    {
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to SSFMuxAddStream\n", hr );
		LogEvent(buf);
        
        return FALSE;
    }

	pStream->SetState(ADDED);

	if(!WriteHeader(pStream))
	{
		return FALSE;
	}

	return TRUE;
}

BOOL CStreamMuxerImp1::AddAudioStream(int samplerate, int bitrate, unsigned __int8 *extradata, int extradata_size)
{
	if(m_pAudioStream)
	{
		return FALSE;
	}
	AudioSettings settings;
	settings.samplerate = samplerate;
	settings.bitrate = bitrate;
	settings.extra_data = extradata;
	settings.extradata_size = extradata_size;

	m_pAudioStream = new CAudioStream(this, &settings);
	if(!m_pAudioStream->OpenChannelWriter()) 
	{
		delete m_pAudioStream;
		m_pAudioStream = NULL;
		Sleep(1);
		return FALSE;
	}
	BOOL bRet = AddStream(m_pAudioStream);
	if(!bRet)
	{
		delete m_pAudioStream;
		m_pAudioStream = NULL;
		Sleep(1);
		return FALSE;
	}
	return bRet;
}


int CStreamMuxerImp1::PushStream(BYTE* pData, int len, bool isSyncPoint, LONGLONG ts)
{
	HRESULT hr = S_OK;
	if(!m_hSSFMux)
	{
		Sleep(1);
		return TRUE;
	}
	BOOL ret = m_pVideoStream->AddFrame(pData,len,isSyncPoint,ts);	
	if(m_pVideoStream->GetState() == NEED_TO_BE_ADDED)
	{
		if(!m_pVideoStream->OpenChannelWriter()) 
		{
			return FALSE;
		}
		if(AddStream(m_pVideoStream))
		{
			//Еще разок, в первый был только анализ.
			m_pVideoStream->InitStart();
			ret = m_pVideoStream->AddFrame(pData,len,isSyncPoint,ts);	
		}
	}
	return ret;
}

int CStreamMuxerImp1::PushAudioStream(BYTE* pData, int len, LONGLONG ts)
{
	if(!m_pAudioStream || *(m_pAudioStream->GetStreamIndex()) == DWORD(-1))
	{
		return 0;
	}
	
	HRESULT hr = S_OK;
	if(!m_hSSFMux)
	{
		Sleep(1);
		return 0;
	}

	BOOL ret = m_pAudioStream->AddFrame(pData, len,true, ts);
	if(!ret)
	{
		return -1;
	}

	return 0;
}

HRESULT CStreamMuxerImp1::MuxProcessInput(DWORD dwStreamIndex, SSF_SAMPLE* pInputBuffer)
{
	return  SSFMuxProcessInput( m_hSSFMux, dwStreamIndex, pInputBuffer);
}

HRESULT CStreamMuxerImp1::MuxProcessOutput(DWORD dwStreamIndex,SSF_BUFFER* pOutputBuffer)
{
	return  SSFMuxProcessOutput( m_hSSFMux, dwStreamIndex, pOutputBuffer);
}
HRESULT CStreamMuxerImp1::MuxAdjustDuration(DWORD dwStreamIndex,UINT64 qwTime)
{
	return  SSFMuxAdjustDuration( m_hSSFMux, dwStreamIndex, qwTime);
}

IChannelWriter* CStreamMuxerImp1::GetChannelWriter(LPCTSTR stream_name)
{
	if(m_pWriter)
	{
		return m_pWriter->GetChannelWriter(stream_name);
	}
	return NULL;
}

HRESULT CStreamMuxerImp1::WriteManifestToFile(
    __in LPCSTR pszFileName,
    __in_bcount(cb) LPCVOID pbData,
    __in ULONG cbData )
{
    HRESULT hr = S_OK;
    FILE* pf = NULL;
    errno_t err = 0;

    //EnterCriticalSection(&g_csSerializeWriteToFile);

    err = fopen_s( &pf, pszFileName, "wb");
    if( 0 != err )
    {
		char buf[1024];
        sprintf_s(buf, "CRT errno %d: trying to open manifest file \'%s\'", err, pszFileName);
		LogEvent(buf);
        hr = E_FAIL;
        goto done;
    }

    fwrite( pbData, cbData, 1, pf );

done:

    if( NULL != pf )
    {
        fclose( pf );
    }

    //LeaveCriticalSection(&g_csSerializeWriteToFile);

    return ( hr );
}

HRESULT CStreamMuxerImp1::WriteManifests(void)
{
	if(IsLive())
	{
		return S_OK;
	}
    // ----------------------------------------------
    //
    // Step 6. Generate the Manifests
    //
    // ----------------------------------------------

    //
    // Get the server manifest including all streams
    //
	SSF_BUFFER outputBuffer;

	HRESULT hr = S_OK;
	std::string s_IsmcName = GetArchiveFolder() + GetPubPointName() + std::string("Manifest.ismc");
	std::string Ismc_Relative = GetPubPointName() + std::string("Manifest.ismc");
	USES_CONVERSION;
    hr = SSFMuxGetServerManifest(
                m_hSSFMux,
				T2CW(Ismc_Relative.c_str()),
                &outputBuffer );
    if( FAILED(hr) )
    {
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to get Server manifest\n", hr );
		LogEvent(buf);
        return hr;
    }

	std::string s_IsmName = GetArchiveFolder() + GetPubPointName() + std::string("Manifest.ism");
	hr = WriteManifestToFile( s_IsmName.c_str(), outputBuffer.pbBuffer, outputBuffer.cbBuffer );
    if( FAILED(hr) )
    {
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to write Server manifest to file\n", hr );
		LogEvent(buf);
        return hr;
    }

    //
    // Get the client manifest including all streams
    //

    hr = SSFMuxGetClientManifest( m_hSSFMux, &outputBuffer );
    if( FAILED(hr) )
    {
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to get Client manifest\n", hr );
		LogEvent(buf);
        return hr;
    }


	hr = WriteManifestToFile( s_IsmcName.c_str(), outputBuffer.pbBuffer, outputBuffer.cbBuffer );
    if( FAILED(hr) )
    {
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to write Client manifest to file\n", hr );
		LogEvent(buf);
        
        return hr;
    }
	return hr;
}

BOOL CStreamMuxerImp1::IsVideoStreamAdded()
{
	if(!m_pVideoStream)
	{
		return FALSE;
	}
	return *(m_pVideoStream->GetStreamIndex()) >= 0;
}

BOOL CStreamMuxerImp1::IsAudioStreamAdded()
{
	if(!m_pAudioStream)
	{
		return FALSE;
	}
	return *(m_pAudioStream->GetStreamIndex()) >= 0;
}
