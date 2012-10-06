// DlgSize.cpp : implementation file
//

#include "stdafx.h"
#include "MStudio.h"
#include "DlgSize.h"


// CDlgSize dialog

IMPLEMENT_DYNAMIC(CDlgSize, CDialog)

CDlgSize::CDlgSize(CWnd* pParent /*=NULL*/)
	: CDialog(CDlgSize::IDD, pParent)
	, m_w(0)
	, m_h(0)
{

}

CDlgSize::~CDlgSize()
{
}

void CDlgSize::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT1, m_w);
	DDX_Text(pDX, IDC_EDIT7, m_h);
}


BEGIN_MESSAGE_MAP(CDlgSize, CDialog)
END_MESSAGE_MAP()


// CDlgSize message handlers
