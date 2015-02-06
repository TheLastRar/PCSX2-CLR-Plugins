//void CALLBACK USBasync(u32 cycles); is missing
//void CALLBACK USBconfigure(); also missing //used in config.h
//void CALLBACK USBabout(); also missing //used in config.h

//#include <msclr\marshal_cppstd.h>
using namespace std;
#include "PSEWrapper.h"

using namespace System::Runtime::InteropServices;
using namespace System::Windows;

NativeRWrapper* nat_R;

PSEWrapper::PSEWrapper()
{
	;
}

u32 PSEWrapper::PS2EgetLibType()
{
	int ret = (int)nat_R->GetPSEMethod("PS2EgetLibType")->Invoke(nullptr, nullptr);
	switch (ret)
	{
	case 1: //GS
		return PS2E_LT_GS;
		break;
	case 2: //PAD
		return PS2E_LT_PAD;
		break;
	case 4: //SPU2
		return PS2E_LT_SPU2;
		break;
	case 8: //CDVD
		return PS2E_LT_CDVD;
		break;
	case 16: //USB
		return PS2E_LT_USB;
		break;
	case 32: //FW
		return PS2E_LT_FW;
		break;
	case 64: //DEV9
		return PS2E_LT_DEV9;
		break;
	default:
		return 0;
		break;
	}
}
char* PSEWrapper::PS2EgetLibName()
{
	return (char*)Marshal::StringToHGlobalAnsi((System::String^)nat_R->GetPSEMethod("PS2EgetLibName")->Invoke(nullptr, nullptr)).ToPointer();
}
u32 PSEWrapper::PS2EgetLibVersion2(u32 type)
{
	int version = 0;
	switch (type)
	{
	case PS2E_LT_GS:
		version = PS2E_GS_VERSION;
		break;
	case PS2E_LT_PAD:
		version = PS2E_PAD_VERSION;
		break;
	case PS2E_LT_SPU2:
		version = PS2E_SPU2_VERSION;
		break;
	case PS2E_LT_CDVD:
		version = PS2E_CDVD_VERSION;
		break;
	case PS2E_LT_USB:
		version = PS2E_USB_VERSION;
		break;
	case PS2E_LT_FW:
		version = PS2E_FW_VERSION;
		break;
	case PS2E_LT_DEV9:
		version = PS2E_DEV9_VERSION;
		break;
	default:
		break;
	}
	unsigned char rev = (unsigned char)nat_R->GetPSEField("revision")->GetValue(nullptr);
	unsigned char bui = (unsigned char)nat_R->GetPSEField("build")->GetValue(nullptr);
	return (version << 16) | (rev << 8) | bui;
}

//NativeRWrapper

System::Reflection::MethodInfo^ NativeRWrapper::GetPSEMethod(System::String^ mName)
{
	System::Type^ tPS = Plugin->GetType(PSEname.get());
	System::Reflection::MethodInfo^ mi = tPS->GetMethod(mName);
	return mi;
}
System::Reflection::FieldInfo^ NativeRWrapper::GetPSEField(System::String^ pName)
{
	System::Type^ tPS = Plugin->GetType(PSEname.get());
	return tPS->GetField(pName);
}

System::Delegate^ NativeRWrapper::CreateCallback(System::String^ cName, System::Delegate^ func)
{
	System::Reflection::MethodInfo^ mi = func->Method;
	System::Type^ tPS = Plugin->GetType(Calname.get() + "." + cName);
	return System::Delegate::CreateDelegate(tPS, func->Target, mi, true);
}

System::Reflection::MethodInfo^ NativeRWrapper::GetUSBMethod(System::String^ mName)
{
	System::Type^ tPS = Plugin->GetType(USBname.get());
	System::Reflection::MethodInfo^ mi = tPS->GetMethod(mName);
	return mi;
}
System::Reflection::FieldInfo^ NativeRWrapper::GetUSBField(System::String^ pName)
{
	System::Type^ tPS = Plugin->GetType(USBname.get());
	return tPS->GetField(pName);
}

System::Reflection::MethodInfo^ NativeRWrapper::GetDEV9Method(System::String^ mName)
{
	System::Type^ tPS = Plugin->GetType(DEV9name.get());
	System::Reflection::MethodInfo^ mi = tPS->GetMethod(mName);
	return mi;
}
System::Reflection::FieldInfo^ NativeRWrapper::GetDEV9Field(System::String^ pName)
{
	System::Type^ tPS = Plugin->GetType(DEV9name.get());
	return tPS->GetField(pName);
}

System::Reflection::FieldInfo^ NativeRWrapper::GetLogField(System::String^ pName)
{
	System::Type^ tPS = Plugin->GetType(Logname.get());
	return tPS->GetField(pName);
}

System::Reflection::MethodInfo^ NativeRWrapper::GetConfigMethod(System::String^ mName)
{
	System::Type^ tPS = Plugin->GetType(Conname.get());
	System::Reflection::MethodInfo^ mi = tPS->GetMethod(mName);
	return mi;
}
System::Reflection::FieldInfo^ NativeRWrapper::GetConfigField(System::String^ pName)
{
	System::Type^ tPS = Plugin->GetType(Conname.get());
	return tPS->GetField(pName);
}
