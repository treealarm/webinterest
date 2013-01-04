#pragma once

#include "BaseStream.h"
#include "MuxerInterface.h"
class CStreamMuxer;
class CBaseWriter;
class CVideoStream;

class CStreamMuxerImp1: public IMuxerImp
{
	CStreamMuxer* m_pParent;
	
	CBaseStream* m_pAudioStream;
	CVideoStream* m_pVideoStream;

	SSFMUXHANDLE m_hSSFMux;

	std::string m_PublishIp;
	std::string m_PublPoint;
	std::string m_ArchiveFolder;
	BOOL m_bLive;
	virtual std::string GetPubPointName()
	{
		return m_PublPoint.c_str();
	};
	virtual std::string GetPubPointIp()
	{
		return m_PublishIp.c_str();
	};
	virtual BOOL IsLive()
	{
		return m_bLive;
	}
	virtual std::string GetArchiveFolder()
	{
		return m_ArchiveFolder.c_str();
	};
	virtual void LogEvent(LPCTSTR message);
	CBaseWriter* m_pWriter;
	HRESULT WriteManifestToFile(
		__in LPCSTR pszFileName,
		__in_bcount(cb) LPCVOID pbData,
		__in ULONG cbData );
	VF_STRUCT m_VF_STRUCT;
public:
	void SetWH(VF_STRUCT& vf);
	CBaseWriter* GetWritter();
	CStreamMuxerImp1(CStreamMuxer* pParent);
	virtual ~CStreamMuxerImp1(void);
	int CreateStreamMuxer(LPCTSTR PublishIp, LPCTSTR PublPoint, BOOL bLive, LPCTSTR DstDir);
	int DoCreateStreamMuxer(BOOL fLive);
	int DestroyStreamMuxer(void);
	int PushStream(BYTE* pData, int len, bool isSyncPoint, LONGLONG ts);

	int CloseStream(void);
	BOOL AddAudioStream(int samplerate, int bitrate, unsigned __int8 *extradata, int extradata_size);
	int PushAudioStream(BYTE* pData, int len, LONGLONG ts);
	BOOL AddStream(CBaseStream* pStream);
	BOOL WriteHeader(CBaseStream* pStream);

	virtual HRESULT MuxProcessInput(DWORD dwStreamIndex, SSF_SAMPLE* pInputBuffer);
	virtual HRESULT MuxProcessOutput(DWORD dwStreamIndex,SSF_BUFFER* pOutputBuffer);
	virtual HRESULT MuxAdjustDuration(DWORD dwStreamIndex, UINT64 qwTime);

	virtual IChannelWriter* GetChannelWriter(LPCTSTR stream_name);
	HRESULT WriteManifests(void);
	BOOL IsVideoStreamAdded();
	BOOL IsAudioStreamAdded();
};

