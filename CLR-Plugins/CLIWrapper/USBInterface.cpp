/*  USBnull
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

//program flow
/*At Startup when pre-configured;
* USBtest
* SetLogDir
* SetSettingDir
*
*At Config Enumuration (Shared Functions)
*At Plugin Selection Config Apply
* Test
* SetLogDir
* SetSettingDir
* SetLogDir
* SetSettingDir
* Init
*
*At PS2 Boot
* SetLogDir
* SetSettingDir
* Init
* SetSettingDir
* Open
* Set irqCallback
* Get IrqHandler
* Set ram
* <hazard a guess at a bunch of read/write stuff>
*
*At PS2 Suspend
* Close
*
*At Resume
* SetSettingDir
* Open
* Set irqCallback
*
* At PS2 Shutdown
* close
*
*At PCSX2 close
* Shutdown
*
*Plugin Deselection
* Shutdown
*/


#include <stdlib.h>
#include <string>
using namespace std;
#include "USBinterface.h"

using namespace System::Runtime::InteropServices;
using namespace System::Reflection;
NativeUSBWrapper* nat_usb;

string s_strIniPath = "inis";
string s_strLogPath = "logs";

//#ifdef _MSC_VER
//#define snprintf sprintf_s
//#endif

//USBcallback USBirq;
//Config conf;

u8 *ram;

void USBCLRInit()
{
	if (nat_usb == NULL)
	{
		nat_usb = new NativeUSBWrapper();
		nat_usb->USBwrap = gcnew USBWrapper();
	}
}

EXPORT_C_(s32) USBinit()
{
	//CLRInit();
	try
	{
		return nat_usb->USBwrap->Init();
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
		return -1;
	}
}

EXPORT_C_(void) USBshutdown()
{
	try
	{
		nat_usb->USBwrap->Shutdown();
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
	}
	//// Yes, we close things in the Shutdown routine, and
	//// don't do anything in the close routine.
	delete nat_usb;
	nat_usb = NULL;

	if (!(nat_R == NULL))
	{
		//Unloading an assembly is impossible
		delete nat_R;
		nat_R = NULL;
	}
}

#define DoAsLilyPad
EXPORT_C_(s32) USBopen(void *pDsp)
{
	System::IntPtr managedHWND = System::IntPtr::Zero;
#ifndef  DoAsLilyPad
	HWND hWnd = (HWND)pDsp;
	if (!IsWindow(hWnd) && !IsBadReadPtr((u32*)hWnd, 4))
		hWnd = *(HWND*)hWnd;
	if (!IsWindow(hWnd))
		hWnd = NULL;
	else
	{
		while (GetWindowLong(hWnd, GWL_STYLE) & WS_CHILD)
			hWnd = GetParent(hWnd);
	}
	managedHWND = System::IntPtr(hWnd);
#else
	//Code Taken from Lilypad, allowing both of us to read RawAPI at the same time
	HWND hWnd;
	if (IsWindow((HWND)pDsp)) {
		hWnd = (HWND)pDsp;
	}
	else if (pDsp && !IsBadReadPtr(pDsp, 4) && IsWindow(*(HWND*)pDsp)) {
		hWnd = *(HWND*)pDsp;
	}
	else {
		//Error Bad Handle
		return -1;
	}
	
	managedHWND = System::IntPtr(hWnd);
#endif
	
	try
	{
		return nat_usb->USBwrap->USBopen(managedHWND);
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
		return -1;
	}
	// Take care of anything else we need on opening, other then initialization.
	//return 0;
}

EXPORT_C_(void) USBclose()
{
	nat_usb->USBwrap->Close();
	//USBLog.WriteLn("Closing USBnull.");
}

EXPORT_C_(u8) USBread8(u32 addr)
{
	return nat_usb->USBwrap->USBread8(addr);
	//u8 value = 0;

	//switch(addr)
	//{
	//	// Handle any appropriate addresses here.
	//	case 0x1f801600:
	//		//USBLog.WriteLn("(USBnull) 8 bit read at address %lx", addr);
	//	break;

	//	default:
	//		//value = usbRu8(addr);
	//		//USBLog.WriteLn("*(USBnull) 8 bit read at address %lx", addr);
	//		break;
	//}
	//return value;
}

EXPORT_C_(u16) USBread16(u32 addr)
{
	return nat_usb->USBwrap->USBread16(addr);
	//u16 value = 0;

	//switch(addr)
	//{
	//	// Handle any appropriate addresses here.
	//	case 0x1f801600:
	//		//USBLog.WriteLn("(USBnull) 16 bit read at address %lx", addr);
	//	break;

	//	default:
	//		value = usbRu16(addr);
	//		//USBLog.WriteLn("(USBnull) 16 bit read at address %lx", addr);
	//}
	//return value;
}

