#pragma once

typedef CMap<COLORREF,COLORREF,CString,LPCTSTR> TIK_COLOR_MAP;

typedef struct RGB_NEAREST
{
	COLORREF color;
	float sum;
	int sumr;
	int sumg;
	int sumb;
}_RGB_NEAREST;


typedef CArray<RGB_NEAREST> RGB_COLOR_ARR;

class TikFinder
{
public:
	TikFinder(void);
	virtual ~TikFinder(void);
	void FillMap(void);
	TIK_COLOR_MAP m_map_COLORREF_to_Tikurila;
	RGB_COLOR_ARR m_arr_RGBs;
	int GetNearestColors(int r, int g, int b,LPCTSTR prior);
	COLORREF m_tik_map[256][256][256];
	void Fill3DMap(void);
	CList<COLORREF> m_list_colors;
};
