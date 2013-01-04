#pragma once
#include "muxerinterface.h"

class CFileWriter;
class FileChannelWriter :
	public IChannelWriter
{
	CFileWriter* m_pWriter;
	std::string m_stream_name;
	std::string m_file_name;
	CRITICAL_SECTION m_csSerializeWriteToFile;
public:
	FileChannelWriter(CFileWriter* pWriter, LPCTSTR stream_name);
	virtual ~FileChannelWriter(void);
public:
	virtual BOOL Open();
	virtual BOOL Close(void);
	virtual HRESULT WriteStreamToOutput(LPCVOID pbData, ULONG cbData,LPCWSTR pszMode);
	virtual void Delete();
};

