#pragma once

interface IChannelWriter
{
	virtual BOOL Open() = 0;
	virtual BOOL Close(void) = 0;
	virtual HRESULT WriteStreamToOutput(LPCVOID pbData, ULONG cbData, LPCWSTR pszMode) = 0;
	virtual void Delete() = 0;
};

interface IMuxerImp
{
	virtual std::string GetPubPointName() = 0;
	virtual std::string GetPubPointIp() = 0;
	virtual std::string GetArchiveFolder() = 0;

	virtual void LogEvent(LPCTSTR message) = 0;
	virtual HRESULT MuxProcessInput(DWORD dwStreamIndex, SSF_SAMPLE* pInputBuffer) = 0;
	virtual HRESULT MuxProcessOutput(DWORD dwStreamIndex,SSF_BUFFER* pOutputBuffer) = 0;
	virtual HRESULT MuxAdjustDuration(DWORD dwStreamIndex,UINT64 qwTime) = 0;
	virtual IChannelWriter* GetChannelWriter(LPCTSTR stream_name) = 0;
	virtual BOOL IsLive() = 0;
};