#pragma once
#include "muxerinterface.h"

class CHttpWriter;
class HttpChannelWriter :
	public IChannelWriter
{
	CHttpWriter* m_pWriter;
	std::string m_stream_name;
	HINTERNET m_hHttpRequest;

	HRESULT WriteToSmoothStreamOutput(
				__in HINTERNET hHttpRequest,
				__in_ecount(cbData) LPCVOID pbData,
				__in ULONG cbData );
public:
	HttpChannelWriter(CHttpWriter* pWritter, LPCTSTR stream_name);
	virtual ~HttpChannelWriter(void);
public:
	virtual BOOL Open();
	virtual BOOL Close(void);
	virtual HRESULT WriteStreamToOutput(LPCVOID pbData, ULONG cbData, LPCWSTR pszMode);
	virtual void Delete();
};