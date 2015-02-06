#ifndef __USBWRAP_H__
#define __USBWRAP_H__

#define USBdefs

#include "PS2Edefs.h"
#include "PS2Eext.h"
#include "ConfigWrapper.h"
#include "PSEWrapper.h"
#include "BaseWrapper.h"

ref class USBWrapper : BaseWrapper
{
private:
	void usb_callbackwrapper(int cycles);

	System::Reflection::MethodInfo^ mUSBopen;
	System::Reflection::MethodInfo^ mUSBread8;
	System::Reflection::MethodInfo^ mUSBread16;
	System::Reflection::MethodInfo^ mUSBread32;
	System::Reflection::MethodInfo^ mUSBwrite8;
	System::Reflection::MethodInfo^ mUSBwrite16;
	System::Reflection::MethodInfo^ mUSBwrite32;
	System::Reflection::MethodInfo^ mUSBirqCallback;
	System::Reflection::MethodInfo^ m_USBirqHandler;
	//non _ version
	System::Reflection::MethodInfo^ mUSBsetRam;
	System::Reflection::MethodInfo^ mUSBasync;

	USBcallback USBirq;
	System::Runtime::InteropServices::GCHandle gch;

	delegate int _USBirqDelegate(void);
public:
	USBWrapper();

	void Shutdown();
	s32 USBopen(System::IntPtr managedHWND);

	u8 USBread8(u32 addr);
	u16 USBread16(u32 addr);
	u32 USBread32(u32 addr);
	void USBwrite8(u32 addr, u8 value);
	void USBwrite16(u32 addr, u16 value);
	void USBwrite32(u32 addr, u32 value);
	void USBirqCallback(USBcallback callback);

	USBhandler USBirqHandler(void);
	int _USBirqHandler(void);

	void USBsetRAM(u8 *mem);
	void USBasync(u32 cycles);
};

#endif
