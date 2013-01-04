// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the STREAMMUXER_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// STREAMMUXER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef STREAMMUXER_EXPORTS
#define STREAMMUXER_API __declspec(dllexport)
#else
#define STREAMMUXER_API __declspec(dllimport)
#endif

class CStreamMuxerImp1;
// This class is exported from the StreamMuxer.dll
#include "SMInterfaces.h"

struct VF_STRUCT
{
	BYTE* pData;
	int len;
	bool isSyncPoint;
	LONGLONG ts;
	int w;
	int h;
};

class STREAMMUXER_API CStreamMuxer 
{
	CStreamMuxerImp1* m_pMuxer;
	IMuxerLogger* m_pLogger;
public:
	CStreamMuxer(IMuxerLogger* pLogger);
	IMuxerLogger* GetLogger();
	int CreateStreamMuxer(LPCTSTR PublishIp, LPCTSTR PublPoint, BOOL bLive, LPCTSTR DstDir);
	int DestroyStreamMuxer(void);
	int PushVideoStream(VF_STRUCT& vf);
	BOOL AddAudioStream(int samplerate, int bitrate, unsigned __int8 *extradata, int extradata_size);
	int PushAudioStream(BYTE* pData, int len, LONGLONG ts);
	BOOL IsVideoStreamAdded();
	BOOL IsAudioStreamAdded();
};

namespace PPTCreator
{
STREAMMUXER_API BOOL CreatePPT(LPCTSTR ip, WORD port, LPCTSTR ppt_name);
}

