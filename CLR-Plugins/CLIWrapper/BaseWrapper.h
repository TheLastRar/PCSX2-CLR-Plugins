#ifndef __BASEWRAP_H__
#define __BASEWRAP_H__

//#include "PS2Edefs.h"
//#include "PS2Eext.h"
#include "ConfigWrapper.h"
//#include "PSEWrapper.h"

ref class BaseWrapper
{
protected:
	System::Object^ cPlugin;

	System::Reflection::MethodInfo^ mInit;
	System::Reflection::MethodInfo^ mShutdown;

	System::Reflection::MethodInfo^ mClose;
	System::Reflection::MethodInfo^ mSetLogDir;
	System::Reflection::MethodInfo^ mSetSettingsDir;
	System::Reflection::MethodInfo^ mFreezeSave;
	System::Reflection::MethodInfo^ mFreezeLoad;
	System::Reflection::MethodInfo^ mFreezeSize;
	System::Reflection::MethodInfo^ mTest;

	PluginLog* Log = new PluginLog();

public:
	ConfigWrapper ^ Config;
protected:
	//Logstuff
	bool Log_open(System::String^ logname);
	void Log_close();
	void Log_write(System::String^ fmt);
	void Log_writeln(System::String^ fmt);
	void Log_set_WriteToFile(bool par);
public:
	BaseWrapper();

	s32 Init();
	void Shutdown();

	//s32 USBopen(System::IntPtr managedHWND);
	void Close();
	void SetSettingsDir(System::String^ dir);
	void SetLogDir(System::String^ dir);
	s32 Freeze(int mode, freezeData *data);
	s32 Test();
};

#endif
