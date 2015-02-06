#include <msclr\marshal_cppstd.h>
using namespace std;
#include "ConfigWrapper.h"

using namespace System::Runtime::InteropServices;
using namespace System::Windows;

bool ConfigWrapper::Conf_open(System::String^ filename, bool WriteAccess)
{
	std::string logname2 = msclr::interop::marshal_as< std::string >(filename);
	if (WriteAccess)
	{
		return Ini->Open(logname2,FileMode::WRITE_FILE);
	}
	else {
		return Ini->Open(logname2, FileMode::READ_FILE);
	}
}
void ConfigWrapper::Conf_close()
{
	Ini->Close();
}
int ConfigWrapper::Conf_readint(System::String^ item, int defval)
{
	std::string logname2 = msclr::interop::marshal_as< std::string >(item);
	return Ini->ReadInt(logname2, defval);
}
void ConfigWrapper::Conf_writeint(System::String^ item, int val)
{
	std::string logname2 = msclr::interop::marshal_as< std::string >(item);
	Ini->WriteInt(logname2, val);
}

ConfigWrapper::ConfigWrapper()
{
	//Init Plugin Config with needed callbacks

	cConf = nat_R->GetPSEMethod("NewConfig")->Invoke(nullptr, nullptr);

	nat_R->GetConfigField("Open")->SetValue(cConf, nat_R->CreateCallback("Config_Open", gcnew Config_Open(this, &ConfigWrapper::Conf_open)));
	nat_R->GetConfigField("Close")->SetValue(cConf, nat_R->CreateCallback("Close", gcnew Del_Close(this, &ConfigWrapper::Conf_close)));
	nat_R->GetConfigField("ReadInt")->SetValue(cConf, nat_R->CreateCallback("Config_ReadInt", gcnew Config_ReadInt(this, &ConfigWrapper::Conf_readint)));
	nat_R->GetConfigField("WriteInt")->SetValue(cConf, nat_R->CreateCallback("Config_WriteInt", gcnew Config_WriteInt(this, &ConfigWrapper::Conf_writeint)));
	//Create Method Links
	mAbout = nat_R->GetConfigMethod("About");
	mConfig = nat_R->GetConfigMethod("Configure");
}
//called on shutdown
void ConfigWrapper::ConfigFree()
{
	try
	{
		Ini->Close();
	}
	catch (System::Exception^ e)
	{
		System::Windows::Forms::MessageBox::Show("WTF? " + e->ToString());
	}		
}

void ConfigWrapper::About()
{
	mAbout->Invoke(cConf, nullptr);
}

void ConfigWrapper::Configure()
{
	mConfig->Invoke(cConf, nullptr);
}