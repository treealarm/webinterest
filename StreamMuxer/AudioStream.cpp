#include "StdAfx.h"
#include "AudioStream.h"


CAudioStream::CAudioStream(IMuxerImp* pMuxer,
	AudioSettings* pSettings):CBaseStream(pMuxer)
{
	m_extradata_size = pSettings->extradata_size;
	m_extradata = new unsigned __int8[m_extradata_size];
	CopyMemory(m_extradata, pSettings->extra_data, m_extradata_size);
	m_samplerate = pSettings->samplerate;
	m_bitrate = pSettings->bitrate;
}


CAudioStream::~CAudioStream(void)
{
	delete m_extradata;
}

std::string CAudioStream::GetStreamName()
{
	if(!m_pMuxer->IsLive())
	{
		return m_pMuxer->GetPubPointName()+"Audio.isma";
	}
	return "Audio";
}

BOOL CAudioStream::Create(SSF_STREAM_INFO& streamInfo)
{
	HRESULT hr = S_OK;

    memset( &streamInfo, 0, sizeof(streamInfo) );

    streamInfo.streamType = SSF_STREAM_AUDIO;
    streamInfo.dwBitrate = m_bitrate;
	//USES_CONVERSION;
	streamInfo.pszSourceFileName = GetStreamNameW();//T2CW(GetStreamName());//"Audio";
    streamInfo.wLanguage = MAKELANGUAGE( 'u', 'n', 'd' );//MAKELANGUAGE( 'e', 'n', 'g' );
	
	CreateAudioContext(streamInfo, m_samplerate, m_extradata, m_extradata_size);
	return TRUE;
}

void CAudioStream::CreateAudioContext(SSF_STREAM_INFO& streamInfo, int samplerate, unsigned __int8 *extradata, int extradata_size)
{
	//http://wiki.multimedia.cx/index.php?title=MPEG-4_Audio

	int cbData = sizeof(WAVEFORMATEX) + extradata_size;
	BYTE* pBuf = new BYTE[cbData];
	WAVEFORMATEX* wavFmt = (WAVEFORMATEX*)pBuf;
	ZeroMemory(pBuf, cbData);
	wavFmt->wFormatTag = WAVE_FORMAT_RAW_AAC1;//0x00FF; // WAVE_FORMAT_RAW_AAC1 defined in Windows 7 SDK MMReg.h
	wavFmt->nChannels = 1;
	wavFmt->nSamplesPerSec = samplerate;
	wavFmt->wBitsPerSample = 16; // Some compression schemes cannot define a value for wBitsPerSample, so this member can be zero.
	wavFmt->nAvgBytesPerSec = samplerate * wavFmt->nChannels * 16 / 8;
	wavFmt->nBlockAlign = wavFmt->nChannels * wavFmt->wBitsPerSample / 8; 

	wavFmt->cbSize = extradata_size; // or 0 ?

	char* data = (char*) pBuf + sizeof(WAVEFORMATEX);
	CopyMemory(data, extradata, extradata_size);

	streamInfo.pTypeSpecificInfo = wavFmt;
	streamInfo.cbTypeSpecificInfo = cbData;
}


BOOL CAudioStream::AddFrame(BYTE* pData, int len, bool isSyncPoint, LONGLONG ts)
{
	HRESULT hr = S_OK;

	SSF_SAMPLE inputSample = { 0 };

	if(GetCtx()->m_rtStartTime == 0)
	{
		GetCtx()->m_rtStartTime = ts;
	}
	//formula ms to hns units: ((ms/1000)*10E9)/100
    inputSample.qwSampleStartTime = (UINT64)(ts - GetCtx()->m_rtStartTime) * 10000;
	if(GetCtx()->m_rtPrevTS >= (LONGLONG)inputSample.qwSampleStartTime)
	{
		inputSample.qwSampleStartTime = GetCtx()->m_rtPrevTS + 1;
	}

	if(m_last_write == 0)
	{
		m_last_write = inputSample.qwSampleStartTime;
	}
	GetCtx()->m_rtPrevTS = inputSample.qwSampleStartTime;
    inputSample.flags = SSF_SAMPLE_FLAG_START_TIME|SSF_SAMPLE_FLAG_FRAME_TYPE;


   inputSample.FrameType = FRAMETYPE_UNKNOWN;

	inputSample.pSampleData = pData;
	inputSample.cbSampleData = len;
	hr = m_pMuxer->MuxProcessInput( GetCtx()->m_dwStreamIndex, &inputSample );
	if(FAILED(hr))
	{
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to SSFMuxProcessInput\n", hr );
		m_pMuxer->LogEvent(buf);
		return FALSE;
	}
	
/////ProcessOutput

	if((inputSample.qwSampleStartTime - m_last_write) < 5000 * 10000)
	{
		return TRUE;
	}
	m_last_write = inputSample.qwSampleStartTime;
	return FlushOutput();
}

BOOL CAudioStream::FlushOutput()
{
	HRESULT hr = S_OK;
	
	GetCtx()->m_rtPrevTS++;

	if(!m_pMuxer->IsLive())
	{
		hr = m_pMuxer->MuxAdjustDuration( GetCtx()->m_dwStreamIndex, (UINT64)GetCtx()->m_rtPrevTS);
		if( FAILED(hr) )
		{
			char buf[1024];
			sprintf_s(buf, "Error 0x%08x: Failed SSFMuxAdjustDuration\n", hr );
			m_pMuxer->LogEvent(buf);
		}
	}
	SSF_BUFFER outputBuffer;

	hr = m_pMuxer->MuxProcessOutput( GetCtx()->m_dwStreamIndex, &outputBuffer );
	if( SUCCEEDED(hr))
	{
		hr = WriteStreamToOutput(
				outputBuffer.pbBuffer,
				outputBuffer.cbBuffer, L"ab");
	}


	if( FAILED(hr) )
	{
		char buf[1024];
        sprintf_s(buf, "Error 0x%08x: Failed to WriteStreamToOutput\n", hr );
		m_pMuxer->LogEvent(buf);
		return FALSE;
	}
	return TRUE;
}