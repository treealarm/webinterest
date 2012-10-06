// MStudioDoc.h : interface of the CMStudioDoc class
//


#pragma once
class TikFinder;


class CMStudioDoc : public CDocument
{
protected: // create from serialization only
	CMStudioDoc();
	DECLARE_DYNCREATE(CMStudioDoc)

// Attributes
public:
// Operations
public:

// Overrides
public:
	virtual BOOL OnNewDocument();
	virtual void Serialize(CArchive& ar);

// Implementation
public:
	virtual ~CMStudioDoc();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Generated message map functions
protected:
	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL OnOpenDocument(LPCTSTR lpszPathName);
	Bitmap* m_pImage;
	Bitmap* m_pResultImage;
	Bitmap* m_pImage_Selected;
	int m_map_color_count[256][256][256];

	COLORREF GetPixelColor(int x, int y);
	void	ConvertToBlackAndWhite();
	afx_msg void OnConvertresultToblackandwhite();
	virtual BOOL OnSaveDocument(LPCTSTR lpszPathName);
	TikFinder* m_pTikFinder;
	afx_msg void OnDrawDodraw();
	afx_msg void OnUpdateDrawDodraw(CCmdUI *pCmdUI);
	afx_msg void OnConvertresultTotekurilla();
	void OnSelectionChanged(COLORREF cr);
};


