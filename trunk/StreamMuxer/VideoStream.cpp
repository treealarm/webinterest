#include "StdAfx.h"
#include "VideoStream.h"

const int startCodeLength = 3;
template <class T> void endswap(T *objp) 
{   
	unsigned char *memp = reinterpret_cast<unsigned char*>(objp);   
	std::reverse(memp, memp + sizeof(T)); 
}

CVideoStream::CVideoStream(IMuxerImp* pMuxer):CBaseStream(pMuxer)
{
	InitStart();
	SetWH(640,480);
}



CVideoStream::~CVideoStream(void)
{
}

void CVideoStream::InitStart()
{
	m_bIsFirstSync = TRUE;
}
BOOL CVideoStream::Create(SSF_STREAM_INFO& streamInfo)
{
    memset( &streamInfo, 0, sizeof(streamInfo) );

    streamInfo.streamType = SSF_STREAM_VIDEO;
    streamInfo.dwBitrate = 96024;
    streamInfo.pszSourceFileName = GetStreamNameW();
    streamInfo.wLanguage = MAKELANGUAGE( 'u', 'n', 'd' );//MAKELANGUAGE( 'e', 'n', 'g' );
	
	CreateContext(streamInfo);
	return TRUE;
}

std::string CVideoStream::GetStreamName()
{
	if(!m_pMuxer->IsLive())
	{
		return m_pMuxer->GetPubPointName() + "Video.ismv";
	}
	return "Video";
}

BOOL CVideoStream::AddFrame(BYTE* pData, int len, bool isSyncPoint, LONGLONG ts)
{
	HRESULT hr = S_OK;
	if(isSyncPoint)
	{
		if(m_bIsFirstSync)
		{
			
			ProcessFrame(pData,len,isSyncPoint, ts);//first frame
			m_bIsFirstSync = FALSE;
			return TRUE;
		}
		else
		{
			ProcessFrame(pData,len,isSyncPoint, ts);//first frame
			FlushOutput();
		}
	}

	if(!m_bIsFirstSync)
	{
		ProcessFrame(pData,len,isSyncPoint, ts);
	}
    if( FAILED(hr) )
    {
        return FALSE;
    }
	
	return TRUE;
}

BOOL CVideoStream::FlushOutput()
{
	HRESULT hr = S_OK;
	GetCtx()->m_rtPrevTS++;
	hr = m_pMuxer->MuxAdjustDuration( GetCtx()->m_dwStreamIndex, (UINT64)GetCtx()->m_rtPrevTS);
	if( FAILED(hr) )
	{
		char buf[1024];
		sprintf_s(buf, "Error 0x%08x: Failed SSFMuxAdjustDuration\n", hr );
		m_pMuxer->LogEvent(buf);
		//goto done;
	}

	SSF_BUFFER outputBuffer;

	hr = m_pMuxer->MuxProcessOutput( GetCtx()->m_dwStreamIndex, &outputBuffer );
	if( SUCCEEDED(hr) && GetCtx()->m_dwStreamIndex >= 0)
	{
		hr = WriteStreamToOutput(
		outputBuffer.pbBuffer,
		outputBuffer.cbBuffer, L"ab");
	}
	return SUCCEEDED(hr);
}

void CVideoStream::SetWH(int w,int h)
{
	m_w = w;
	m_h = h;
}

void CVideoStream::CreateContext(SSF_STREAM_INFO& streamInfo)
{
	MPEG2VIDEOINFO * mpegTemp = NULL; 
	VIDEOINFOHEADER2 vihTemp;
	memset( &vihTemp, 0, sizeof(vihTemp) );

	memset( &vihTemp.bmiHeader, 0, sizeof(vihTemp.bmiHeader) );

	
	//BITMAP_INFOHEADER
	vihTemp.bmiHeader.biSize = sizeof(BITMAPINFOHEADER); // biClrUsed not include
	vihTemp.bmiHeader.biWidth = m_w;//640;
	vihTemp.bmiHeader.biHeight = m_h;//480;
	vihTemp.bmiHeader.biPlanes = 1;
	vihTemp.bmiHeader.biBitCount = 24;
	vihTemp.bmiHeader.biCompression = MAKEFOURCC('A','V','C','1');
	
	
	vihTemp.bmiHeader.biSizeImage = (DWORD) ((vihTemp.bmiHeader.biWidth * vihTemp.bmiHeader.biHeight) );
	vihTemp.bmiHeader.biXPelsPerMeter = 0;
	vihTemp.bmiHeader.biYPelsPerMeter = 0;
	vihTemp.bmiHeader.biClrUsed = 0; // must be 0
	vihTemp.bmiHeader.biClrImportant = 0;

	RECT r = {0,0,vihTemp.bmiHeader.biWidth,vihTemp.bmiHeader.biHeight};
	vihTemp.rcSource = r;
	vihTemp.rcTarget = r;
	vihTemp.dwBitRate = 96024; 
	vihTemp.dwBitErrorRate = 0; 
	// in 100* nanosec : 
	vihTemp.AvgTimePerFrame = 400000;
	vihTemp.dwInterlaceFlags = 0;
	vihTemp.dwCopyProtectFlags = 0;
	vihTemp.dwPictAspectRatioX = 0;
	vihTemp.dwPictAspectRatioY = 0;
	vihTemp.dwControlFlags = 0;
	vihTemp.dwReserved1 = 0;
	vihTemp.dwReserved2 = 0;


	// MPEG2VIDEOINFO
	 
	int sizeSequenceHeader = 0; // TEST !
	mpegTemp = (MPEG2VIDEOINFO *)new BYTE[sizeof(MPEG2VIDEOINFO) + sizeSequenceHeader];
	memset(mpegTemp,0,sizeof(MPEG2VIDEOINFO) + sizeSequenceHeader);
	mpegTemp->hdr = vihTemp; 
	mpegTemp->dwStartTimeCode = 0; // must be 0 
	mpegTemp->cbSequenceHeader = sizeSequenceHeader ;//The length of the dwSequenceHeader array in bytes.
	mpegTemp->dwProfile = m_profile;//Specifies the H.264 profile.
	mpegTemp->dwLevel = m_level; //Specifies the H.264 level.
	mpegTemp->dwFlags = 4 ; //The number of bytes used for the length field that appears before each NALU. The length field indicates the size of the following NALU in bytes. 
	//For example, if dwFlags is 4, each NALU is preceded by a 4-byte length field. The valid values are 1, 2, and 4.
	mpegTemp->dwSequenceHeader[0] = 0;
	
	streamInfo.pTypeSpecificInfo = mpegTemp;
	streamInfo.cbTypeSpecificInfo = sizeof(MPEG2VIDEOINFO) + sizeSequenceHeader;
}

