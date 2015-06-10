/*
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

#include <stdlib.h>
#include <string>
using namespace std;
#include "DEV9interface.h"

using namespace System::Runtime::InteropServices;
using namespace System::Reflection;
NativeDEV9Wrapper* nat_dev9;

void DEV9CLRInit()
{
	if (nat_dev9 == NULL)
	{
		nat_dev9 = new NativeDEV9Wrapper();
		nat_dev9->DEV9wrap = gcnew DEV9Wrapper();
	}
}

EXPORT_C_(s32) DEV9init()
{
	//CLRInit();
	try
	{
		return nat_dev9->DEV9wrap->Init();
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
		return -1;
	}
}

EXPORT_C_(void) DEV9shutdown()
{
	try
	{
		nat_dev9->DEV9wrap->Shutdown();
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
	}
	//// Yes, we close things in the Shutdown routine, and
	//// don't do anything in the close routine.
	if (!(nat_dev9 == NULL))
	{
		delete nat_dev9;
		nat_dev9 = NULL;
	}
	if (!(nat_R == NULL))
	{
		//Unloading an assembly is impossible
		delete nat_R;
		nat_R = NULL;
	}
}

EXPORT_C_(s32) DEV9open(void *pDsp)
{
	//Is a pointer to IOP PC
	System::IntPtr managedHWND = System::IntPtr::Zero;

	managedHWND = System::IntPtr((u32*)pDsp); //bah

	try
	{
		return nat_dev9->DEV9wrap->DEV9open(managedHWND);
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
		return -1;
	}
	// Take care of anything else we need on opening, other then initialization.
	//return 0;
}

EXPORT_C_(void) DEV9close()
{
	nat_dev9->DEV9wrap->Close();
}

EXPORT_C_(u8) DEV9read8(u32 addr)
{
	return nat_dev9->DEV9wrap->DEV9read8(addr);
}

EXPORT_C_(u16) DEV9read16(u32 addr)
{
	return nat_dev9->DEV9wrap->DEV9read16(addr);
}

EXPORT_C_(u32) DEV9read32(u32 addr)
{
	return nat_dev9->DEV9wrap->DEV9read32(addr);
}

EXPORT_C_(void) DEV9write8(u32 addr, u8 value)
{
	nat_dev9->DEV9wrap->DEV9write8(addr, value);
}

EXPORT_C_(void) DEV9write16(u32 addr, u16 value)
{
	nat_dev9->DEV9wrap->DEV9write16(addr, value);
}

EXPORT_C_(void) DEV9write32(u32 addr, u32 value)
{
	nat_dev9->DEV9wrap->DEV9write32(addr, value);
}

EXPORT_C_(void) DEV9irqCallback(DEV9callback callback)
{
	nat_dev9->DEV9wrap->DEV9irqCallback(callback);
}

EXPORT_C_(void) DEV9async(u32 cycles)
{
	nat_dev9->DEV9wrap->DEV9async(cycles);
}

EXPORT_C_(void) DEV9readDMA8Mem(u32 *pMem, int size)
{
	nat_dev9->DEV9wrap->DEV9readDMA8Mem(pMem, size);
}

EXPORT_C_(void) DEV9writeDMA8Mem(u32 *pMem, int size)
{
	nat_dev9->DEV9wrap->DEV9writeDMA8Mem(pMem, size);
}

EXPORT_C_(int) _DEV9irqHandler(void)
{
	// This is our DEV9 irq handler, so if an interrupt gets triggered,
	// deal with it here.
	//return 0;
	return nat_dev9->DEV9wrap->_DEV9irqHandler();
}

EXPORT_C_(DEV9handler) DEV9irqHandler(void)
{
	// Pass our handler to pcsx2.
	return nat_dev9->DEV9wrap->DEV9irqHandler();
}

EXPORT_C_(void) DEV9setSettingsDir(const char* dir)
{
	// Get the path to the ini directory.
	try
	{
		EnsureInitialized();
		DEV9CLRInit();

		nat_dev9->DEV9wrap->SetSettingsDir(gcnew System::String(dir));
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
	}
}
EXPORT_C_(void) DEV9setLogDir(const char* dir)
{
	//// Get the path to the log directory.
	try
	{
		EnsureInitialized();
		DEV9CLRInit();

		nat_dev9->DEV9wrap->SetLogDir(gcnew System::String(dir));
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
	}
}

// extended funcs

EXPORT_C_(s32) DEV9freeze(int mode, freezeData *data)
{
	// This should store or retrieve any information, for if emulation
	// gets suspended, or for savestates.
	return nat_dev9->DEV9wrap->Freeze(mode, data);
}

EXPORT_C_(s32) DEV9test()
{
	try
	{
		EnsureInitialized();
		DEV9CLRInit();
		// 0 if the plugin works, non-0 if it doesn't.
		//return 0;
		return nat_dev9->DEV9wrap->Test();
	}
	catch (System::Exception^ e)
	{
		System::Windows::Forms::MessageBox::Show(e->Message + System::Environment::NewLine + e->StackTrace);
		return -1;
	}
}
