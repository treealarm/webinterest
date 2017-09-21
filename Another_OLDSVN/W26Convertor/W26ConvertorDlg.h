
// W26ConvertorDlg.h : header file
//

#pragma once


// CW26ConvertorDlg dialog
class CW26ConvertorDlg : public CDialog
{
// Construction
public:
	CW26ConvertorDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_W26CONVERTOR_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	DWORD m_dw_input;
	CString m_output;
	afx_msg void OnBnClickedButton1();
	CString m_facility;
	CString m_card;
	afx_msg void OnBnClickedButton2();
	afx_msg void OnBnClickedButton3();
	afx_msg void OnBnClickedButton4();
};
