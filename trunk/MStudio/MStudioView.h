// MStudioView.h : interface of the CMStudioView class
//


#pragma once
#include "atltypes.h"


class CMStudioView : public CView
{
protected: // create from serialization only
	CMStudioView();
	DECLARE_DYNCREATE(CMStudioView)

// Attributes
public:
	enum ViewType
	{
		ORIGINAL,
		RESULT,
		EMPTY
	};
	ViewType m_ViewType;


// Operations
public:

// Overrides
public:
	virtual void OnDraw(CDC* pDC);  // overridden to draw this view
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
protected:

// Implementation
public:
	virtual ~CMStudioView();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Generated message map functions
protected:
	DECLARE_MESSAGE_MAP()
public:
	afx_msg int OnMouseActivate(CWnd* pDesktopWnd, UINT nHitTest, UINT message);
	afx_msg void OnViewShowbaseimage();
	afx_msg void OnViewShowresultimage();
	afx_msg void OnUpdateViewShowbaseimage(CCmdUI *pCmdUI);
	afx_msg void OnUpdateViewShowresultimage(CCmdUI *pCmdUI);

#ifndef _DEBUG  // debug version in MStudioView.cpp
inline CMStudioDoc* CMStudioView::GetDocument() const
   { return reinterpret_cast<CMStudioDoc*>(m_pDocument); }
#else
		CMStudioDoc* GetDocument() const;
#endif

CPoint m_current_draw;
void UpdateCursor(CPoint* pPoint);
};