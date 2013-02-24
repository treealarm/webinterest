// DlgDraw.cpp : implementation file
//

#include "stdafx.h"
#include "MStudio.h"
#include "DlgDraw.h"
#include "MStudioDoc.h"
#include "ThreadDrawGenerateXY.h"
#include "MStudioView.h"
#include "DlgSize.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif
// CDlgDraw dialog

extern int GetEncoderClsid(const WCHAR* format, CLSID* pClsid);

IMPLEMENT_DYNAMIC(CDlgDraw, CDialog)

CDlgDraw::CDlgDraw(CWnd* pParent /*=NULL*/)
	: CDialog(CDlgDraw::IDD, pParent)
	, m_commands_done(0)
	, m_timer_res(100)
	, m_pPrevPoint(0)
	, m_nInkImpuls(0)
	, m_bDriversOffPending(FALSE)
{
	m_selection = -1;
	m_pThreadDrawGenerateXY = NULL;
	ZeroMemory(&m_cur_pos,sizeof(m_cur_pos));
	m_p_do_control_signals = new do_control_signals;
	m_p_do_control_signals->ms1 = 1;
	m_p_do_control_signals->ms2 = 1;
	m_p_do_control_signals->reset = 1;

	m_pControlWrapper = new ControlWrapper;
	m_timer_res = AfxGetApp()->GetProfileInt("settings","m_timer_res",100);

	int i=0;
	for(i=0;i<MOTORS_COUNT;i++)
	{
		CString s;
		s.Format("m_mult%d",i);
		m_mult[i] = AfxGetApp()->GetProfileInt("settings",s,1);

		s.Format("m_step_mult%d",i);
		m_step_mult[i] = AfxGetApp()->GetProfileInt("settings",s,1);
	}
	//i=0;
	//for(int j=0; j < 32*400;j++)
	//{
	//	if(j%32 == 0)
	//	{
	//		i++;
	//	}
	//}
	m_nInkImpuls = AfxGetApp()->GetProfileInt("settings","m_nInkImpuls",1);
	m_nPointsDone = 0;
}

CDlgDraw::~CDlgDraw()
{
	DeleteXYThread();
	delete m_p_do_control_signals;
	delete m_pControlWrapper;
}


void CDlgDraw::CreateXYThread(COLORREF selcolor)
{
	DeleteXYThread();

	int selection = m_list.GetSelectionMark();
	if(selection<0)
	{
		AfxMessageBox("Nothing is selected");
		return;
	}

	Bitmap* pImage = g_pDoc->m_pImage;
	int w = pImage->GetWidth();
	int h = pImage->GetHeight();


	m_pThreadDrawGenerateXY = new CThreadDrawGenerateXY();
	m_pThreadDrawGenerateXY->m_h = h;
	m_pThreadDrawGenerateXY->m_w = w;
	m_pThreadDrawGenerateXY->m_selcolor = selcolor;
	m_pThreadDrawGenerateXY->m_hWnd = GetSafeHwnd();

	m_pThreadDrawGenerateXY->m_arr_points.RemoveAll();
	
	for(int x = 0; x < w;x++)
	{
		for(int y = 0; y < h;y++)	
		{
			Color color;
			Status status = pImage->GetPixel(x,h-y-1,&color);
			if(selcolor==color.ToCOLORREF()/* 
										   || (x==0 && (y%5)==0 ) || (x==(w-1) && (y%5)==0)*/)
			{
				m_pThreadDrawGenerateXY->AddPoint(x,y);
			}
		}
	}
	/*m_pThreadDrawGenerateXY->AddPoint(0,0);
	m_pThreadDrawGenerateXY->AddPoint(w-1,0);
	m_pThreadDrawGenerateXY->AddPoint(w-1,h-1);
	m_pThreadDrawGenerateXY->AddPoint(0,h-1);*/
	m_pThreadDrawGenerateXY->CreateThread();
	if(!IsPaused())
	{
		m_pThreadDrawGenerateXY->GoAhead();
	}
	
};
void CDlgDraw::DeleteXYThread()
{
	if(m_pThreadDrawGenerateXY)
	{
		m_pThreadDrawGenerateXY->m_h = 0;
		m_pThreadDrawGenerateXY->m_w = 0;
		m_pThreadDrawGenerateXY->m_hWnd = NULL;
		m_pThreadDrawGenerateXY->Stop();
		delete m_pThreadDrawGenerateXY;
	}
};

