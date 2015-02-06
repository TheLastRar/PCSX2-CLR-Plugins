//void CALLBACK DEV9configure(); also missing //used in config.h
//void CALLBACK DEV9about(); also missing //used in config.h

#include <msclr\marshal_cppstd.h>
using namespace std;
#include "DEV9Wrapper.h"

using namespace System::Runtime::InteropServices;
using namespace System::Windows;

void DEV9Wrapper::dev9_callbackwrapper(int cycles)
{
	DEV9irq(cycles);
}

DEV9Wrapper::DEV9Wrapper()
{
	cPlugin = nat_R->GetPSEMethod("NewDEV9")->Invoke(nullptr, nullptr);

	System::Object^ clog = nat_R->GetPSEMethod("NewLog")->Invoke(nullptr, nullptr);
	nat_R->GetLogField("Open")->SetValue(clog, nat_R->CreateCallback("Log_Open", gcnew Log_Open(this, &DEV9Wrapper::Log_open)));
	nat_R->GetLogField("Close")->SetValue(clog, nat_R->CreateCallback("Close", gcnew Del_Close(this, &DEV9Wrapper::Log_close)));
	nat_R->GetLogField("LogWrite")->SetValue(clog, nat_R->CreateCallback("Log_Write", gcnew Log_Write(this, &DEV9Wrapper::Log_write)));
	nat_R->GetLogField("LogWriteLine")->SetValue(clog, nat_R->CreateCallback("Log_Write", gcnew Log_Write(this, &DEV9Wrapper::Log_writeln)));
	nat_R->GetLogField("SetWriteToFile")->SetValue(clog, nat_R->CreateCallback("Log_SetValue", gcnew Log_SetValue(this, &DEV9Wrapper::Log_set_WriteToFile)));

	nat_R->GetDEV9Field("PluginLog")->SetValue(cPlugin, clog);
	//Pass Config

	Config = gcnew ConfigWrapper();

	nat_R->GetDEV9Method("SetConfig")->Invoke(cPlugin, gcnew array < System::Object^ > {Config->cConf});
	//Create Method Links
	mInit = nat_R->GetDEV9Method("Init");
	mShutdown = nat_R->GetDEV9Method("Shutdown");
	mDEV9open = nat_R->GetDEV9Method("DEV9open");
	mClose = nat_R->GetDEV9Method("Close");
	mDEV9read8 = nat_R->GetDEV9Method("DEV9read8");
	mDEV9read16 = nat_R->GetDEV9Method("DEV9read16");
	mDEV9read32 = nat_R->GetDEV9Method("DEV9read32");
	mDEV9write8 = nat_R->GetDEV9Method("DEV9write8");
	mDEV9write16 = nat_R->GetDEV9Method("DEV9write16");
	mDEV9write32 = nat_R->GetDEV9Method("DEV9write32");

	mDEV9readDMA8Mem = nat_R->GetDEV9Method("DEV9readDMA8Mem");
	mDEV9writeDMA8Mem = nat_R->GetDEV9Method("DEV9writeDMA8Mem");

	mDEV9irqCallback = nat_R->GetDEV9Method("DEV9irqCallback");
	m_DEV9irqHandler = nat_R->GetDEV9Method("_DEV9irqHandler");

	mSetSettingsDir = nat_R->GetDEV9Method("SetSettingsDir");
	mSetLogDir = nat_R->GetDEV9Method("SetLogDir");
	mFreezeLoad = nat_R->GetDEV9Method("FreezeLoad");
	mFreezeSave = nat_R->GetDEV9Method("FreezeSave");
	mFreezeSize = nat_R->GetDEV9Method("FreezeSize");
	mTest = nat_R->GetDEV9Method("Test");
}

void DEV9Wrapper::Shutdown()
{
	BaseWrapper::Shutdown();
	if (gch.IsAllocated)
	{
		gch.Free();
	}
}
s32 DEV9Wrapper::DEV9open(System::IntPtr managedHWND)
{
	return (s32)mDEV9open->Invoke(cPlugin, gcnew array < System::Object^ > {managedHWND});
	// Take care of anything else we need on opening, other then initialization.
}

