#pragma once

#include "MuxerInterface.h"

class StreamContext
{
public:
	StreamContext()
	{
		m_dwStreamIndex = -1;
		m_rtStartTime = 0;
		m_rtPrevTS = 0;
	}
	DWORD m_dwStreamIndex;
	LONGLONG m_rtStartTime;
	LONGLONG m_rtPrevTS;
};

enum E_StreamState
{
	UNDEFINED,
	NEED_TO_BE_ADDED,
	ADDED
};

class CBaseStream
{
	StreamContext m_Ctx;
	WCHAR   szStreamName[MAX_PATH];
	E_StreamState m_eMuxState;
	IChannelWriter* m_pChannelWriter;
public:
	LONGLONG m_last_write;
	IMuxerImp* m_pMuxer;
	CBaseStream(IMuxerImp* pMuxer);
	virtual ~CBaseStream();
	virtual BOOL Create(SSF_STREAM_INFO& streamInfo) = 0;
	DWORD* GetStreamIndex();
	virtual BOOL OpenChannelWriter();
	virtual std::string GetStreamName() = 0;
	LPCWSTR GetStreamNameW();
	StreamContext* GetCtx();
	HRESULT WriteStreamToOutput(LPCVOID pbData, ULONG cbData, LPCWSTR pszMode);
	HRESULT WriteToSmoothStreamOutput(
				__in HINTERNET hHttpRequest,
				__in_ecount(cbData) LPCVOID pbData,
				__in ULONG cbData );
	virtual BOOL AddFrame(BYTE* pData, int len, bool isSyncPoint, LONGLONG ts) = 0;
	virtual E_StreamState GetState();
	virtual void SetState(E_StreamState state);
	virtual BOOL FlushOutput() = 0;
};