void CDlgDraw::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST1, m_list);
	DDX_Text(pDX, IDC_EDIT_TIMER_RES, m_timer_res);
	DDV_MinMaxUInt(pDX, m_timer_res, 1, 65000);
	for(int i=0;i<MOTORS_COUNT;i++)
	{
		DDX_Text(pDX, IDC_EDIT_TMR_MULT0+i, m_mult[i]);
		DDV_MinMaxByte(pDX, m_mult[i], 0, 255);

		DDX_Text(pDX, IDC_EDIT_STEP_MULT0+i, m_step_mult[i]);
		DDV_MinMaxInt(pDX, m_step_mult[i], 1, 65000);
	}
	DDX_Text(pDX, IDC_EDITINK_IMPULS, m_nInkImpuls);
	DDV_MinMaxInt(pDX, m_nInkImpuls, 0, 500);
}


BEGIN_MESSAGE_MAP(CDlgDraw, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_INIT, &CDlgDraw::OnBnClickedButtonInit)
	ON_BN_CLICKED(IDC_BUTTON_X_PLUS, &CDlgDraw::OnBnClickedButtonXPlus)
	ON_BN_CLICKED(IDC_BUTTON_X_MINUS, &CDlgDraw::OnBnClickedButtonXMinus)
	ON_BN_CLICKED(IDC_BUTTON_Y_PLUS, &CDlgDraw::OnBnClickedButtonYPlus)
	ON_BN_CLICKED(IDC_BUTTON_Y_MINUS, &CDlgDraw::OnBnClickedButtonYMinus)
	ON_BN_CLICKED(IDC_BUTTON_Z_PLUS, &CDlgDraw::OnBnClickedButtonZPlus)
	ON_BN_CLICKED(IDC_BUTTON_Z_MINUS, &CDlgDraw::OnBnClickedButtonZMinus)
	ON_WM_TIMER()
	ON_WM_DESTROY()
	ON_BN_CLICKED(IDC_CHECK_ENABLE, &CDlgDraw::OnBnClickedCheckEnable)
	ON_BN_CLICKED(IDC_BUTTON_DRAW_SELECTED, &CDlgDraw::OnBnClickedButtonDrawSelected)
	ON_BN_CLICKED(IDC_BUTTON_GOTO_XY, &CDlgDraw::OnBnClickedButtonGotoXy)
	ON_BN_CLICKED(IDC_BUTTON_GOTO_Z, &CDlgDraw::OnBnClickedButtonGotoZ)
	ON_REGISTERED_MESSAGE(WM_PROCESS_XY, OnProcessXY)
	ON_REGISTERED_MESSAGE(WM_END_PROCESS_XY, OnEndProcessXY)
	ON_BN_CLICKED(IDC_BUTTON_SAVE_PROREJ, &CDlgDraw::OnBnClickedButtonSaveProrej)
	ON_BN_CLICKED(IDC_BUTTON2, &CDlgDraw::OnBnClickedButton2)
	ON_BN_CLICKED(IDC_BUTTON3, &CDlgDraw::OnBnClickedButton3)
	ON_BN_CLICKED(IDC_BUTTON4, &CDlgDraw::OnBnClickedButton4)
END_MESSAGE_MAP()


// CDlgDraw message handlers

BOOL CDlgDraw::OnInitDialog()
{
	CDialog::OnInitDialog();

	m_list.InsertColumn(0,"RGB",LVCFMT_LEFT,100);
	m_list.InsertColumn(1,"RGB_Text",LVCFMT_LEFT,100);
	m_list.InsertColumn(2,"Count",LVCFMT_LEFT,200);
	m_list.SetItemCount((int)m_list.m_arr_RGBs.GetCount());

	SetTimer(1,50,NULL);
	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}