HRESULT CVideoStream::ProcessFrame(BYTE* pData, int len, bool isSyncPoint, LONGLONG ts)
{
	HRESULT hr = S_OK;

	SSF_SAMPLE inputSample = { 0 };

	if(GetCtx()->m_rtStartTime == 0)
	{
		GetCtx()->m_rtStartTime = ts;
	}
	//formula ms to hns units: ((ms/1000)*10E9)/100
    inputSample.qwSampleStartTime = (UINT64)(ts - GetCtx()->m_rtStartTime)* 10000;
	if(GetCtx()->m_rtPrevTS >= (LONGLONG)inputSample.qwSampleStartTime)
	{
		inputSample.qwSampleStartTime = GetCtx()->m_rtPrevTS + 1;
	}
	GetCtx()->m_rtPrevTS = inputSample.qwSampleStartTime;
    inputSample.flags = SSF_SAMPLE_FLAG_START_TIME|SSF_SAMPLE_FLAG_FRAME_TYPE;

    if( isSyncPoint)
    {
        inputSample.FrameType = FRAMETYPE_I;
    }

	BYTE* fTo = new BYTE[len+8];
	BYTE* dataStart = pData+startCodeLength+1;
	BYTE* dataEnd = pData + len;

	int written = 0;
	while(dataStart < dataEnd)
	{
		BYTE* naluEnd = findStartCode(dataStart, dataEnd, startCodeLength);
		INT32 nalusize = static_cast<INT32>(naluEnd - dataStart);
		INT32 nalusize_net = nalusize;
		endswap(&nalusize_net); 
		 memcpy(fTo + written, &nalusize_net, sizeof(nalusize_net));
		 written+=sizeof(nalusize);

		 memcpy(fTo + written, dataStart, nalusize);
		 BYTE nal_unit_type = (fTo + written)[0]&0x1F;
		 BYTE IsIFrame = 0x7C == *(fTo + written);
		 if(IsIFrame)
		 {
			 OutputDebugString("IFrame\n");
		 }
		 if(nal_unit_type == 7)
		 {
			 BYTE* sps = (fTo + written);
			 m_profile = sps[1]<<16;
			 m_level = sps[2]<<8;
			 BYTE id = sps[3];
			 char buf[100];
			 sprintf_s(buf,100,"SPS-7 profile=%d,level=%d,id=%d\n",(int)m_profile,(int)m_level,(int)id);
			 OutputDebugString(buf);
			 OnStreamInfoFound();
		 }
		 if(nal_unit_type == 8)
		 {
			 OutputDebugString("PPS-8\n");
		 }
		 written+=nalusize;
		 dataStart = dataStart+((naluEnd - dataStart) + startCodeLength + 1);
	}
	inputSample.pSampleData = fTo;
	inputSample.cbSampleData = written;
	if(GetState() == ADDED)
	{
		hr = m_pMuxer->MuxProcessInput(GetCtx()->m_dwStreamIndex, &inputSample );
	}
	delete fTo;
	return hr;
}

void CVideoStream::OnStreamInfoFound()
{
	if(GetState() == UNDEFINED)
	{
		SetState(NEED_TO_BE_ADDED);
	}
}

BYTE* CVideoStream::findStartCode(BYTE* begin, BYTE* end, int startCodeLength)
{
	if ((end - begin) < (startCodeLength + 1))
		 return end;

	int zerosCount = 0;

	// Find <startCodeLength> successive zeros.
	while (begin < end)
	{
		// Not a null, reset counter.
		if (*(begin++))
		{
			zerosCount = 0;
			continue;
		}

		// Probably got a start code.
		if (++zerosCount == startCodeLength)
		{
			// Check if we've approached end of buffer.
			if (begin == end)
				return begin;

			// Check bytes following zeros.

			// Ok, we found start code, return end of previous NALU.
			if (*begin == 1)
				return begin - startCodeLength;

			// Otherwise, pop front zero (since there may be more than startCodeLength
			// zeros. The byte will be checked in next iteration.
			--zerosCount;
			++begin;
		}
	}
	return begin;
}

