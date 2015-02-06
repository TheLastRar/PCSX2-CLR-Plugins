#ifndef __PSEW_H__
#define __PSEW_H__

#include <msclr\auto_gcroot.h>

#define USBdefs

#include "PS2Edefs.h"
#include "PS2Eext.h"

#define USE_Reflection

delegate void Del_Close();

delegate System::Boolean Log_Open(System::String^ str);
delegate void Log_Write(System::String^ str);
delegate void Log_SetValue(System::Boolean b);

delegate void CLR_CyclesCallback(int cycles);

delegate System::Boolean Config_Open(System::String^ str, System::Boolean b);
delegate int Config_ReadInt(System::String^ str, int def);
delegate void Config_WriteInt(System::String^ str, int def);

class NativeRWrapper
{
private:
	const msclr::auto_gcroot<System::String^> PSEname = "PSE.CLR_PSE";
	const msclr::auto_gcroot<System::String^> Calname = "PSE.CLR_PSE_Callbacks";
	const msclr::auto_gcroot<System::String^> USBname = "PSE.CLR_PSE_USB";
	const msclr::auto_gcroot<System::String^> DEV9name = "PSE.CLR_PSE_DEV9";
	const msclr::auto_gcroot<System::String^> Logname = "PSE.CLR_PSE_PluginLog";
	const msclr::auto_gcroot<System::String^> Conname = "PSE.CLR_PSE_Config";
public:
	msclr::auto_gcroot<System::Reflection::Assembly^> Plugin;
	//System::Object^ CreatePSEInstance()
	//{
	//	System::Type^ tPS = Plugin->GetType(PSEname.get);
	//	return System::Activator::CreateInstance(tPS);
	//}
	System::Reflection::MethodInfo^ GetPSEMethod(System::String^ mName);
	System::Reflection::FieldInfo^ GetPSEField(System::String^ pName);

	System::Delegate^ CreateCallback(System::String^ cName, System::Delegate^ func);

	System::Reflection::MethodInfo^ GetUSBMethod(System::String^ mName);
	System::Reflection::FieldInfo^ GetUSBField(System::String^ pName);

	System::Reflection::MethodInfo^ GetDEV9Method(System::String^ mName);
	System::Reflection::FieldInfo^ GetDEV9Field(System::String^ pName);

	System::Reflection::FieldInfo^ GetLogField(System::String^ pName);

	System::Reflection::MethodInfo^ GetConfigMethod(System::String^ mName);
	System::Reflection::FieldInfo^ GetConfigField(System::String^ pName);
};
extern NativeRWrapper* nat_R;

ref class PSEWrapper
{
private:
	//static unsigned char version = PS2E_USB_VERSION;
public:
	static const unsigned char revision = 0;
	static const unsigned char build = 5;    // increase that with each version
private:

public:
	PSEWrapper();
	static u32 PS2EgetLibType();
	static char* PS2EgetLibName();
	static u32 PS2EgetLibVersion2(u32 type);
};

#endif
