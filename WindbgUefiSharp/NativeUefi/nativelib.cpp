#ifdef __cplusplus
extern "C" {
#endif
#include <Uefi.h>
#include <Library/UefiLib.h>

#ifdef __cplusplus
}
#endif
#define EXTERN_C       extern "C"


EXTERN_C void CallExceptionHandler();

EXTERN_C void stall(int multi);

static void abort()
{
	while (TRUE)
	{
		stall(10);
	}
	return;
}

void subfun()
{
	Print(L"subfun cpp!\n");

	return;
}

void subfunthrow()
{
	Print(L"subfunthrow cpp!\n");
	CallExceptionHandler();
	//throw 1;
	return;
}

EXTERN_C int mainexp()
{
	try {
		Print(L"main cpp!\n");
		//__debugbreak();
		//throw 1;

		//subfun();

		subfunthrow();
	}
	catch (...)
	{
		Print(L"EXCEPTION_EXECUTE_HANDLER cpp!\n");
	}
	abort();
	return 0;
}

EXTERN_C void  ConsoleOutputString(CHAR16* Buffer);

EXTERN_C EFI_STATUS OutputStringWrapper(IN CHAR16* buf)
{

	try {

		ConsoleOutputString(buf);
	}
	catch (...)
	{
		Print(L"Exception OutputString Handler\r\n");
	}

	return 0;
}
EXTERN_C void WriteLineReal(IN UINT64 rcx);
EXTERN_C void WriteLineWrapper(IN UINT64 rcx)
{

	try {

		WriteLineReal(rcx);
	}
	catch (...)
	{
		Print(L"Exception WriteLine Handler\r\n");
	}

	return;
}


EXTERN_C
UINTN
InternalPrint(
	IN  CONST CHAR16* Format,
	IN  VA_LIST                          Marker
);

/**
  Prints a formatted Unicode string to the console output device specified by
  ConOut defined in the EFI_SYSTEM_TABLE.

  This function prints a formatted Unicode string to the console output device
  specified by ConOut in EFI_SYSTEM_TABLE and returns the number of Unicode
  characters that printed to ConOut.  If the length of the formatted Unicode
  string is greater than PcdUefiLibMaxPrintBufferSize, then only the first
  PcdUefiLibMaxPrintBufferSize characters are sent to ConOut.
  If Format is NULL, then ASSERT().
  If Format is not aligned on a 16-bit boundary, then ASSERT().
  If gST->ConOut is NULL, then ASSERT().

  @param Format   A Null-terminated Unicode format string.
  @param ...      A Variable argument list whose contents are accessed based
				  on the format string specified by Format.

  @return The number of Unicode characters printed to ConOut.

**/
EXTERN_C UINTN
EFIAPI
Print(
	IN CONST CHAR16* Format,
	...
)
{
	try {
		VA_LIST  Marker;
		UINTN    Return;

		VA_START(Marker, Format);

		Return = InternalPrint(Format, Marker);

		VA_END(Marker);
	}
	catch (...)
	{
		Print(L"Exception Print Handler\r\n");
	}
	return 0;
}

EXTERN_C void
EFIAPI
KdpDprintf(
	IN CONST CHAR16* Format,
	...
)
{
	try {
		VA_LIST  Marker;
		UINTN    Return;

		VA_START(Marker, Format);

		Return = InternalPrint(Format, Marker);

		VA_END(Marker);
	}
	catch (...)
	{
		Print(L"Exception KdpDprintf Handler\r\n");
	}
	return;
}