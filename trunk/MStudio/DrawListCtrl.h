#pragma once


// CDrawListCtrl
typedef struct RGB_PIXEL_COUNT
{
	COLORREF color;
	int nPixels;
}_RGB_PIXEL_COUNT;

typedef CArray<RGB_PIXEL_COUNT> RGB_PIXEL_COUNT_ARR;

class CDrawListCtrl : public CListCtrl
{
	DECLARE_DYNAMIC(CDrawListCtrl)

public:
	CDrawListCtrl();
	virtual ~CDrawListCtrl();

protected:
	DECLARE_MESSAGE_MAP()
public:
	RGB_PIXEL_COUNT_ARR m_arr_RGBs;
	virtual void DrawItem(LPDRAWITEMSTRUCT /*lpDrawItemStruct*/);
	afx_msg void OnHdnItemclick(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnLvnItemchanged(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnNMClick(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnLvnOdstatechanged(NMHDR *pNMHDR, LRESULT *pResult);
	int m_item_selected;
};


