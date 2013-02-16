// ThreadDrawGenerateXY.cpp : implementation file
//

#include "stdafx.h"
#include "MStudio.h"
#include "ThreadDrawGenerateXY.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif
// CThreadDrawGenerateXY



IMPLEMENT_DYNCREATE(CThreadDrawGenerateXY, CWinThread)

CThreadDrawGenerateXY::CThreadDrawGenerateXY()
: m_bWorking(TRUE)
{
	m_bAutoDelete = FALSE;
	m_hWnd = NULL;
	m_w = 0;
	m_h = 0;
}

CThreadDrawGenerateXY::~CThreadDrawGenerateXY()
{
}

void CThreadDrawGenerateXY::GoAhead()
{
	m_EventNext.SetEvent();
}

double GetDistanse(CPoint pt1,CPoint pt2)
{
	int xx = pt1.x-pt2.x;
	int yy = pt1.y-pt2.y;
	double dist = sqrt((double)(xx*xx+yy*yy));
	return dist;
}
BOOL CThreadDrawGenerateXY::InitInstance()
{
	//for(POSITION pos = m_arr_points.GetHeadPosition();pos!=NULL;)
	//{
	//	CPoint cur_pt = m_arr_points.GetNext(pos);
	//	CPoint* pPoint = new CPoint(cur_pt);
	//	PostMessage(m_hWnd,WM_PROCESS_XY,(WPARAM)pPoint,0);
	//	WaitForSingleObject(m_EventNext,INFINITE);
	//}

	m_cur_point = CPoint(0,0);
	while(!m_arr_points.IsEmpty() && m_bWorking)
	{
		double cur_dist = (double)(m_h+m_w);
		POSITION pos_to_send = m_arr_points.GetHeadPosition();
		for(POSITION pos = m_arr_points.GetHeadPosition();pos!=NULL;)
		{
			POSITION prev_pos = pos;
			CPoint cur_pt = m_arr_points.GetNext(pos);
			double dist = GetDistanse(m_cur_point,cur_pt);
			if(dist < cur_dist)
			{
				cur_dist = dist ;
				pos_to_send = prev_pos;
			}
		}
		if(pos_to_send)
		{
			m_cur_point = m_arr_points.GetAt(pos_to_send);
			CPoint* pPoint = new CPoint(m_cur_point);
			m_arr_points.RemoveAt(pos_to_send);
			PostMessage(m_hWnd,WM_PROCESS_XY,(WPARAM)pPoint,0);
		}
		WaitForSingleObject(m_EventNext,INFINITE);
	}
	/*int g_chet = 0;
	for(int y = 0; y < m_h;y++)
	{
		if(y%2)
		{
			for(int x = 0; x < m_w;x++)
			{
				WaitForSingleObject(m_EventNext,INFINITE);
				CPoint* pPoint = new CPoint(x,y);
				PostMessage(m_hWnd,WM_PROCESS_XY,(WPARAM)pPoint,0);
			}
		}
		else
		{
			for(int x = m_w-1; x >=0;x--)
			{
				WaitForSingleObject(m_EventNext,INFINITE);
				CPoint* pPoint = new CPoint(x,y);
				PostMessage(m_hWnd,WM_PROCESS_XY,(WPARAM)pPoint,0);
			}
		}

	}*/
	PostMessage(m_hWnd,WM_END_PROCESS_XY,0,0);
	
	return FALSE;
}

int CThreadDrawGenerateXY::ExitInstance()
{
	// TODO:  perform any per-thread cleanup here
	return CWinThread::ExitInstance();
}

BEGIN_MESSAGE_MAP(CThreadDrawGenerateXY, CWinThread)
END_MESSAGE_MAP()


// CThreadDrawGenerateXY message handlers

void CThreadDrawGenerateXY::Stop(void)
{
	m_bWorking = FALSE;
	GoAhead();
	WaitForSingleObject(m_hThread,INFINITE);
}
