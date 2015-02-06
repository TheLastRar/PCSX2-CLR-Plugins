#ifndef __CONWRAP_H__
#define __CONWRAP_H__

#include "PS2Edefs.h"
#include "PS2Eext.h"
#include "PSEWrapper.h"

ref class ConfigWrapper
{
private:
	PluginConf^ Ini = gcnew PluginConf();
public:
	System::Object^ cConf;

	System::Reflection::MethodInfo^ mAbout;
	System::Reflection::MethodInfo^ mConfig;
private:
	bool Conf_open(System::String^ filename, bool WriteAccess);
	void Conf_close();
	int Conf_readint(System::String^ item, int defval);
	void Conf_writeint(System::String^ item, int val);

public:
	ConfigWrapper();
	void ConfigFree();
	void About();
	void Configure();
};

#endif
