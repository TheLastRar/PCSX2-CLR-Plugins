//void CALLBACK USBasync(u32 cycles); is missing
//void CALLBACK USBconfigure(); also missing //used in config.h
//void CALLBACK USBabout(); also missing //used in config.h

#include <msclr\marshal_cppstd.h>
using namespace std;
#include "USBWrapper.h"

using namespace System::Runtime::InteropServices;
using namespace System::Windows;

//Logstuff
void USBWrapper::usb_callbackwrapper(int cycles)
{
	USBirq(cycles);
}

USBWrapper::USBWrapper()
{
	cPlugin = nat_R->GetPSEMethod("NewUSB")->Invoke(nullptr, nullptr);

	System::Object^ clog = nat_R->GetPSEMethod("NewLog")->Invoke(nullptr, nullptr);
	nat_R->GetLogField("Open")->SetValue(clog, nat_R->CreateCallback("Log_Open", gcnew Log_Open(this, &USBWrapper::Log_open)));
	nat_R->GetLogField("Close")->SetValue(clog, nat_R->CreateCallback("Close", gcnew Del_Close(this, &USBWrapper::Log_close)));
	nat_R->GetLogField("LogWrite")->SetValue(clog, nat_R->CreateCallback("Log_Write", gcnew Log_Write(this, &USBWrapper::Log_write)));
	nat_R->GetLogField("LogWriteLine")->SetValue(clog, nat_R->CreateCallback("Log_Write", gcnew Log_Write(this, &USBWrapper::Log_writeln)));
	nat_R->GetLogField("SetWriteToFile")->SetValue(clog, nat_R->CreateCallback("Log_SetValue", gcnew Log_SetValue(this, &USBWrapper::Log_set_WriteToFile)));

	nat_R->GetUSBField("PluginLog")->SetValue(cPlugin, clog);
	//Pass Config

	Config = gcnew ConfigWrapper();

	nat_R->GetUSBMethod("SetConfig")->Invoke(cPlugin, gcnew array < System::Object^ > {Config->cConf});
	//Create Method Links
	mInit = nat_R->GetUSBMethod("Init");
	mShutdown = nat_R->GetUSBMethod("Shutdown");
	mUSBopen = nat_R->GetUSBMethod("USBopen");
	mClose = nat_R->GetUSBMethod("Close");
	mUSBread8 = nat_R->GetUSBMethod("USBread8");
	mUSBread16 = nat_R->GetUSBMethod("USBread16");
	mUSBread32 = nat_R->GetUSBMethod("USBread32");
	mUSBwrite8 = nat_R->GetUSBMethod("USBwrite8");
	mUSBwrite16 = nat_R->GetUSBMethod("USBwrite16");
	mUSBwrite32 = nat_R->GetUSBMethod("USBwrite32");
	mUSBirqCallback = nat_R->GetUSBMethod("USBirqCallback");
	m_USBirqHandler = nat_R->GetUSBMethod("_USBirqHandler");

	mUSBsetRam = nat_R->GetUSBMethod("USBsetRam");
	mSetSettingsDir = nat_R->GetUSBMethod("SetSettingsDir");
	mSetLogDir = nat_R->GetUSBMethod("SetLogDir");
	mFreezeLoad = nat_R->GetUSBMethod("FreezeLoad");
	mFreezeSave = nat_R->GetUSBMethod("FreezeSave");
	mFreezeSize = nat_R->GetUSBMethod("FreezeSize");
	mUSBasync = nat_R->GetUSBMethod("USBasync");
	mTest = nat_R->GetUSBMethod("Test");
}

void USBWrapper::Shutdown()
{
	BaseWrapper::Shutdown();
	if (gch.IsAllocated)
	{
		gch.Free();
	}
}
s32 USBWrapper::USBopen(System::IntPtr managedHWND)
{
	return (s32)mUSBopen->Invoke(cPlugin, gcnew array < System::Object^ > {managedHWND});
	// Take care of anything else we need on opening, other then initialization.
}

u8 USBWrapper::USBread8(u32 addr)
{
	return (u8)mUSBread8->Invoke(cPlugin, gcnew array < System::Object^ > {addr});
}

u16 USBWrapper::USBread16(u32 addr)
{
	return (u16)mUSBread16->Invoke(cPlugin, gcnew array < System::Object^ > {addr});
}

u32 USBWrapper::USBread32(u32 addr)
{
	return (u32)mUSBread32->Invoke(cPlugin, gcnew array < System::Object^ > {addr});
}

void USBWrapper::USBwrite8(u32 addr, u8 value)
{
	mUSBwrite8->Invoke(cPlugin, gcnew array < System::Object^ > {addr, value});
}

void USBWrapper::USBwrite16(u32 addr, u16 value)
{
	mUSBwrite16->Invoke(cPlugin, gcnew array < System::Object^ > {addr, value});
}

void USBWrapper::USBwrite32(u32 addr, u32 value)
{
	mUSBwrite32->Invoke(cPlugin, gcnew array < System::Object^ > {addr, value});
}

void USBWrapper::USBirqCallback(USBcallback callback)
{
	// Register USBirq, so we can trigger an interrupt with it later.
	// It will be called as USBirq(cycles); where cycles is the number
	// of cycles before the irq is triggered.
	USBirq = callback;
	System::Delegate^ clrusbcallback = nat_R->CreateCallback("CLR_CyclesCallback", gcnew CLR_CyclesCallback(this, &USBWrapper::usb_callbackwrapper));
	mUSBirqCallback->Invoke(cPlugin, gcnew array < System::Object^ > {clrusbcallback});
}

USBhandler USBWrapper::USBirqHandler(void) //this dosn't work
{
	// Pass our handler to pcsx2.
	if (gch.IsAllocated)
	{
		gch.Free(); //allow garbage collection
	}
	Log->WriteLn("Get IRQ");
	_USBirqDelegate ^fp = gcnew _USBirqDelegate(this, &USBWrapper::_USBirqHandler);
	gch = GCHandle::Alloc(fp); //prevent GC
	System::IntPtr ip = Marshal::GetFunctionPointerForDelegate(fp);
	USBhandler usbH = static_cast<USBhandler>(ip.ToPointer());
	return usbH;
}

int USBWrapper::_USBirqHandler(void)
{
	// This is our USB irq handler, so if an interrupt gets triggered,
	// deal with it here.
	return (int)m_USBirqHandler->Invoke(cPlugin, nullptr);
}

const s64 IOP_MAIN_MEM_SIZE = 1024 * 1024 * 2; //2mb memory

void USBWrapper::USBsetRAM(u8 *mem)
{
	System::IO::UnmanagedMemoryStream ^ cMem = gcnew System::IO::UnmanagedMemoryStream(mem, IOP_MAIN_MEM_SIZE, IOP_MAIN_MEM_SIZE, System::IO::FileAccess::ReadWrite);
	mUSBsetRam->Invoke(cPlugin, gcnew array < System::Object^ > {cMem});
}

void USBWrapper::USBasync(u32 cycles)
{
	mUSBasync->Invoke(cPlugin, gcnew array < System::Object^ > {cycles});
}
