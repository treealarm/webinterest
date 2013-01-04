#pragma once
#include "basestream.h"
class CVideoStream :
	public CBaseStream
{
	BYTE m_profile;
	BYTE m_level;
	BOOL m_bIsFirstSync;
	int m_w;
	int m_h;
public:
	CVideoStream(IMuxerImp* pMuxer);
	virtual ~CVideoStream(void);
	virtual BOOL Create(SSF_STREAM_INFO& streamInfo);
	virtual std::string GetStreamName();
	virtual BOOL AddFrame(BYTE* pData, int len, bool isSyncPoint, LONGLONG ts);
	void CreateContext(SSF_STREAM_INFO& streamInfo);
	HRESULT ProcessFrame(BYTE* pData, int len, bool isSyncPoint, LONGLONG ts);
	void OnStreamInfoFound();
	BYTE* findStartCode(BYTE* begin, BYTE* end, int startCodeLength);
	virtual BOOL FlushOutput();
	void SetWH(int w,int h);
	void InitStart();
};

