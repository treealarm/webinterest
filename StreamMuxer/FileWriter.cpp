#include "StdAfx.h"
#include "FileWriter.h"
#include "FileChannelWriter.h"

CFileWriter::CFileWriter(IMuxerImp* pMuxer):CBaseWriter(pMuxer)
{
}


CFileWriter::~CFileWriter(void)
{
}

BOOL CFileWriter::OpenWriter()
{
	return TRUE;
}
BOOL CFileWriter::CloseWriter()
{
	return TRUE;
}

IChannelWriter* CFileWriter::CreateChannelWriter(LPCTSTR stream_name)
{
	return new FileChannelWriter(this,stream_name);
}