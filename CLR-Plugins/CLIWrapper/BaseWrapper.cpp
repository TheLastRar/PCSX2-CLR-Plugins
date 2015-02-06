//void CALLBACK USBasync(u32 cycles); is missing
//void CALLBACK USBconfigure(); also missing //used in config.h
//void CALLBACK USBabout(); also missing //used in config.h

#include <msclr\marshal_cppstd.h>
using namespace std;
#include "BaseWrapper.h"

using namespace System::Runtime::InteropServices;
using namespace System::Windows;

//Logstuff
bool BaseWrapper::Log_open(System::String^ logname)
{
	std::string logname2 = msclr::interop::marshal_as< std::string >(logname);
	return Log->Open(logname2);
}
void BaseWrapper::Log_close()
{
	Log->Close();
}
void BaseWrapper::Log_write(System::String^ fmt)
{
	Log->Write((const char*)Marshal::StringToHGlobalAnsi(fmt).ToPointer());
}
void BaseWrapper::Log_writeln(System::String^ fmt)
{
	Log->WriteLn((const char*)Marshal::StringToHGlobalAnsi(fmt).ToPointer());
}
void BaseWrapper::Log_set_WriteToFile(bool par)
{
	Log->WriteToFile = par;
}

BaseWrapper::BaseWrapper()
{
}

s32 BaseWrapper::Init()
{
	return (s32)mInit->Invoke(cPlugin, gcnew array < System::Object^ > {PSEWrapper::revision, PSEWrapper::build});
}
void BaseWrapper::Shutdown()
{
	mShutdown->Invoke(cPlugin, nullptr);
	//// Yes, we close things in the Shutdown routine, and
	//// don't do anything in the close routine.
	Config->ConfigFree();
	cPlugin = nullptr;
	Log->Close();
	delete Log;
	Log = nullptr;
}

void BaseWrapper::Close()
{
	mClose->Invoke(cPlugin, nullptr);
}

void BaseWrapper::SetSettingsDir(System::String^ dir)
{
	// Get the path to the ini directory.
	//CLRInit();
	mSetSettingsDir->Invoke(cPlugin, gcnew array < System::Object^ > {dir});
}
void BaseWrapper::SetLogDir(System::String^ dir)
{
	//// Get the path to the log directory.
	//
	//// Reload the log file after updated the path
	//CLRInit();
	mSetLogDir->Invoke(cPlugin, gcnew array < System::Object^ > {dir});
}

// extended funcs

s32 BaseWrapper::Freeze(int mode, freezeData *data)
{
	// freezeData is a struct that contains a byte* array and the size of said array

	// This should store or retrieve any information, for if emulation
	// gets suspended, or for savestates.

	array<System::Byte>^ freezeArray;
	array<System::Object^>^ param = gcnew array<System::Object^>(1);
	switch (mode)
	{
	case FREEZE_LOAD: //we get given an empty save file for some reason
		// Load previously saved data.
		freezeArray = gcnew array<System::Byte>(data->size);

		for (int i = 0; i < freezeArray->Length; ++i)
			freezeArray[i] = data->data[i];
		param[0] = freezeArray;
		mFreezeLoad->Invoke(cPlugin, param);
		break;
	case FREEZE_SAVE:
		// Save data.
		if (data->data == NULL)
			return -1;
		freezeArray = (array<System::Byte>^) mFreezeSave->Invoke(cPlugin, nullptr);
		for (int i = 0; i < freezeArray->Length; ++i)
			data->data[i] = freezeArray[i];
		return 1;
		break;
	case FREEZE_SIZE:
		// return the size of the data.
		data->size = (int)mFreezeSize->Invoke(cPlugin, nullptr);
		return 1;
		break;
	}
	return 0;
}

s32 BaseWrapper::Test()
{
	return (s32)mTest->Invoke(cPlugin, nullptr);
}
