#include <Windows.h>

void __stdcall ResumeThisProcess(HINSTANCE g_hDLL)
{

}

int __stdcall DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpReserved)
{
	/*if (fdwReason == DLL_PROCESS_ATTACH)
	{
		CloseHandle(CreateThread(nullptr, 0, (LPTHREAD_START_ROUTINE)ResumeThisProcess, hinstDLL, 0, nullptr));
	}*/

	switch (fdwReason)
	{
	case DLL_PROCESS_ATTACH:
		MessageBoxA(nullptr, "Confirmed injection.", "DllMain", MB_OK);
		break;

	case DLL_PROCESS_DETACH:
		break;

	case DLL_THREAD_ATTACH:
		break;

	case DLL_THREAD_DETACH:
		break;

	default:
		break;
	}

	return true;
}