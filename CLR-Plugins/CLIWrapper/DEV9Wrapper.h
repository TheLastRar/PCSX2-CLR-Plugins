#ifndef __DEV9WRAP_H__
#define __DEV9WRAP_H__

#define DEV9defs

#include "PS2Edefs.h"
#include "PS2Eext.h"
#include "ConfigWrapper.h"
#include "PSEWrapper.h"
#include "BaseWrapper.h"

ref class DEV9Wrapper : BaseWrapper
{
private:
	void dev9_callbackwrapper(int cycles);

	System::Reflection::MethodInfo^ mDEV9open;
	System::Reflection::MethodInfo^ mDEV9read8;
	System::Reflection::MethodInfo^ mDEV9read16;
	System::Reflection::MethodInfo^ mDEV9read32;
	System::Reflection::MethodInfo^ mDEV9write8;
	System::Reflection::MethodInfo^ mDEV9write16;
	System::Reflection::MethodInfo^ mDEV9write32;
	System::Reflection::MethodInfo^ mDEV9irqCallback;
	System::Reflection::MethodInfo^ m_DEV9irqHandler;
	//non _ version
	System::Reflection::MethodInfo^ mDEV9async;
	System::Reflection::MethodInfo^ mDEV9readDMA8Mem;
	System::Reflection::MethodInfo^ mDEV9writeDMA8Mem;

	DEV9callback DEV9irq;
	System::Runtime::InteropServices::GCHandle gch;

	delegate int _DEV9irqDelegate(void);
public:

	DEV9Wrapper();

	void Shutdown();
	s32 DEV9open(System::IntPtr managedHWND);

	u8 DEV9read8(u32 addr);
	u16 DEV9read16(u32 addr);
	u32 DEV9read32(u32 addr);
	void DEV9write8(u32 addr, u8 value);
	void DEV9write16(u32 addr, u16 value);
	void DEV9write32(u32 addr, u32 value);
	void DEV9readDMA8Mem(u32 *pMem, int size);
	void DEV9writeDMA8Mem(u32 *pMem, int size);

	void DEV9irqCallback(DEV9callback callback);
	void DEV9async(u32 cycles);

	DEV9handler DEV9irqHandler(void);
	int _DEV9irqHandler(void);
};

#endif
