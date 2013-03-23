#pragma once


// CDlgDraw dialog

#include "DrawListCtrl.h"

class CThreadDrawGenerateXY;
class ControlWrapper;
class CDlgDraw : public CDialog
{
	DECLARE_DYNAMIC(CDlgDraw)

public:
	enum COORD_POS
	{
		X_POS = 1,
		Y_POS = 0,
		Z_POS = 2
	};
	CDlgDraw(CWnd* pParent = NULL);   // standard constructor
	virtual ~CDlgDraw();

// Dialog Data
	enum { IDD = IDD_DIALOG_DRAW };
	xyz_coord m_cur_pos;
	void GoToStartPosition();
	void GoToStartPositionX();
	void GoToStartPositionY();
	void GoToStartPositionZ();
	void ShiftX(int x);
	ControlWrapper* m_pControlWrapper;
protected:
	CDrawListCtrl m_list;
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
public:
	
	do_control_signals* m_p_do_control_signals;
	virtual BOOL OnInitDialog();
	afx_msg void OnBnClickedButtonInit();
	afx_msg void OnBnClickedButtonXPlus();
	afx_msg void OnBnClickedButtonXMinus();
	afx_msg void OnBnClickedButtonYPlus();
	afx_msg void OnBnClickedButtonYMinus();
	afx_msg void OnBnClickedButtonZPlus();
	afx_msg void OnBnClickedButtonZMinus();
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnDestroy();
	afx_msg void OnBnClickedCheckEnable();
	afx_msg LRESULT OnProcessXY(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnEndProcessXY(WPARAM wParam, LPARAM lParam);
	

	void GetEnableFromCheck();
	
	afx_msg void OnBnClickedButtonDrawSelected();
	void SetStepsToController(do_steps& steps);
	void GoToXY(int x, int y);
	afx_msg void OnBnClickedButtonGotoXy();
	void GoToZ(int z);
	void GoToX(int x);
	afx_msg void OnBnClickedButtonGotoZ();
	int m_commands_done;
	void DriversOff(void);
	int m_nPointsDone;
	CThreadDrawGenerateXY* m_pThreadDrawGenerateXY;
	void DeleteXYThread();
	void CreateXYThread(COLORREF selcolor);
	int m_selection;
	BOOL IsPaused();
	void Pause();
	UINT m_timer_res;
	BYTE m_mult[MOTORS_COUNT];
	int m_step_mult[MOTORS_COUNT];
	afx_msg void OnBnClickedButtonSaveProrej();
	afx_msg void OnBnClickedButton2();
	int DoProrejka2(COLORREF selcolor,Bitmap& Image1,BOOL divider1);
	afx_msg void OnBnClickedButton3();
	UINT m_nInkImpuls;
	afx_msg void OnBnClickedButton4();
	BOOL m_bDriversOffPending;
	BOOL m_OptimizePath;
	int m_StartY;
};
