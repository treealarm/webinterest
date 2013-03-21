// DrawListCtrl.cpp : implementation file
//

#include "stdafx.h"
#include "MStudio.h"
#include "DrawListCtrl.h"
#include "MStudioDoc.h"
#include "MStudioView.h"

int compare_count( void *pParam, const void *cr1, const void *cr2)
{
	RGB_PIXEL_COUNT* pcr1 = (RGB_PIXEL_COUNT*)cr1;
	RGB_PIXEL_COUNT* pcr2 = (RGB_PIXEL_COUNT*)cr2;

	if(pcr1->nPixels > pcr2->nPixels)
		return -1;
	if(pcr1->nPixels < pcr2->nPixels)
		return 1;

	return 0;
}

int compare_count1( void *pParam, const void *cr1, const void *cr2)
{
	RGB_PIXEL_COUNT* pcr1 = (RGB_PIXEL_COUNT*)cr1;
	RGB_PIXEL_COUNT* pcr2 = (RGB_PIXEL_COUNT*)cr2;

	if(pcr1->color > pcr2->color)
		return -1;
	if(pcr1->color < pcr2->color)
		return 1;

	return 0;
}
// CDrawListCtrl

IMPLEMENT_DYNAMIC(CDrawListCtrl, CListCtrl)

CDrawListCtrl::CDrawListCtrl()
: m_item_selected(-1)
{
	for(int r=0;r<256;r++)
		for(int g=0;g<256;g++)
			for(int b=0;b<256;b++)
			{
				if(g_pDoc->m_map_color_count[r][g][b]>0)
				{
					RGB_PIXEL_COUNT info;
					info.color = RGB(r,g,b);
					info.nPixels = g_pDoc->m_map_color_count[r][g][b];
					m_arr_RGBs.Add(info);
				}
				
			}
	/*for(POSITION pos = g_pDoc->m_map_color_count.GetStartPosition();pos!=NULL;)
	{
		RGB_PIXEL_COUNT info;
		g_pDoc->m_map_color_count.GetNextAssoc(pos,info.color,info.nPixels);
		m_arr_RGBs.Add(info);
	}*/
}

CDrawListCtrl::~CDrawListCtrl()
{
}


BEGIN_MESSAGE_MAP(CDrawListCtrl, CListCtrl)
	ON_NOTIFY(HDN_ITEMCLICKA, 0, &CDrawListCtrl::OnHdnItemclick)
	ON_NOTIFY(HDN_ITEMCLICKW, 0, &CDrawListCtrl::OnHdnItemclick)
	ON_NOTIFY_REFLECT(LVN_ITEMCHANGED, &CDrawListCtrl::OnLvnItemchanged)
	ON_NOTIFY_REFLECT(NM_CLICK, &CDrawListCtrl::OnNMClick)
	ON_NOTIFY_REFLECT(LVN_ODSTATECHANGED, &CDrawListCtrl::OnLvnOdstatechanged)
END_MESSAGE_MAP()



// CDrawListCtrl message handlers



void CDrawListCtrl::DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct)
{
	CListCtrl& ListCtrl=*this;
	CDC* pDC = CDC::FromHandle(lpDrawItemStruct->hDC);
	CRect rcItem(lpDrawItemStruct->rcItem);
	int nItem = lpDrawItemStruct->itemID;
	BOOL bFocus = (GetFocus() == this);

	CRect rcLabel;
	
	int r = GetRValue(m_arr_RGBs[nItem].color);
	int g = GetGValue(m_arr_RGBs[nItem].color);
	int b = GetBValue(m_arr_RGBs[nItem].color);

	ListCtrl.GetSubItemRect(nItem,1,LVIR_BOUNDS,rcLabel);
	int left_of_color = rcLabel.left;
	CString s;
	s.Format("%d,%d,%d",r,g,b);
	pDC->DrawText(s, -1, rcLabel,
		DT_SINGLELINE | DT_NOPREFIX | DT_NOCLIP | DT_VCENTER);

	
	s.Format("%d",m_arr_RGBs[nItem].nPixels);
	ListCtrl.GetSubItemRect(nItem,2,LVIR_BOUNDS,rcLabel);
	pDC->DrawText(s, -1, rcLabel,
		DT_SINGLELINE | DT_NOPREFIX | DT_NOCLIP | DT_VCENTER);

	ListCtrl.GetSubItemRect(nItem,0,LVIR_BOUNDS,rcLabel);
	rcLabel.right = left_of_color-2;
	CBrush brush;
	brush.CreateSolidBrush(RGB(r,g,b));
	pDC->FillRect(rcLabel,&brush);
	int sel = ListCtrl.GetSelectionMark();
	if(sel == nItem)
	{
		ListCtrl.GetSubItemRect(nItem,0,LVIR_BOUNDS,rcLabel);
		pDC->Draw3dRect(rcLabel,RGB(255,0,0),RGB(0,255,0));
	}
}

void CDrawListCtrl::OnHdnItemclick(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMHEADER phdr = reinterpret_cast<LPNMHEADER>(pNMHDR);
	if(phdr->iItem == 2)
	{
		qsort_s(m_arr_RGBs.GetData(),m_arr_RGBs.GetSize(),sizeof(RGB_PIXEL_COUNT),compare_count,(void*)this);
		RedrawWindow();
	}
	if(phdr->iItem == 1)
	{
		qsort_s(m_arr_RGBs.GetData(),m_arr_RGBs.GetSize(),sizeof(RGB_PIXEL_COUNT),compare_count1,(void*)this);
		RedrawWindow();
	}
	// TODO: Add your control notification handler code here
	*pResult = 0;
}

void CDrawListCtrl::OnLvnItemchanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);
	
	int sel = GetSelectionMark();
	if(sel >= 0 && m_item_selected != sel)
	{
		g_pDoc->OnSelectionChanged(m_arr_RGBs[sel].color);
		m_item_selected = sel;
		UpdateWindow();
		RedrawWindow();
	}
	*pResult = 0;
}

void CDrawListCtrl::OnNMClick(NMHDR *pNMHDR, LRESULT *pResult)
{
	int sel = GetSelectionMark();
	if(sel >=0 && m_item_selected != sel)
	{
		g_pDoc->OnSelectionChanged(m_arr_RGBs[sel].color);
		m_item_selected = sel;
		UpdateWindow();
		RedrawWindow();
	}
	
	*pResult = 0;
}

void CDrawListCtrl::OnLvnOdstatechanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLVODSTATECHANGE pStateChanged = reinterpret_cast<LPNMLVODSTATECHANGE>(pNMHDR);
	// TODO: Add your control notification handler code here
	*pResult = 0;
}