u8 DEV9Wrapper::DEV9read8(u32 addr)
{
	return (u8)mDEV9read8->Invoke(cPlugin, gcnew array < System::Object^ > {addr});
}

u16 DEV9Wrapper::DEV9read16(u32 addr)
{
	return (u16)mDEV9read16->Invoke(cPlugin, gcnew array < System::Object^ > {addr});
}

u32 DEV9Wrapper::DEV9read32(u32 addr)
{
	return (u32)mDEV9read32->Invoke(cPlugin, gcnew array < System::Object^ > {addr});
}

void DEV9Wrapper::DEV9write8(u32 addr, u8 value)
{
	mDEV9write8->Invoke(cPlugin, gcnew array < System::Object^ > {addr, value});
}

void DEV9Wrapper::DEV9write16(u32 addr, u16 value)
{
	mDEV9write16->Invoke(cPlugin, gcnew array < System::Object^ > {addr, value});
}

void DEV9Wrapper::DEV9write32(u32 addr, u32 value)
{
	mDEV9write32->Invoke(cPlugin, gcnew array < System::Object^ > {addr, value});
}

void DEV9Wrapper::DEV9readDMA8Mem(u32 *pMem, int size) //check
{
	//System::Console::Error->WriteLine("DEV9readDMA8Mem");
	u8* pMem8 = (u8*)pMem;
	int trueSize = size >> 1;
	System::IO::UnmanagedMemoryStream ^ cMem = gcnew System::IO::UnmanagedMemoryStream(pMem8, (trueSize), (trueSize), System::IO::FileAccess::ReadWrite);
	mDEV9readDMA8Mem->Invoke(cPlugin, gcnew array < System::Object^ > {cMem, size});
	cMem->Close();
}
void DEV9Wrapper::DEV9writeDMA8Mem(u32 *pMem, int size) //check
{
	//System::Console::Error->WriteLine("DEV9writeDMA8Mem");
	u8* pMem8 = (u8*)pMem;
	int trueSize = size >> 1;
	System::IO::UnmanagedMemoryStream ^ cMem = gcnew System::IO::UnmanagedMemoryStream(pMem8, (trueSize), (trueSize), System::IO::FileAccess::ReadWrite);
	mDEV9writeDMA8Mem->Invoke(cPlugin, gcnew array < System::Object^ > {cMem, size});
	cMem->Close();
}

void DEV9Wrapper::DEV9irqCallback(DEV9callback callback)
{
	// Register DEV9irq, so we can trigger an interrupt with it later.
	// It will be called as DEV9irq(cycles); where cycles is the number
	// of cycles before the irq is triggered.
	DEV9irq = callback;
	System::Delegate^ clrdev9callback = nat_R->CreateCallback("CLR_CyclesCallback", gcnew CLR_CyclesCallback(this, &DEV9Wrapper::dev9_callbackwrapper));
	mDEV9irqCallback->Invoke(cPlugin, gcnew array < System::Object^ > {clrdev9callback});
}

DEV9handler DEV9Wrapper::DEV9irqHandler(void) //this dosn't work
{
	// Pass our handler to pcsx2.
	if (gch.IsAllocated)
	{
		gch.Free(); //allow garbage collection
	}
	Log->WriteLn("Get IRQ");
	_DEV9irqDelegate ^fp = gcnew _DEV9irqDelegate(this, &DEV9Wrapper::_DEV9irqHandler);
	gch = GCHandle::Alloc(fp); //prevent GC
	System::IntPtr ip = Marshal::GetFunctionPointerForDelegate(fp);
	DEV9handler dev9H = static_cast<DEV9handler>(ip.ToPointer());
	return dev9H;
}

int DEV9Wrapper::_DEV9irqHandler(void)
{
	// This is our DEV9 irq handler, so if an interrupt gets triggered,
	// deal with it here.
	return (int)m_DEV9irqHandler->Invoke(cPlugin, nullptr);
}