EXPORT_C_(u32) USBread32(u32 addr)
{
	return nat_usb->USBwrap->USBread32(addr);
	//u32 value = 0;

	//switch(addr)
	//{
	//	// Handle any appropriate addresses here.
	//	case 0x1f801600:
	//		//USBLog.WriteLn("(USBnull) 32 bit read at address %lx", addr);
	//	break;

	//	default:
	//		//value = usbRu32(addr);
	//		//USBLog.WriteLn("(USBnull) 32 bit read at address %lx", addr);
	//}
	//return value;
}

EXPORT_C_(void) USBwrite8(u32 addr, u8 value)
{
	nat_usb->USBwrap->USBwrite8(addr, value);
	//switch(addr)
	//{
	//	// Handle any appropriate addresses here.
	//	case 0x1f801600:
	//		//USBLog.WriteLn("(USBnull) 8 bit write at address %lx value %x", addr, value);
	//	break;

	//	default:
	//		//usbRu8(addr) = value;
	//		//USBLog.WriteLn("(USBnull) 8 bit write at address %lx value %x", addr, value);
	//}
}

EXPORT_C_(void) USBwrite16(u32 addr, u16 value)
{
	nat_usb->USBwrap->USBwrite16(addr, value);
	//switch(addr)
	//{
	//	// Handle any appropriate addresses here.
	//	case 0x1f801600:
	//		//USBLog.WriteLn("(USBnull) 16 bit write at address %lx value %x", addr, value);
	//	break;

	//	default:
	//		//usbRu16(addr) = value;
	//		//USBLog.WriteLn("(USBnull) 16 bit write at address %lx value %x", addr, value);
	//}
}

EXPORT_C_(void) USBwrite32(u32 addr, u32 value)
{
	nat_usb->USBwrap->USBwrite32(addr, value);
	//switch(addr)
	//{
	//	// Handle any appropriate addresses here.
	//	case 0x1f801600:
	//		//USBLog.WriteLn("(USBnull) 16 bit write at address %lx value %x", addr, value);
	//	break;

	//	default:
	//		//usbRu32(addr) = value;
	//		//USBLog.WriteLn("(USBnull) 32 bit write at address %lx value %x", addr, value);
	//}
}

EXPORT_C_(void) USBirqCallback(USBcallback callback)
{
	// Register USBirq, so we can trigger an interrupt with it later.
	// It will be called as USBirq(cycles); where cycles is the number
	// of cycles before the irq is triggered.

	nat_usb->USBwrap->USBirqCallback(callback);
}

EXPORT_C_(int) _USBirqHandler(void)
{
	// This is our USB irq handler, so if an interrupt gets triggered,
	// deal with it here.
	//return 0;
	return nat_usb->USBwrap->_USBirqHandler();
	//return clr_usb->clr_USB->_USBirqHandler();
}

EXPORT_C_(USBhandler) USBirqHandler(void)
{
	// Pass our handler to pcsx2.
	return nat_usb->USBwrap->USBirqHandler();
}

EXPORT_C_(void) USBsetRAM(void *mem)
{
	ram = (u8*)mem; //keep track of mem address

	nat_usb->USBwrap->USBsetRAM((u8*)mem);
}

EXPORT_C_(void) USBsetSettingsDir(const char* dir)
{
	// Get the path to the ini directory.
	try
	{
		EnsureInitialized();
		USBCLRInit();

		nat_usb->USBwrap->SetSettingsDir(gcnew System::String(dir));
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
	}
}
EXPORT_C_(void) USBsetLogDir(const char* dir)
{
		//// Get the path to the log directory.
	try
	{
		EnsureInitialized();
		USBCLRInit();

		nat_usb->USBwrap->SetLogDir(gcnew System::String(dir));
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
	}
}

// extended funcs

EXPORT_C_(s32) USBfreeze(int mode, freezeData *data)
{
	// This should store or retrieve any information, for if emulation
	// gets suspended, or for savestates.
	if (mode == FREEZE_LOAD)
	{
		//When a diffrent type of plugin gets swapped, this plugin gets shutdown/reloaded
		//during shutdown, the CLR plugin gets unloaded and loses its refrence to 
		//IOP memory
		if (!(ram == NULL))
		{
			nat_usb->USBwrap->USBsetRAM(ram);
		}
	}
	return nat_usb->USBwrap->Freeze(mode, data);
}

EXPORT_C_(void) USBasync(u32 cycles)
{
// Optional function: Called in IopCounter.cpp.
	nat_usb->USBwrap->USBasync(cycles);
}

EXPORT_C_(s32) USBtest()
{
	try
	{
		EnsureInitialized();
		USBCLRInit();
		// 0 if the plugin works, non-0 if it doesn't.
		//return 0;
		return nat_usb->USBwrap->Test();
	}
	catch (System::Exception^ e)
	{
		System::Windows::Forms::MessageBox::Show(e->Message + System::Environment::NewLine + e->StackTrace);
		return -1;
	}
}