#pragma once
#include "basestream.h"

struct AudioSettings
{
	unsigned __int8* extra_data;
	int extradata_size;
	int samplerate;
	int bitrate;
};

class CAudioStream :
	public CBaseStream
{
	unsigned __int8* m_extradata;
	int m_extradata_size;
	int m_samplerate;
	int m_bitrate;
public:
	CAudioStream(IMuxerImp* pMuxer, AudioSettings* pSettings);
	virtual ~CAudioStream(void);
	virtual BOOL Create(SSF_STREAM_INFO& streamInfo);
	virtual std::string GetStreamName();
	void CreateAudioContext(SSF_STREAM_INFO& streamInfo, int samplerate, unsigned __int8 *extradata, int extradata_size);
	virtual BOOL AddFrame(BYTE* pData, int len, bool isSyncPoint, LONGLONG ts);
	virtual BOOL FlushOutput();
};

