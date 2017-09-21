
// W26ConvertorDlg.cpp : implementation file
//

#include "stdafx.h"
#include "W26Convertor.h"
#include "W26ConvertorDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()


// CW26ConvertorDlg dialog

typedef union
{
	struct
	{
		DWORD Day:5;
		DWORD Month:4;
		DWORD Year:6;

		DWORD hh:5;
		DWORD mm:6;
		DWORD ss:6;
	}wf;
	DWORD code;
}wcode_t;

int what_day(DWORD date1)
{
	wcode_t date;
	date.code = date1;
	int a = (14 - date.wf.Month) / 12;
	int y = 2000 + date.wf.Year - a;
	int m = date.wf.Month + 12 * a - 2;
	return (7000 + (date.wf.Day + y + y / 4 - y / 100 + y / 400 + (31 * m) / 12)) % 7;
}


CW26ConvertorDlg::CW26ConvertorDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CW26ConvertorDlg::IDD, pParent)
	, m_dw_input(52425718)
	, m_output(_T(""))
	, m_facility(_T("95"))
	, m_card(_T("24799"))
{
	wcode_t wcode;
	int i = sizeof(wcode);
	wcode.wf.hh = 23;
	wcode.wf.mm = 59;
	wcode.wf.ss = 59;

	wcode.wf.Day = 17;
	wcode.wf.Month = 11;
	wcode.wf.Year = 10;

	int ddday = what_day(wcode.code);

	CString s;
	s.Format("%d-%d-%d %d:%d:%d\n",wcode.wf.Day,wcode.wf.Month,wcode.wf.Year,wcode.wf.hh = 23,wcode.wf.mm = 59,wcode.wf.ss = 59);
	TRACE0(s);
}

void CW26ConvertorDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT1, m_dw_input);
	DDX_Text(pDX, IDC_EDIT2, m_output);
	DDX_Text(pDX, IDC_EDIT3, m_facility);
	DDX_Text(pDX, IDC_EDIT4, m_card);
}

BEGIN_MESSAGE_MAP(CW26ConvertorDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_BUTTON1, &CW26ConvertorDlg::OnBnClickedButton1)
	ON_BN_CLICKED(IDC_BUTTON2, &CW26ConvertorDlg::OnBnClickedButton2)
	ON_BN_CLICKED(IDC_BUTTON3, &CW26ConvertorDlg::OnBnClickedButton3)
	ON_BN_CLICKED(IDC_BUTTON4, &CW26ConvertorDlg::OnBnClickedButton4)
END_MESSAGE_MAP()


// CW26ConvertorDlg message handlers

BOOL CW26ConvertorDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		BOOL bNameValid;
		CString strAboutMenu;
		bNameValid = strAboutMenu.LoadString(IDS_ABOUTBOX);
		ASSERT(bNameValid);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CW26ConvertorDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CW26ConvertorDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CW26ConvertorDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

//==========================================================================
//Преобразование номера карты и фасилити-кода в формат, поддерживаемый контроллерами TSS (Touch Memory)
CString Wiegand2TouchMemory(CString card_number, CString facility_code)
{
	if( card_number == "" || facility_code == "" )
		return("");

	int fc(0),card(0),l(0),r(0),cl(0),cr(0);
	CString c("");
	union
	{
		struct
		{
			DWORD even_f	: 1;
			DWORD card		: 16;
			DWORD f_code	: 8;
			DWORD even_l	: 1;
		}wf;
		DWORD code;
	}wcode;
	
	fc = atoi(facility_code);
	card = atoi(card_number);
	wcode.code = 0;									
	wcode.wf.card	= card;
	wcode.wf.f_code	= fc;

	r = wcode.code & 0x1fff;
	l = (wcode.code >> 13) & 0x1fff;
	
	while( r > 0 )
	{
		if( r & 1 > 0 )
			cr++;
		r = r >> 1;
	}
	
	while( l > 0 )
	{
		if( l & 1 > 0 )
			cl++;
		l = l >> 1;
	}

	wcode.wf.even_l	= cl % 2 == 0 ? 0 : 1;
	wcode.wf.even_f	= cr % 2 == 0 ? 1 : 0;					
	
	c.Format("%012X",wcode.code);
	return (c);
}
//==========================================================================
//Преобразование номера карты и фасилити-кода в формате Touch Memory в формат Wiegand-26 (номер карты и фасилии-код)
void TouchMemory2Wiegand(CString code, CString& card, CString& facility_code)
{
	card = "";
	facility_code = "";
	if( code == "" )
		return;
	int t(0);
	char *st;
	t = strtol(code,&st,16);
	card.Format("%d",(t >> 1) & 0xFFFF);
	facility_code.Format("%d",(t >> 17) & 0xFF);

}

