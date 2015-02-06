#include "AssemblyResolve.h"

using namespace System::Reflection;
using namespace System::Windows;

bool _initialized = false;

static array<System::Byte>^ ToBytes(System::IO::Stream^ instance)
{
	int capacity = instance->CanSeek ? System::Convert::ToInt32(instance->Length) : 0;

	System::IO::MemoryStream^ result = gcnew System::IO::MemoryStream(capacity);

	int readLength = 0;
	array<System::Byte>^ buffer = gcnew array<System::Byte>(4096);

	do {
		readLength = instance->Read(buffer, 0, buffer->Length);
		result->Write(buffer, 0, readLength);
	} while (readLength > 0);

	return result->ToArray();
}
static Assembly^ LoadEmbedAssembly(System::String^ FileName)
{
	System::String^ resourceFullName = FileName;
	Assembly^ thisAssembly = Assembly::GetExecutingAssembly();
	System::IO::Stream^ resource = thisAssembly->GetManifestResourceStream(resourceFullName);
	if (resource != nullptr)
	{
		Assembly^ loadingAssembly = System::Reflection::Assembly::Load(ToBytes(resource));
		return loadingAssembly;
	}
	else
	{
		return nullptr;
	}
}
Assembly^ EmbedREH(System::Object^ sender, System::ResolveEventArgs^ args)
{
	Forms::MessageBox::Show("AResolve");
	return LoadEmbedAssembly(System::String::Format("{0}.dll", args->Name->Split(',')[0]));
}

void EnsureInitialized()
{
	if (nat_R == NULL)
	{
		nat_R = new NativeRWrapper();
		nat_R->Plugin = LoadEmbedAssembly("CLRPLUGIN.dll");
	}
}