void CDlgDraw::OnBnClickedButtonInit()
{
	m_nPointsDone = 0;
	int size = sizeof(_do_steps);
	size = sizeof(do_steps_multiplier);
	size = sizeof(do_timer_set);
	UpdateData();
	


	do_timer_set var_do_timer_set;
	var_do_timer_set.m_timer_res.u16 = 65536 - m_timer_res;//65536 - 2;

	AfxGetApp()->WriteProfileInt("settings","m_timer_res",m_timer_res);

	do_steps_multiplier step_mult;

	for(int i=0;i<MOTORS_COUNT;i++)
	{
		var_do_timer_set.m_multiplier[i] = m_mult[i];
		CString s;
		s.Format("m_mult%d",i);
		AfxGetApp()->WriteProfileInt("settings",s,m_mult[i]);

		step_mult.m_uMult[i] = m_step_mult[i];

		s.Format("m_step_mult%d",i);
		AfxGetApp()->WriteProfileInt("settings",s,m_step_mult[i]);
	}
	var_do_timer_set.m_ink_impuls = m_nInkImpuls;
	AfxGetApp()->WriteProfileInt("settings","m_nInkImpuls",m_nInkImpuls);
	
	m_pControlWrapper->ClearCommandQueue();
	ZeroMemory(&m_cur_pos,sizeof(m_cur_pos));
	m_pControlWrapper->CloseController();
	m_pControlWrapper->Connect();
	if(!m_pControlWrapper->IsOpen())
	{
		AfxMessageBox("Cannot connect to device");
		return;
	}
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	SetStepsToController(var_do_steps);

	
	m_pControlWrapper->SetTimer(var_do_timer_set);
	m_pControlWrapper->SetStepMultiplier(step_mult);


	m_p_do_control_signals->ms1 = 1;
	m_p_do_control_signals->ms2 = 1;
	m_p_do_control_signals->reset = 1;

	
	GetEnableFromCheck();

	m_pControlWrapper->SetControlSignals(*m_p_do_control_signals);

}


void CDlgDraw::OnBnClickedButtonXPlus()
{
	CString s;
	((CEdit*)GetDlgItem(IDC_EDIT_X))->GetWindowText(s);
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[X_POS] = atoi(s);
	SetStepsToController(var_do_steps);
}

void CDlgDraw::OnBnClickedButtonXMinus()
{
	CString s;
	((CEdit*)GetDlgItem(IDC_EDIT_X))->GetWindowText(s);
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[X_POS] = -atoi(s);
	SetStepsToController(var_do_steps);
}

void CDlgDraw::OnBnClickedButtonYPlus()
{
	CString s;
	((CEdit*)GetDlgItem(IDC_EDIT_Y))->GetWindowText(s);
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[Y_POS] = atoi(s);
	SetStepsToController(var_do_steps);
}

void CDlgDraw::OnBnClickedButtonYMinus()
{
	CString s;
	((CEdit*)GetDlgItem(IDC_EDIT_Y))->GetWindowText(s);
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[Y_POS] = -atoi(s);
	SetStepsToController(var_do_steps);
}

void CDlgDraw::OnBnClickedButtonZPlus()
{
	CString s;
	((CEdit*)GetDlgItem(IDC_EDIT_Z))->GetWindowText(s);
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[Z_POS] = atoi(s);
	SetStepsToController(var_do_steps);
}

void CDlgDraw::OnBnClickedButtonZMinus()
{
	CString s;
	((CEdit*)GetDlgItem(IDC_EDIT_Z))->GetWindowText(s);
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[Z_POS] = -atoi(s);
	SetStepsToController(var_do_steps);
}

