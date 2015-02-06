#include <msclr\marshal_cppstd.h>
using namespace std;
#include "PSEinterface.h"

using namespace System::Runtime::InteropServices;
using namespace System::Reflection;

void MsgBoxError(System::Exception^ e)
{
	if (e->InnerException == nullptr)
	{
		System::Windows::Forms::MessageBox::Show("Encounted Exception! : " + e->Message + System::Environment::NewLine + e->StackTrace);
	}
	else
	{
		System::Windows::Forms::MessageBox::Show("Encounted Exception! : " + System::Environment::NewLine + e->Message + System::Environment::NewLine +
			System::Environment::NewLine + "With InnerException : " + e->InnerException->Message + System::Environment::NewLine + e->InnerException->StackTrace);
	}
}

EXPORT_C_(u32) PS2EgetLibType()
{
	try
	{
		EnsureInitialized();
		return PSEWrapper::PS2EgetLibType();
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
		return 0;
	}
}

EXPORT_C_(char*) PS2EgetLibName()
{
	try
	{
		EnsureInitialized();
		//snprintf( libraryName, 255, "USBnull Driver");

		return PSEWrapper::PS2EgetLibName();
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
		return NULL;
	}
}

EXPORT_C_(u32) PS2EgetLibVersion2(u32 type)
{
	try
	{
		EnsureInitialized();

		return PSEWrapper::PS2EgetLibVersion2(type);
	}
	catch (System::Exception^ e)
	{
		MsgBoxError(e);
		return 0; //confuses PCXS2 but dosn't crash/hang
	}

}

/* For operating systems that need an entry point for a dll/library, here it is. Defined in PS2Eext.h. */
#pragma unmanaged
ENTRY_POINT;
#pragma managed
