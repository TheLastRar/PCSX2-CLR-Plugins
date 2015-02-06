/*  PCSX2 - PS2 Emulator for PCs
*  Copyright (C) 2002-2010  PCSX2 Dev Team
*
*  PCSX2 is free software: you can redistribute it and/or modify it under the terms
*  of the GNU Lesser General Public License as published by the Free Software Found-
*  ation, either version 3 of the License, or (at your option) any later version.
*
*  PCSX2 is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
*  without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR
*  PURPOSE.  See the GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License along with PCSX2.
*  If not, see <http://www.gnu.org/licenses/>.
*/

#ifndef __PCSX2DEFS_H__
#define __PCSX2DEFS_H__

// Indicate that this is the wx port to the plugins.
#define WX_PCSX2

#ifdef __CYGWIN__
#define __linux__
#endif

#include "Pcsx2Types.h"

#ifdef _MSC_VER
#	include <intrin.h>
extern "C" unsigned __int64 __xgetbv(int);
#else
#	include <intrin_x86.h>
#endif

// Renamed ARRAYSIZE to ArraySize -- looks nice and gets rid of Windows.h conflicts (air)
// Notes: I'd have used ARRAY_SIZE instead but ran into cross-platform lib conflicts with
// that as well.  >_<
#ifndef ArraySize
#	define ArraySize(x) (sizeof(x)/sizeof((x)[0]))
#endif

// --------------------------------------------------------------------------------------
// jASSUME - give hints to the optimizer  [obsolete, use pxAssume() instead]
// --------------------------------------------------------------------------------------
//  This is primarily useful for the default case switch optimizer, which enables VC to
//  generate more compact switches.
//
// Note: When using the PCSX2 Utilities library, this is deprecated.  Use pxAssert instead,
//  which itself optimizes to an __assume() hint in release mode builds.
//
#ifndef jASSUME
#	ifdef NDEBUG
#		define jBREAKPOINT() ((void) 0)
#		ifdef _MSC_VER
#			define jASSUME(exp) (__assume(exp))
#		else
#			define jASSUME(exp) do { if(!(exp)) __builtin_unreachable(); } while(0)
#		endif
#	else
#		define jBREAKPOINT() __debugbreak();
#		ifdef wxASSERT
#			define jASSUME(exp) wxASSERT(exp)
#		else
#			define jASSUME(exp) do { if(!(exp)) jBREAKPOINT(); } while(0)
#		endif
#	endif
#endif

// --------------------------------------------------------------------------------------
// Dev / Debug conditionals - Consts for using if() statements instead of uglier #ifdef.
// --------------------------------------------------------------------------------------
// Note: Using if() optimizes nicely in Devel and Release builds, but will generate extra
// code overhead in debug builds (since debug neither inlines, nor optimizes out const-
// level conditionals).  Normally not a concern, but if you stick if( IsDevbuild ) in
// some tight loops it will likely make debug builds unusably slow.
//
#ifdef __cplusplus
#	ifdef PCSX2_DEVBUILD
static const bool IsDevBuild = true;
#	else
static const bool IsDevBuild = false;
#	endif

#	ifdef PCSX2_DEBUG
static const bool IsDebugBuild = true;
#	else
static const bool IsDebugBuild = false;
#	endif

#else

#	ifdef PCSX2_DEVBUILD
static const u8 IsDevBuild = 1;
#	else
static const u8 IsDevBuild = 0;
#	endif

#	ifdef PCSX2_DEBUG
static const u8 IsDebugBuild = 1;
#	else
static const u8 IsDebugBuild = 0;
#	endif
#endif

#ifdef PCSX2_DEBUG
#	define pxDebugCode(code)		code
#else
#	define pxDebugCode(code)
#endif

#ifdef PCSX2_DEVBUILD
#	define pxDevelCode(code)		code
#else
#	define pxDevelCode(code)
#endif

#if !defined(PCSX2_DEBUG) && !defined(PCSX2_DEVEL)
#	define pxReleaseCode(code)
#else
#	define pxReleaseCode(code)		code
#endif

// --------------------------------------------------------------------------------------
// __aligned / __aligned16 / __pagealigned
// --------------------------------------------------------------------------------------
// GCC Warning!  The GCC linker (LD) typically fails to assure alignment of class members.
// If you want alignment to be assured, the variable must either be a member of a struct
// or a static global.
//
// __pagealigned is equivalent to __aligned(0x1000), and is used to align a dynarec code
// buffer to a page boundary (allows the use of execution-enabled mprotect).
//
// General Performance Warning: Any function that specifies alignment on a local (stack)
// variable will have to align the stack frame on enter, and restore it on exit (adds
// overhead).  Furthermore, compilers cannot inline functions that have aligned local
// vars.  So use local var alignment with much caution.
//