void CDlgDraw::OnTimer(UINT_PTR nIDEvent)
{
	while(m_pControlWrapper->WriteCommandFromQueueToController())
	{
		m_commands_done++;
	}

	m_pControlWrapper->IsControllerAvailable();
	CString s;
	s.Format("%d",m_pControlWrapper->GetCountInQueue());
	((CEdit*)GetDlgItem(IDC_EDIT_QUEUE_COUNT))->SetWindowText(s);

	s.Format("Com:%d,Pnts:%d",m_commands_done,m_nPointsDone);
	((CEdit*)GetDlgItem(IDC_EDIT_COM_DONE))->SetWindowText(s);

	if(m_pControlWrapper->GetCountInQueue() == 0 && !IsPaused())
	{
		if(m_pThreadDrawGenerateXY)
		{
			m_pThreadDrawGenerateXY->GoAhead();
		}
		
	}
	CString result;
	BOOL bZeroPos = TRUE;
	for(int i = 0; i < MOTORS_COUNT; i++)
	{
		if(m_pControlWrapper->m_cur_steps.m_uSteps[i] != 0)
		{
			bZeroPos = FALSE;
		}
		s.Format("motor%d=%d;",i,m_pControlWrapper->m_cur_steps.m_uSteps[i]);
		result += s;
	}
	s.Format("ink=%d;",m_pControlWrapper->m_timer_ink_impuls);
	result += s;
	
	GetDlgItem(IDC_STATIC_INFO)->SetWindowText(result);

	if(m_bDriversOffPending && bZeroPos && m_pControlWrapper->GetCountInQueue() == 0)
	{
		m_bDriversOffPending--;
		if(!m_bDriversOffPending)
		{
			DriversOff();
		}
	}

	CDialog::OnTimer(nIDEvent);
}

void CDlgDraw::OnDestroy()
{
	KillTimer(1);
	m_pControlWrapper->ClearCommandQueue();
	DriversOff();
	
	m_pControlWrapper->WriteCommandFromQueueToController();
	CDialog::OnDestroy();
}

void CDlgDraw::OnBnClickedCheckEnable()
{
	GetEnableFromCheck();
	m_pControlWrapper->SetControlSignals(*m_p_do_control_signals);
}

void CDlgDraw::GetEnableFromCheck()
{
	m_p_do_control_signals->enable = BST_CHECKED!=((CButton*)GetDlgItem(IDC_CHECK_ENABLE))->GetCheck();
}

LRESULT CDlgDraw::OnEndProcessXY(WPARAM wParam, LPARAM lParam)
{
	//m_nPointsDone = 100;
	GoToStartPositionZ();
	GoToStartPosition();
	m_bDriversOffPending = 100;
	//DriversOff();
	return 0;
}
LRESULT CDlgDraw::OnProcessXY(WPARAM wParam, LPARAM lParam)
{
	UpdateData();
	Bitmap* pImage = g_pDoc->m_pImage;
	int w = pImage->GetWidth();
	int h = pImage->GetHeight();

	CPoint* pPoint = (CPoint*)wParam;
	int x = pPoint->x;
	int y = pPoint->y;
	
	CPoint point(x,y);
	delete pPoint;

	COLORREF selcolor = m_list.m_arr_RGBs[m_selection].color;
	Color color;

	Status status = pImage->GetPixel(x,h-y-1,&color);
	if(selcolor==color.ToCOLORREF())
	{
		m_nPointsDone++;
	}
	g_pView->UpdateCursor(&CPoint(x,h-y-1));
	
	GoToZ(0);
	
	m_pPrevPoint = point;
	
	GoToXY(x,y);
	GoToZ(0);

	if(m_pControlWrapper->GetCountInQueue() == 0 && !IsPaused())
	{
		m_pThreadDrawGenerateXY->GoAhead();
	}
	
	
	

	return 0;
}

void CDlgDraw::OnBnClickedButtonDrawSelected()
{
	m_selection = m_list.GetSelectionMark();
	if(m_selection < 0)
		return;

	m_commands_done = 0;
	m_nPointsDone = 0;
	COLORREF selcolor = m_list.m_arr_RGBs[m_selection].color;
	CString s;
	s.Format("%d,%d,%d is now Drawing",GetRValue(selcolor),GetGValue(selcolor),GetBValue(selcolor));
	SetWindowText(s);
	CreateXYThread(selcolor);
}


void CDlgDraw::GoToStartPosition()
{
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[Z_POS] = -m_cur_pos.z;
	var_do_steps.m_uSteps[X_POS] = -m_cur_pos.x;
	var_do_steps.m_uSteps[Y_POS] = -m_cur_pos.y;
	
	SetStepsToController(var_do_steps);
}

