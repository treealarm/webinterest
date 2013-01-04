// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>
#include <Winhttp.h>
#include "DShow.h"
#include <ssfsdkapi.h>
#include <Dvdmedia.h>
#include <string>
#include <sstream>
#include <map>
#include <iosfwd>
//#include <stdio.h>
#include <ATLCONV.H>
#include <MMReg.h>
#include <algorithm> 
#include <mfapi.h>
#include <wmcodecdsp.h>

#define MAKELANGUAGE( a, b, c )  ( ((c - 0x60) & 0x1f) | ( ((b-0x60) & 0x1f) << 5 ) |  ( ((a-0x60) & 0x1f) << 10 ))