// Defines the memory page size for the target platform at compilation.  All supported platforms
// (which means Intel only right now) have a 4k granularity.
#define PCSX2_PAGESIZE		0x1000
static const int __pagesize = PCSX2_PAGESIZE;

// --------------------------------------------------------------------------------------
// Structure Packing (__packed)
// --------------------------------------------------------------------------------------
// Current Method:
// Use a combination of embedded compiler-specific #pragma mess in conjunction with a
// __packed macro.  The former appeases the MSVC gods, the latter appeases the GCC gods.
// The end result looks something like this:
//
// #ifdef _MSC_VER
// #   pragma pack(1)
// #endif
//
// struct SomeKindaFail {
//     u8   neat;
//     u32  unaligned32;
// } __packed;
//
// MSVC 2008 and better support __pragma, however there's no way to support that in
// a way that's backwards compatible to VS 2005, without still including the old-style
// #pragma mess.  So there's really not much point (yet) in using it.  I've included macros
// that utilize __pragma (commented out below) which can be deployed at a time when we
// are ok with the idea of completely breaking backwards compat with VC2005/prior.
//

// --------------------------------------------------------------------------------------
//  Microsoft Visual Studio
// --------------------------------------------------------------------------------------
#ifdef _MSC_VER

// Using these breaks compat with VC2005; so we're not using it yet.
//#	define __pack_begin		__pragma(pack(1))
//#	define __pack_end		__pragma(pack())

// This is the 2005/earlier compatible packing define, which must be used in conjunction
// with #ifdef _MSC_VER/#pragma pack() directives (ugly).
#	define __packed

#	define __aligned(alig)	__declspec(align(alig))
#	define __aligned16		__declspec(align(16))
#	define __aligned32		__declspec(align(32))
#	define __pagealigned	__declspec(align(PCSX2_PAGESIZE))

// Deprecated; use __align instead.
#	define PCSX2_ALIGNED(alig,x)		__declspec(align(alig)) x
#	define PCSX2_ALIGNED_EXTERN(alig,x)	extern __declspec(align(alig)) x
#	define PCSX2_ALIGNED16(x)			__declspec(align(16)) x
#	define PCSX2_ALIGNED16_EXTERN(x)	extern __declspec(align(16)) x

#	define __noinline		__declspec(noinline)
#	define __threadlocal	__declspec(thread)

// Don't know if there are Visual C++ equivalents of these.
#	define likely(x)		(!!(x))
#	define unlikely(x)		(!!(x))

#	define CALLBACK		   __stdcall

#else

// --------------------------------------------------------------------------------------
//  GCC / Intel Compilers Section
// --------------------------------------------------------------------------------------

#	define __packed			__attribute__((packed))

#	define __aligned(alig)	__attribute__((aligned(alig)))
#	define __aligned16		__attribute__((aligned(16)))
#	define __aligned32		__attribute__((aligned(32)))
#	define __pagealigned	__attribute__((aligned(PCSX2_PAGESIZE)))
// Deprecated; use __align instead.
#	define PCSX2_ALIGNED(alig,x) x __attribute((aligned(alig)))
#	define PCSX2_ALIGNED16(x) x __attribute((aligned(16)))
#	define PCSX2_ALIGNED_EXTERN(alig,x) extern x __attribute((aligned(alig)))
#	define PCSX2_ALIGNED16_EXTERN(x) extern x __attribute((aligned(16)))

#	define __assume(cond)	((void)0)	// GCC has no equivalent for __assume
#	define CALLBACK			__attribute__((stdcall))

// Inlining note: GCC needs ((unused)) attributes defined on inlined functions to suppress
// warnings when a static inlined function isn't used in the scope of a single file (which
// happens *by design* like all the friggen time >_<)

#	define __fastcall		__attribute__((fastcall))
#	define _inline			__inline__ __attribute__((unused))
#	ifdef NDEBUG
#		define __forceinline	__attribute__((always_inline,unused))
#	else
#		define __forceinline	__attribute__((unused))
#	endif
#	define __noinline		__attribute__((noinline))
#	define __threadlocal	__thread
#	define likely(x)		__builtin_expect(!!(x), 1)
#	define unlikely(x)		__builtin_expect(!!(x), 0)
#endif

// --------------------------------------------------------------------------------------
// __releaseinline / __ri -- a forceinline macro that is enabled for RELEASE/PUBLIC builds ONLY.
// --------------------------------------------------------------------------------------
// This is useful because forceinline can make certain types of debugging problematic since
// functions that look like they should be called won't breakpoint since their code is
// inlined, and it can make stack traces confusing or near useless.
//
// Use __releaseinline for things which are generally large functions where trace debugging
// from Devel builds is likely useful; but which should be inlined in an optimized Release
// environment.
//
#ifdef PCSX2_DEVBUILD
#	define __releaseinline
#else
#	define __releaseinline __forceinline
#endif

#define __ri	__releaseinline
#define __fi	__forceinline
#define __fc	__fastcall

#endif