void CDlgDraw::GoToStartPositionX()
{
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[X_POS] = -m_cur_pos.x;
	SetStepsToController(var_do_steps);
}
void CDlgDraw::GoToStartPositionY()
{
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[Y_POS] = -m_cur_pos.y;
	SetStepsToController(var_do_steps);
}
void CDlgDraw::GoToStartPositionZ()
{
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[Z_POS] = -m_cur_pos.z;
	SetStepsToController(var_do_steps);
}

void CDlgDraw::ShiftX(int x)
{
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[X_POS] = x;
	SetStepsToController(var_do_steps);
}
void CDlgDraw::SetStepsToController(do_steps& steps)
{	//ASSERT(steps.m_uSteps[X_POS] >= 0);// We must not turn roller opposite direction
	m_cur_pos.y += steps.m_uSteps[Y_POS];
	m_cur_pos.x += steps.m_uSteps[X_POS];
	m_cur_pos.z += steps.m_uSteps[Z_POS];

	m_pControlWrapper->SetSteps(steps);
}

void CDlgDraw::GoToXY(int x, int y)
{
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[X_POS] = x-m_cur_pos.x;
	var_do_steps.m_uSteps[Y_POS] = y-m_cur_pos.y;
	SetStepsToController(var_do_steps);
}

void CDlgDraw::OnBnClickedButtonGotoXy()
{
	CString s;
	((CEdit*)GetDlgItem(IDC_EDIT_X_TO_GO))->GetWindowText(s);
	int x = atoi(s);

	((CEdit*)GetDlgItem(IDC_EDIT_Y_TO_GO))->GetWindowText(s);
	int y = atoi(s);
	
	GoToXY(x,y);
}

void CDlgDraw::GoToZ(int z)
{
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[Z_POS] = z-m_cur_pos.z;
	SetStepsToController(var_do_steps);
}
void CDlgDraw::GoToX(int x)
{
	do_steps var_do_steps;
	ZeroMemory(&var_do_steps,sizeof(var_do_steps));
	var_do_steps.m_uSteps[X_POS] = x-m_cur_pos.x;
	SetStepsToController(var_do_steps);
}

void CDlgDraw::OnBnClickedButtonGotoZ()
{
	CString s;
	((CEdit*)GetDlgItem(IDC_EDIT_Z_TO_GO))->GetWindowText(s);
	int z = atoi(s);
	GoToZ(z);
}


void CDlgDraw::DriversOff(void)
{
	m_p_do_control_signals->enable = 1;
	
	m_pControlWrapper->SetControlSignals(*m_p_do_control_signals);
	((CButton*)GetDlgItem(IDC_CHECK_ENABLE))->SetCheck(0);
}

BOOL CDlgDraw::IsPaused()
{
	BOOL bPaused = BST_CHECKED==((CButton*)GetDlgItem(IDC_CHECK_PAUSED))->GetCheck();
	return bPaused;
}

void CDlgDraw::Pause()
{
	((CButton*)GetDlgItem(IDC_CHECK_PAUSED))->SetCheck(BST_CHECKED);
}


void CDlgDraw::OnBnClickedButtonSaveProrej()
{
	int selection = m_list.GetSelectionMark();
	if(selection<0)
	{
		AfxMessageBox("Nothing is selected");
		return;
	}
 
	Bitmap* pImage = g_pDoc->m_pImage;
	int w = pImage->GetWidth();
	int h = pImage->GetHeight();

	//PixelFormat pf = pImage->GetPixelFormat();
	Bitmap Image1(w,h,PixelFormat24bppRGB);
	Bitmap Image2(w,h,PixelFormat24bppRGB);

	Graphics graphics1(&Image1);
	Graphics graphics2(&Image2);
	SolidBrush solidBrush(Color(255, 111, 111, 111));

	graphics1.FillRectangle(&solidBrush,0,0,w,h);
	graphics2.FillRectangle(&solidBrush,0,0,w,h);


	COLORREF selcolor = m_list.m_arr_RGBs[selection].color;

	for(int y = 0; y < h;y++)
	{
		for(int x = 0; x < w;x++)
		{
			Color color;
			Status status = pImage->GetPixel(x,h-y-1,&color);
			if(selcolor==color.ToCOLORREF())
			{
				color.SetFromCOLORREF(RGB(0,255,0));
				if(y%2)
				{
					Image1.SetPixel(x,h-y-1,color);
				}
				else
				{
					Image2.SetPixel(x,h-y-1,color);
				}
			}
		}
	}
	CLSID bmpClsid;
	GetEncoderClsid(L"image/bmp", &bmpClsid);

	Image1.Save(L"C:\\1.bmp",&bmpClsid);
	Image2.Save(L"C:\\2.bmp",&bmpClsid);
}

