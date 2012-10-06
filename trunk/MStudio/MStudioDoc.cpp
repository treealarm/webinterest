// MStudioDoc.cpp : implementation of the CMStudioDoc class
//

#include "stdafx.h"
#include "MStudio.h"

#include "MStudioDoc.h"
#include "MainFrm.h"
#include "TikFinder.h"
#include "DlgDraw.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

int GetEncoderClsid(const WCHAR* format, CLSID* pClsid)
{
   UINT  num = 0;          // number of image encoders
   UINT  size = 0;         // size of the image encoder array in bytes

   ImageCodecInfo* pImageCodecInfo = NULL;

   GetImageEncodersSize(&num, &size);
   if(size == 0)
      return -1;  // Failure

   pImageCodecInfo = (ImageCodecInfo*)(malloc(size));
   if(pImageCodecInfo == NULL)
      return -1;  // Failure

   GetImageEncoders(num, size, pImageCodecInfo);

   for(UINT j = 0; j < num; ++j)
   {
      if( wcscmp(pImageCodecInfo[j].MimeType, format) == 0 )
      {
         *pClsid = pImageCodecInfo[j].Clsid;
         free(pImageCodecInfo);
         return j;  // Success
      }    
   }

   free(pImageCodecInfo);
   return -1;  // Failure
}
// CMStudioDoc

IMPLEMENT_DYNCREATE(CMStudioDoc, CDocument)

BEGIN_MESSAGE_MAP(CMStudioDoc, CDocument)
	
	ON_COMMAND(ID_CONVERTRESULT_TOBLACKANDWHITE, &CMStudioDoc::OnConvertresultToblackandwhite)
	ON_COMMAND(ID_DRAW_DODRAW, &CMStudioDoc::OnDrawDodraw)
	ON_UPDATE_COMMAND_UI(ID_DRAW_DODRAW, &CMStudioDoc::OnUpdateDrawDodraw)
	ON_COMMAND(ID_CONVERTRESULT_TOTEKURILLA, &CMStudioDoc::OnConvertresultTotekurilla)
END_MESSAGE_MAP()


// CMStudioDoc construction/destruction
CMStudioDoc* g_pDoc = NULL;

CMStudioDoc::CMStudioDoc()
//: m_pTikFinder(new TikFinder)
{
	//m_pTikFinder->FillMap();

	DWORD dw1 = GetTickCount();
	m_pImage = NULL;
	m_pResultImage = NULL;
	m_pImage_Selected = NULL;
	g_pDoc = this;
}

CMStudioDoc::~CMStudioDoc()
{
	//delete m_pTikFinder;
	if(m_pImage)
	{
		delete m_pImage;
		m_pImage = NULL;
	}
	if(m_pResultImage)
	{
		delete m_pResultImage;
		m_pResultImage = NULL;
	}
	if(m_pImage_Selected)
	{
		delete m_pImage_Selected;
		m_pImage_Selected = NULL;
	}
	g_pDoc = NULL;
}

BOOL CMStudioDoc::OnNewDocument()
{
	if (!CDocument::OnNewDocument())
		return FALSE;

	// TODO: add reinitialization code here
	// (SDI documents will reuse this document)

	return TRUE;
}




// CMStudioDoc serialization

void CMStudioDoc::Serialize(CArchive& ar)
{
	if (ar.IsStoring())
	{
		
	}
	else
	{
		// TODO: add loading code here
	}
}


// CMStudioDoc diagnostics

#ifdef _DEBUG
void CMStudioDoc::AssertValid() const
{
	CDocument::AssertValid();
}

void CMStudioDoc::Dump(CDumpContext& dc) const
{
	CDocument::Dump(dc);
}
#endif //_DEBUG


// CMStudioDoc commands

BOOL CMStudioDoc::OnOpenDocument(LPCTSTR lpszPathName)
{
	if (!CDocument::OnOpenDocument(lpszPathName))
		return FALSE;

	if(m_pImage)
	{
		delete m_pImage;
		m_pImage = NULL;
	}
	if(m_pResultImage)
	{
		delete m_pResultImage;
		m_pResultImage = NULL;
	}
#undef new
#define new new

	USES_CONVERSION;
	m_pImage = new Gdiplus::Bitmap(A2W(lpszPathName));
	if(m_pImage)
	{
		int w = m_pImage->GetWidth();
		int h = m_pImage->GetHeight();
		m_pResultImage = new Gdiplus::Bitmap(w,h,PixelFormat24bppRGB );
	}

#undef new
#ifdef _DEBUG
#define new DEBUG_NEW
#endif
	
	int w = g_pDoc->m_pImage->GetWidth();
	int h = g_pDoc->m_pImage->GetHeight();
	int x = 0;
	int y = 0;
	
	ZeroMemory(m_map_color_count,sizeof(int)*256*256*256);
	Color color;
	for(y = 0; y < h; ++y)
	{
	  for(x = 0; x < w; ++x)
	  {
		Status status = g_pDoc->m_pImage->GetPixel(x,y,&color);
		COLORREF cr = color.ToCOLORREF();
		m_map_color_count[GetRValue(cr)][GetGValue(cr)][GetBValue(cr)]++;
	  }
	}

	return TRUE;
}


