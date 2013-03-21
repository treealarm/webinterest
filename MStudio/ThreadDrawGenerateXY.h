#pragma once



// CThreadDrawGenerateXY

class CThreadDrawGenerateXY : public CWinThread
{
	DECLARE_DYNCREATE(CThreadDrawGenerateXY)

protected:
	BOOL m_OptimizePath;
	CThreadDrawGenerateXY(){};           
public:
	CThreadDrawGenerateXY(BOOL optimizePath);           
	virtual ~CThreadDrawGenerateXY();


	virtual BOOL InitInstance();
	virtual int ExitInstance();
	HWND m_hWnd;
	int m_w;
	int m_h;
	COLORREF m_selcolor;
	void GoAhead();
	CList<CPoint,CPoint&> m_arr_points ;
	void AddPoint(int x,int y)
	{
		m_arr_points.AddTail(CPoint(x,y));
	}
protected:
	CPoint m_cur_point;
	CEvent m_EventNext;
	DECLARE_MESSAGE_MAP()
public:
	BOOL m_bWorking;
	void Stop(void);
};
static UINT WM_PROCESS_XY = RegisterWindowMessage("WM_PROCESS_CDlgDraw");
static UINT WM_END_PROCESS_XY = RegisterWindowMessage("WM_WM_END_PROCESS_XY_CDlgDraw");
