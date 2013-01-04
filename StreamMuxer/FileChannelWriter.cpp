#include "StdAfx.h"
#include "FileChannelWriter.h"
#include "FileWriter.h"


FileChannelWriter::FileChannelWriter(CFileWriter* pWriter,
				LPCTSTR stream_name):m_pWriter(pWriter)
{
	m_stream_name = stream_name;
	InitializeCriticalSection( &m_csSerializeWriteToFile );
}


FileChannelWriter::~FileChannelWriter(void)
{
	DeleteCriticalSection( &m_csSerializeWriteToFile );
}

void FileChannelWriter::Delete()
{
	delete this;
}

BOOL FileChannelWriter::Open()
{
	m_file_name = m_pWriter->m_pMuxer->GetArchiveFolder() + m_stream_name;
	
	DWORD dwAttrs = GetFileAttributes(m_file_name.c_str()); 
	if (dwAttrs!=INVALID_FILE_ATTRIBUTES)
	{
		return FALSE;
	}
	
	return TRUE;
}


BOOL FileChannelWriter::Close(void)
{
	return TRUE;
}

HRESULT FileChannelWriter::WriteStreamToOutput(LPCVOID pbData, ULONG cbData, LPCWSTR pszMode)
{
    HRESULT hr = S_OK;
    FILE* pf = NULL;
    errno_t err;

    //
    // Make the file name and write the buffer
    //
    // the error checking is lax in this section
    //

    EnterCriticalSection(&m_csSerializeWriteToFile);

	USES_CONVERSION;
	err = _wfopen_s( &pf, T2CW( m_file_name.c_str()), pszMode );
    
    if( 0 != err )
    {
		char buf[1024];
		sprintf_s(buf, "CRT errno %d: trying to open file \'%s\'\n", err, m_file_name.c_str() );
		m_pWriter->m_pMuxer->LogEvent(buf);
        hr = E_FAIL;
        goto done;
    }

    if( 1 != fwrite( pbData, cbData, 1, pf ) )
    {
        char buf[1024];
		sprintf_s(buf, "CRT errno %d: trying to write to file \'%s\'\n", err, m_file_name.c_str() );
		m_pWriter->m_pMuxer->LogEvent(buf);
        hr = E_FAIL;
        goto done;
    }

done:

    if( NULL != pf )
    {
        fclose( pf );
    }

    LeaveCriticalSection(&m_csSerializeWriteToFile);

    return ( hr );
}