COLORREF CMStudioDoc::GetPixelColor(int x, int y)
{
	Color color;
	Status status = m_pImage->GetPixel(x,y,&color);
	ASSERT(status == Ok);
	return color.ToCOLORREF();
}

void CMStudioDoc::ConvertToBlackAndWhite()
{
	int w = m_pImage->GetWidth();
	int h = m_pImage->GetHeight();
	int x = 0;
	int y = 0;
	
	
	Color color;
	for(int row = 0; row < h; ++row)
	{
	  for(int col = 0; col < w; ++col)
	  {
		Status status = m_pImage->GetPixel(col,row,&color);
		int gray = (color.GetR()+color.GetG()+color.GetB())/3;
		color.SetFromCOLORREF( RGB(gray,gray,gray));
		m_pResultImage->SetPixel(col,row,color);
	  }
	}
}
void CMStudioDoc::OnConvertresultToblackandwhite()
{
	ConvertToBlackAndWhite();
}

BOOL CMStudioDoc::OnSaveDocument(LPCTSTR lpszPathName)
{
	if(m_pResultImage)
	{
		CLSID pngClsid;
		GetEncoderClsid(L"image/png", &pngClsid);

		USES_CONVERSION;

		m_pResultImage->Save(A2W(lpszPathName),&pngClsid, NULL);
		return 1;
	}

	return CDocument::OnSaveDocument(lpszPathName);
}

void CMStudioDoc::OnDrawDodraw()
{
	CDlgDraw dlg;
	dlg.DoModal();
	if(m_pImage_Selected)
	{
		delete m_pImage_Selected;
		m_pImage_Selected = NULL;
	}
}

void CMStudioDoc::OnUpdateDrawDodraw(CCmdUI *pCmdUI)
{
	pCmdUI->Enable(1);//m_pImage!=NULL);
}

COLORREF tik_map[256][256][256];

void CMStudioDoc::OnConvertresultTotekurilla()
{
	/*int b = 0;
	for(int r=0;r<256;r++)
	{

	for(int g=0;g<256;g++)
	{
		CString ss;
	ss.Format("%d,%d,%d",r,g,b);
	g_pFrame->m_wndStatusBar.SetWindowText(ss);
	g_pFrame->m_wndStatusBar.RedrawWindow();

	for(b=0;b<256;b++)
	{
		m_pTikFinder->GetNearestColors(r,g,b,"");
		COLORREF cr = m_pTikFinder->m_arr_RGBs[0].color;
		m_pTikFinder->m_tik_map[r][g][b] = cr;
		//CString s;
		//s.Format("m_tik_map[%d][%d][%d]=RGB(%d,%d,%d);\n",r,g,b,int(GetRValue(cr)),int(GetGValue(cr)),int(GetBValue(cr)));
	}
	}
	}
	int len = sizeof(m_pTikFinder->m_tik_map);
	CFile file;
	file.Open("C:\\arrtik.bin",CFile::modeWrite|CFile::modeCreate);
	file.Write(m_pTikFinder->m_tik_map,sizeof(m_pTikFinder->m_tik_map));*/
	
	int w = m_pImage->GetWidth();
	int h = m_pImage->GetHeight();

	
	
	Color color;
	for(int row = 0; row < h; ++row)
	{
	  for(int col = 0; col < w; ++col)
	  {
		Status status = m_pImage->GetPixel(col,row,&color);
		color.SetFromCOLORREF(m_pTikFinder->m_tik_map[color.GetR()][color.GetG()][color.GetB()]);
		m_pResultImage->SetPixel(col,row,color);
	  }
	}
	
}

void CMStudioDoc::OnSelectionChanged(COLORREF cr)
{
	if(m_pImage_Selected)
	{
		delete m_pImage_Selected;
		m_pImage_Selected = NULL;
	}

	int w = m_pImage->GetWidth();
	int h = m_pImage->GetHeight();

	//m_pImage_Selected = m_pImage->Clone(0,0,w,h,PixelFormat24bppRGB );

#undef new
#define new new

	USES_CONVERSION;
	m_pImage_Selected = new Gdiplus::Bitmap(w,h,PixelFormat24bppRGB);

#undef new
#ifdef _DEBUG
#define new DEBUG_NEW
#endif

	Color color;
	for(int row = 0; row < h; ++row)
	{
	  for(int col = 0; col < w; ++col)
	  {
		Status status = m_pImage->GetPixel(col,row,&color);
		if(color.ToCOLORREF() == cr)
		{
			color.SetFromCOLORREF(RGB(0,255,0));
			m_pImage_Selected->SetPixel(col,row,color);
		}
		else
		{
			m_pImage_Selected->SetPixel(col,row,color);
		}
	  }
	}
	UpdateAllViews(NULL);
}