void CW26ConvertorDlg::OnBnClickedButton1()
{
	UpdateData();
	unsigned long bytes = m_dw_input;
	unsigned long bytes_out = 0;
	//CopyMemory(&bytes,&m_dw_input,sizeof(bytes));
	//int j = 32;
	//for(int i=0;i < 26;i++)
	//{
	//	int bit = bytes & (1 << j); 
	//	if(bit)
	//	{
	//		 bytes_out |= 1 << (i); 
	//	}
	//	j--;
	//}
	bytes_out = m_dw_input;
	CString key;
	key.Format("%.12X",bytes_out);
	m_output = key;

	TouchMemory2Wiegand(m_output,m_card,m_facility);
	UpdateData(FALSE);
}

#define LOBYTE1(w)           ((BYTE)(((WORD)(w)) & 0xff))
#define HIBYTE1(w)           ((BYTE)((((WORD)(w)) >> 8) & 0xff))

DWORD Wiegand2TouchMemoryDword(char* card_number, char* facility_code)
{
	//12501438
	int fc = (0);
	WORD card = (0);
	int l = (0);
	int r = (0);
	int cl = (0);
	int cr = (0);
	union
	{
		struct
		{
			DWORD even_f	: 1;
			DWORD cardL		: 8;
			DWORD cardH		: 8;
			DWORD f_code	: 8;
			DWORD even_l	: 1;
		}wf;
		DWORD code;
	}wcode;
	
	int size_s = sizeof(wcode);
	fc = atoi(facility_code);
	card = (WORD)atoi(card_number);
	wcode.code = 0;									
	wcode.wf.cardL = LOBYTE1(card);
	wcode.wf.cardH = HIBYTE1(card);
	wcode.wf.f_code	= fc;

	r = wcode.code & 0x1fff;
	l = (wcode.code >> 13) & 0x1fff;
	
	while( r > 0 )
	{
		if( r & 1 > 0 )
			cr++;
		r = r >> 1;
	}
	
	while( l > 0 )
	{
		if( l & 1 > 0 )
			cl++;
		l = l >> 1;
	}

	wcode.wf.even_l	= cl % 2 == 0 ? 0 : 1;
	wcode.wf.even_f	= cr % 2 == 0 ? 1 : 0;					

	return wcode.code;
}
int getBit(BYTE data, int pos) 
{
	static unsigned char mask_table[] = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
	int bit_n = ( data & mask_table[ pos ] ) != 0x00;
	return bit_n;
}

DWORD Wiegand2TouchMemoryDword1(char* card_number, char* facility_code)
{
	//12501438
	BYTE fc = (0);
	WORD card = (0);
	int l = (0);
	int r = (0);
	int cl = (0);
	int cr = (0);
	int i = 0;
	int j = 0;
	int even_l = 0;
	int even_f = 0;
	DWORD code = 0;
	
	fc = (BYTE)atoi(facility_code);
	card = (WORD)atoi(card_number);

	TRACE0("bits1:\n");
	int curbit = 1;
	for(i = 0; i < sizeof(card);i++)
	{
		for(j=0;j<8;j++)
		{
			int bit = getBit(*(((BYTE*)&card)+i),j);
			TRACE1("%d",bit);
			if(bit)
			{
				code |=  ((DWORD)1) << curbit;
			}
			curbit++;
		}
	}
	TRACE0("\nbits2:\n");
	for(j=0;j<8;j++)
	{
		int bit = getBit(fc,j);
		TRACE1("%d",bit);
		if(bit)
		{
			code |=  ((DWORD)1) << curbit;
		}
		curbit++;
	}
	TRACE0("\n");
	r = code & 0x1fff;
	l = (code >> 13) & 0x1fff;

	while( r > 0 )
	{
		if( r & 1 > 0 )
			cr++;
		r = r >> 1;
	}

	while( l > 0 )
	{
		if( l & 1 > 0 )
			cl++;
		l = l >> 1;
	}

	even_l	= cl % 2 == 0 ? 0 : 1;
	even_f	= cr % 2 == 0 ? 1 : 0;					
	if(even_l)
	{
		code |=  ((DWORD)1) << 0;
	}
	if(even_f)
	{
		code |=  ((DWORD)1) << curbit;
	}
	return code;
}

void CW26ConvertorDlg::OnBnClickedButton2()
{
	UpdateData();
	m_output = Wiegand2TouchMemory(m_card,m_facility);
	UpdateData(FALSE);
}

void CW26ConvertorDlg::OnBnClickedButton3()
{
	UpdateData();
	TouchMemory2Wiegand(m_output,m_card,m_facility);
	UpdateData(FALSE);
}

void CW26ConvertorDlg::OnBnClickedButton4()
{
	UpdateData();
	m_output = Wiegand2TouchMemory(m_card,m_facility);
	m_dw_input = Wiegand2TouchMemoryDword1((char*)(LPCTSTR)m_card,(char*)(LPCTSTR)m_facility);
	UpdateData(FALSE);
}