void CDlgDraw::OnBnClickedButton2()
{
	//Prprejka2;
	Bitmap* pImage = g_pDoc->m_pImage;
	int w = pImage->GetWidth();
	int h = pImage->GetHeight();

	CLSID bmpClsid;
	GetEncoderClsid(L"image/bmp", &bmpClsid);
	CDlgSize dlg;
	dlg.m_w = 540;
	dlg.m_h = 540;
	if(dlg.DoModal()!=IDOK)
	{
		return;
	}
	::CreateDirectory("C:\\Pics",NULL);
	int xmax = dlg.m_w;
	int ymax = dlg.m_h;
	for(int x = 0;x < w;x+=xmax)
	for(int y = 0;y < h;y+=ymax)
	{
		int ww = min(xmax,w-x);
		int hh = min(ymax,h-y);
		Bitmap* clone = pImage->Clone(Rect(x, y, ww, hh), PixelFormatDontCare);
		if(!clone)
		{
			continue;
		}
		CString s1;
		s1.Format("C:\\Pics\\%d-%d.bmp",x,y);
		USES_CONVERSION;
		clone->Save(A2W(s1),&bmpClsid);
		delete clone;
	}

	
}

int CDlgDraw::DoProrejka2(COLORREF selcolor,Bitmap& Image1,BOOL divider1)
{

	Bitmap* pImage = g_pDoc->m_pImage;
	int w = pImage->GetWidth();
	int h = pImage->GetHeight();

	int count1 = 0;

	int counterof3 = 0;
	for(int y = 0; y < h;y++)
	{
		int count_of_podryad = 0;
		if(divider1)
		{
			if(y%2 == 0)
				continue;
		}
		else
		{
			if(y%2!= 0)
				continue;
		}


		for(int x = 0; x < w;x++)
		{
			Color color;
			Status status = pImage->GetPixel(x,h-y-1,&color);

			if(selcolor==color.ToCOLORREF())
			{
				count_of_podryad++;
				if(count_of_podryad >= 5)
				{
					count_of_podryad = 0;
					continue;
				}

				if(x>1 && y>1 && x < (w-1) && y <(h-1))
				{
					int how_many = 0;
					Color color_test;

					Image1.GetPixel(x-1,h-y,&color_test);
					if(color_test.ToCOLORREF() == selcolor)
					{
						how_many++;
					}
					Image1.GetPixel(x,h-y,&color_test);
					if(color_test.ToCOLORREF() == selcolor)
					{
						how_many++;
					}
					Image1.GetPixel(x+1,h-y,&color_test);
					if(color_test.ToCOLORREF() == selcolor)
					{
						how_many++;
					}
					if(how_many >= 3)
					{
						continue;
					}
				}
				Image1.SetPixel(x,h-1-y,selcolor);
				count1++;
				
				
			}
			else
			{
				count_of_podryad = 0;
			}
		}
	}
	return count1;
}

void CDlgDraw::OnBnClickedButton3()
{
	m_pControlWrapper->ToggleLed();
}



void CDlgDraw::OnBnClickedButton4()
{
	m_pControlWrapper->ClearCommandQueue();
	ZeroMemory(&m_cur_pos,sizeof(m_cur_pos));
	m_pControlWrapper->CloseController();
	m_pControlWrapper->Connect();
	if(!m_pControlWrapper->IsOpen())
	{
		AfxMessageBox("Cannot connect to device");
		return;
	}
}
