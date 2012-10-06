// MStudioView.cpp : implementation of the CMStudioView class
//

#include "stdafx.h"
#include "MStudio.h"

#include "MStudioDoc.h"
#include "MStudioView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

CMStudioView* g_pView = NULL;

// CMStudioView

IMPLEMENT_DYNCREATE(CMStudioView, CView)

BEGIN_MESSAGE_MAP(CMStudioView, CView)
	ON_WM_MOUSEACTIVATE()
	ON_COMMAND(ID_VIEW_SHOWBASEIMAGE, &CMStudioView::OnViewShowbaseimage)
	ON_COMMAND(ID_VIEW_SHOWRESULTIMAGE, &CMStudioView::OnViewShowresultimage)
	ON_UPDATE_COMMAND_UI(ID_VIEW_SHOWBASEIMAGE, &CMStudioView::OnUpdateViewShowbaseimage)
	ON_UPDATE_COMMAND_UI(ID_VIEW_SHOWRESULTIMAGE, &CMStudioView::OnUpdateViewShowresultimage)
END_MESSAGE_MAP()

// CMStudioView construction/destruction

CMStudioView::CMStudioView()
: m_current_draw(100,100)
{
	m_ViewType = ORIGINAL;
	g_pView = this;
}

CMStudioView::~CMStudioView()
{
	g_pView = NULL;
}

BOOL CMStudioView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: Modify the Window class or styles here by modifying
	//  the CREATESTRUCT cs

	return CView::PreCreateWindow(cs);
}

// CMStudioView drawing

void CMStudioView::OnDraw(CDC* pDC)
{
	CMStudioDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	if (!pDoc || !pDoc->m_pImage || m_ViewType == EMPTY)
		return;

	CRect rect;
	GetClientRect(rect);
	
	
	int w = pDoc->m_pImage->GetWidth();
	int h = pDoc->m_pImage->GetHeight();

	Gdiplus::Graphics myGraphics(*pDC);
	

	float maxw = float(rect.Width())/float(w);
	float maxh = float(rect.Height())/float(h);
	
	float Zoom = 0;

	if(maxh < maxw)
		Zoom = maxh;
	else
		Zoom = maxw;
	

	if(pDoc->m_pImage_Selected)
	{
		myGraphics.DrawImage(pDoc->m_pImage_Selected,(float)0,(float)0,w*Zoom,h*Zoom);
	}
	else
	{
	if(m_ViewType == ORIGINAL && pDoc->m_pImage)
		myGraphics.DrawImage(pDoc->m_pImage,(float)0,(float)0,w*Zoom,h*Zoom);
	if(m_ViewType == RESULT && pDoc->m_pResultImage)
		myGraphics.DrawImage(pDoc->m_pResultImage,(float)0,(float)0,w*Zoom,h*Zoom);
	}
	
	int x = int((m_current_draw.x-5)*Zoom);
	int y = int((m_current_draw.y-5)*Zoom);
    int width = int(10*Zoom);
    int height = int(10*Zoom);

	Pen pen(Color::Red);
	myGraphics.DrawEllipse(&pen,x,y,width,height);
	

	//CRgn   rgnA, rgnB;

	//CPoint ptVertex[5];

	//ptVertex[0].x = 180;
	//ptVertex[0].y = 80;
	//ptVertex[1].x = 100;
	//ptVertex[1].y = 160;
	//ptVertex[2].x = 120;
	//ptVertex[2].y = 260;
	//ptVertex[3].x = 240;
	//ptVertex[3].y = 260;
	//ptVertex[4].x = 260;
	//ptVertex[4].y = 160;

	//VERIFY(rgnA.CreatePolygonRgn( ptVertex, 5, ALTERNATE));

	//CRect rectRgnBox;
	//int nRgnBoxResult = rgnA.GetRgnBox( &rectRgnBox );
	//ASSERT( nRgnBoxResult != ERROR || nRgnBoxResult != NULLREGION );

	//CBrush brA, brB;
	//VERIFY(brA.CreateSolidBrush( RGB(255, 0, 0) ));  // rgnA Red
	//VERIFY(pDC->FrameRgn( &rgnA, &brA, 2, 2 ));
	//VERIFY(brB.CreateSolidBrush( RGB(0, 0, 255) ));  // Blue
	//rectRgnBox.InflateRect(3,3);
	//pDC->FrameRect( &rectRgnBox, &brB );

}


// CMStudioView diagnostics

#ifdef _DEBUG
void CMStudioView::AssertValid() const
{
	CView::AssertValid();
}

void CMStudioView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}

CMStudioDoc* CMStudioView::GetDocument() const // non-debug version is inline
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CMStudioDoc)));
	return (CMStudioDoc*)m_pDocument;
}
#endif //_DEBUG


// CMStudioView message handlers

int CMStudioView::OnMouseActivate(CWnd* pDesktopWnd, UINT nHitTest, UINT message)
{
	return TRUE;

	return CView::OnMouseActivate(pDesktopWnd, nHitTest, message);
}

void CMStudioView::OnViewShowbaseimage()
{
	m_ViewType = ORIGINAL;
	RedrawWindow();
}

void CMStudioView::OnViewShowresultimage()
{
	m_ViewType = RESULT;
	RedrawWindow();
}

void CMStudioView::OnUpdateViewShowbaseimage(CCmdUI *pCmdUI)
{
	pCmdUI->SetCheck(m_ViewType == ORIGINAL);
}

void CMStudioView::OnUpdateViewShowresultimage(CCmdUI *pCmdUI)
{
	pCmdUI->SetCheck(m_ViewType == RESULT);
}

void CMStudioView::UpdateCursor(CPoint* pPoint)
{
	CPoint prev_point = m_current_draw;
	m_current_draw = *pPoint;

	if (!g_pDoc || !g_pDoc->m_pImage)
		return;

	CRect rect;
	GetClientRect(rect);
	
	
	int w = g_pDoc->m_pImage->GetWidth();
	int h = g_pDoc->m_pImage->GetHeight();

	float maxw = float(rect.Width())/float(w);
	float maxh = float(rect.Height())/float(h);
	
	float Zoom = 0;

	if(maxh < maxw)
		Zoom = maxh;
	else
		Zoom = maxw;

	RedrawWindow(&CRect(int((m_current_draw.x-15)*Zoom),int((m_current_draw.y-15)*Zoom),
		int((m_current_draw.x+15)*Zoom),int((m_current_draw.y+15)*Zoom)));
	RedrawWindow(&CRect(int((prev_point.x-15)*Zoom),int((prev_point.y-15)*Zoom),
		int((prev_point.x+15)*Zoom),int((prev_point.y+15)*Zoom)));

}
