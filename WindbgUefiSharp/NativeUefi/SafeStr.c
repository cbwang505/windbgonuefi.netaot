#include <Uefi.h>
#include <Library/UefiLib.h>
#include <Base.h>
#include <Library/PrintLib.h>
#include <Library/BaseLib.h>
#include <Library/DebugLib.h>
#include <Library/PcdLib.h>

//
// Print primitives
//
#define PREFIX_SIGN          BIT1
#define PREFIX_BLANK         BIT2
#define LONG_TYPE            BIT4
#define OUTPUT_UNICODE       BIT6
#define FORMAT_UNICODE       BIT8
#define PAD_TO_WIDTH         BIT9
#define ARGUMENT_UNICODE     BIT10
#define PRECISION            BIT11
#define ARGUMENT_REVERSED    BIT12
#define COUNT_ONLY_NO_PRINT  BIT13
#define UNSIGNED_TYPE        BIT14

#define WARNING_STATUS_NUMBER  7
#define ERROR_STATUS_NUMBER    35
/* Computed page size */
#define VSM_PAGE_SIZE  0x1000
#define VSM_PAGE_SIZE_DOUBLE  0x2000


int PcdUefiLibMaxPrintBufferSize = 0x700;
int PcdMaximumAsciiStringLength = 0x700;
int PcdMaximumUnicodeStringLength = 0x380;

static int PAGE_SIZE = VSM_PAGE_SIZE;

size_t inline ALIGN_UP(size_t x)
{
	return ((PAGE_SIZE - 1) & x) ? ((x + PAGE_SIZE) & ~(PAGE_SIZE - 1)) : x;
}

size_t inline ALIGN_UP_FIX(size_t x, size_t y)
{
	return ((y - 1) & x) ? ((x + y) & ~(y - 1)) : x;
}

void  ConsoleOutputString(CHAR16* Buffer);
EFI_STATUS OutputStringWrapper(IN CHAR16* buf);
//
// Safe print checks
//
#define RSIZE_MAX        ( 2*(PcdMaximumUnicodeStringLength)+0x100)
#define ASCII_RSIZE_MAX  ( 2* (PcdMaximumAsciiStringLength)+0x100)



GLOBAL_REMOVE_IF_UNREFERENCED CONST CHAR8  mHexStr[] = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

#define ASSERT_UNICODE_BUFFER(p)\
 ASSERT (p!=NULL)


#define ASSERT_STRING(Expression,Text)\
 if (!(Expression)) { \
      OutputStringWrapper(Text); \
    }


#define SAFE_PRINT_CONSTRAINT_CHECK_STRING(Expression, RetVal,Text)  \
  do { \
    ASSERT (Expression); \
    if (!(Expression)) { \
        OutputStringWrapper(Text); \
      return RetVal; \
    } \
  } while (FALSE)
//
// Longest string: RETURN_WARN_BUFFER_TOO_SMALL => 24 characters plus NUL byte
//
GLOBAL_REMOVE_IF_UNREFERENCED CONST CHAR8  mWarningString[][24 + 1] = {
  "Success",                      //  RETURN_SUCCESS                = 0
  "Warning Unknown Glyph",        //  RETURN_WARN_UNKNOWN_GLYPH     = 1
  "Warning Delete Failure",       //  RETURN_WARN_DELETE_FAILURE    = 2
  "Warning Write Failure",        //  RETURN_WARN_WRITE_FAILURE     = 3
  "Warning Buffer Too Small",     //  RETURN_WARN_BUFFER_TOO_SMALL  = 4
  "Warning Stale Data",           //  RETURN_WARN_STALE_DATA        = 5
  "Warning File System",          //  RETURN_WARN_FILE_SYSTEM       = 6
  "Warning Reset Required",       //  RETURN_WARN_RESET_REQUIRED    = 7
};

//
// Longest string: RETURN_INCOMPATIBLE_VERSION => 20 characters plus NUL byte
//
GLOBAL_REMOVE_IF_UNREFERENCED CONST CHAR8  mErrorString[][20 + 1] = {
  "Load Error",                   //  RETURN_LOAD_ERROR             = 1  | MAX_BIT
  "Invalid Parameter",            //  RETURN_INVALID_PARAMETER      = 2  | MAX_BIT
  "Unsupported",                  //  RETURN_UNSUPPORTED            = 3  | MAX_BIT
  "Bad Buffer Size",              //  RETURN_BAD_BUFFER_SIZE        = 4  | MAX_BIT
  "Buffer Too Small",             //  RETURN_BUFFER_TOO_SMALL,      = 5  | MAX_BIT
  "Not Ready",                    //  RETURN_NOT_READY              = 6  | MAX_BIT
  "Device Error",                 //  RETURN_DEVICE_ERROR           = 7  | MAX_BIT
  "Write Protected",              //  RETURN_WRITE_PROTECTED        = 8  | MAX_BIT
  "Out of Resources",             //  RETURN_OUT_OF_RESOURCES       = 9  | MAX_BIT
  "Volume Corrupt",               //  RETURN_VOLUME_CORRUPTED       = 10 | MAX_BIT
  "Volume Full",                  //  RETURN_VOLUME_FULL            = 11 | MAX_BIT
  "No Media",                     //  RETURN_NO_MEDIA               = 12 | MAX_BIT
  "Media changed",                //  RETURN_MEDIA_CHANGED          = 13 | MAX_BIT
  "Not Found",                    //  RETURN_NOT_FOUND              = 14 | MAX_BIT
  "Access Denied",                //  RETURN_ACCESS_DENIED          = 15 | MAX_BIT
  "No Response",                  //  RETURN_NO_RESPONSE            = 16 | MAX_BIT
  "No mapping",                   //  RETURN_NO_MAPPING             = 17 | MAX_BIT
  "Time out",                     //  RETURN_TIMEOUT                = 18 | MAX_BIT
  "Not started",                  //  RETURN_NOT_STARTED            = 19 | MAX_BIT
  "Already started",              //  RETURN_ALREADY_STARTED        = 20 | MAX_BIT
  "Aborted",                      //  RETURN_ABORTED                = 21 | MAX_BIT
  "ICMP Error",                   //  RETURN_ICMP_ERROR             = 22 | MAX_BIT
  "TFTP Error",                   //  RETURN_TFTP_ERROR             = 23 | MAX_BIT
  "Protocol Error",               //  RETURN_PROTOCOL_ERROR         = 24 | MAX_BIT
  "Incompatible Version",         //  RETURN_INCOMPATIBLE_VERSION   = 25 | MAX_BIT
  "Security Violation",           //  RETURN_SECURITY_VIOLATION     = 26 | MAX_BIT
  "CRC Error",                    //  RETURN_CRC_ERROR              = 27 | MAX_BIT
  "End of Media",                 //  RETURN_END_OF_MEDIA           = 28 | MAX_BIT
  "Reserved (29)",                //  RESERVED                      = 29 | MAX_BIT
  "Reserved (30)",                //  RESERVED                      = 30 | MAX_BIT
  "End of File",                  //  RETURN_END_OF_FILE            = 31 | MAX_BIT
  "Invalid Language",             //  RETURN_INVALID_LANGUAGE       = 32 | MAX_BIT
  "Compromised Data",             //  RETURN_COMPROMISED_DATA       = 33 | MAX_BIT
  "IP Address Conflict",          //  RETURN_IP_ADDRESS_CONFLICT    = 34 | MAX_BIT
  "HTTP Error"                    //  RETURN_HTTP_ERROR             = 35 | MAX_BIT
};



//
// Record date and time information
//
typedef struct {
	UINT16    Year;
	UINT8     Month;
	UINT8     Day;
	UINT8     Hour;
	UINT8     Minute;
	UINT8     Second;
	UINT8     Pad1;
	UINT32    Nanosecond;
	INT16     TimeZone;
	UINT8     Daylight;
	UINT8     Pad2;
} TIME;
UINTN
EFIAPI
BasePrintLibSPrint(
    OUT CHAR8* StartOfBuffer,
    IN  UINTN        BufferSize,
    IN  UINTN        Flags,
    IN  CONST CHAR8* FormatString,
    ...
);
VOID* malloc(
    IN  UINTN                        Size
);

void free(
	IN  VOID*);


VOID* AllocatePool(
	IN  UINTN                        Size	
	)
{
    return malloc(Size);
}




void FreePool(
	 IN  VOID* buffer
 )
{
	 free(buffer);
     return;
}


/**
  Shifts a 64-bit integer right between 0 and 63 bits. This high bits
  are filled with zeros. The shifted value is returned.

  This function shifts the 64-bit value Operand to the right by Count bits. The
  high Count bits are set to zero. The shifted value is returned.

  @param  Operand The 64-bit operand to shift right.
  @param  Count   The number of bits to shift right.

  @return Operand >> Count.

**/
UINT64
EFIAPI
RShiftU64(
    IN      UINT64  Operand,
    IN      UINTN   Count
)
{
    return Operand >> Count;
}

/**
  Shifts a 64-bit integer left between 0 and 63 bits. The low bits
  are filled with zeros. The shifted value is returned.

  This function shifts the 64-bit value Operand to the left by Count bits. The
  low Count bits are set to zero. The shifted value is returned.

  @param  Operand The 64-bit operand to shift left.
  @param  Count   The number of bits to shift left.

  @return Operand << Count.

**/
UINT64
EFIAPI
LShiftU64(
    IN      UINT64  Operand,
    IN      UINTN   Count
)
{
    return Operand << Count;
}

/**
  Reads a 16-bit value from memory that may be unaligned.

  This function returns the 16-bit value pointed to by Buffer. The function
  guarantees that the read operation does not produce an alignment fault.

  If the Buffer is NULL, then ASSERT().

  @param  Buffer  The pointer to a 16-bit value that may be unaligned.

  @return The 16-bit value read from Buffer.

**/
UINT16
EFIAPI
ReadUnaligned16(
    IN CONST UINT16* Buffer
)
{
    volatile UINT8  LowerByte;
    volatile UINT8  HigherByte;

    ASSERT(Buffer != NULL);

    LowerByte = ((UINT8*)Buffer)[0];
    HigherByte = ((UINT8*)Buffer)[1];

    return (UINT16)(LowerByte | (HigherByte << 8));
}

/**
  Writes a 16-bit value to memory that may be unaligned.

  This function writes the 16-bit value specified by Value to Buffer. Value is
  returned. The function guarantees that the write operation does not produce
  an alignment fault.

  If the Buffer is NULL, then ASSERT().

  @param  Buffer  The pointer to a 16-bit value that may be unaligned.
  @param  Value   16-bit value to write to Buffer.

  @return The 16-bit value to write to Buffer.

**/
UINT16
EFIAPI
WriteUnaligned16(
    OUT UINT16* Buffer,
    IN  UINT16  Value
)
{
    ASSERT(Buffer != NULL);

    ((volatile UINT8*)Buffer)[0] = (UINT8)Value;
    ((volatile UINT8*)Buffer)[1] = (UINT8)(Value >> 8);

    return Value;
}

/**
  Reads a 24-bit value from memory that may be unaligned.

  This function returns the 24-bit value pointed to by Buffer. The function
  guarantees that the read operation does not produce an alignment fault.

  If the Buffer is NULL, then ASSERT().

  @param  Buffer  The pointer to a 24-bit value that may be unaligned.

  @return The 24-bit value read from Buffer.

**/
UINT32
EFIAPI
ReadUnaligned24(
    IN CONST UINT32* Buffer
)
{
    ASSERT(Buffer != NULL);

    return (UINT32)(
        ReadUnaligned16((UINT16*)Buffer) |
        (((UINT8*)Buffer)[2] << 16)
        );
}

/**
  Writes a 24-bit value to memory that may be unaligned.

  This function writes the 24-bit value specified by Value to Buffer. Value is
  returned. The function guarantees that the write operation does not produce
  an alignment fault.

  If the Buffer is NULL, then ASSERT().

  @param  Buffer  The pointer to a 24-bit value that may be unaligned.
  @param  Value   24-bit value to write to Buffer.

  @return The 24-bit value to write to Buffer.

**/
UINT32
EFIAPI
WriteUnaligned24(
    OUT UINT32* Buffer,
    IN  UINT32  Value
)
{
    ASSERT(Buffer != NULL);

    WriteUnaligned16((UINT16*)Buffer, (UINT16)Value);
    *(UINT8*)((UINT16*)Buffer + 1) = (UINT8)(Value >> 16);
    return Value;
}

/**
  Reads a 32-bit value from memory that may be unaligned.

  This function returns the 32-bit value pointed to by Buffer. The function
  guarantees that the read operation does not produce an alignment fault.

  If the Buffer is NULL, then ASSERT().

  @param  Buffer  The pointer to a 32-bit value that may be unaligned.

  @return The 32-bit value read from Buffer.

**/
UINT32
EFIAPI
ReadUnaligned32(
    IN CONST UINT32* Buffer
)
{
    UINT16  LowerBytes;
    UINT16  HigherBytes;

    ASSERT(Buffer != NULL);

    LowerBytes = ReadUnaligned16((UINT16*)Buffer);
    HigherBytes = ReadUnaligned16((UINT16*)Buffer + 1);

    return (UINT32)(LowerBytes | (HigherBytes << 16));
}

/**
  Writes a 32-bit value to memory that may be unaligned.

  This function writes the 32-bit value specified by Value to Buffer. Value is
  returned. The function guarantees that the write operation does not produce
  an alignment fault.

  If the Buffer is NULL, then ASSERT().

  @param  Buffer  The pointer to a 32-bit value that may be unaligned.
  @param  Value   32-bit value to write to Buffer.

  @return The 32-bit value to write to Buffer.

**/
UINT32
EFIAPI
WriteUnaligned32(
    OUT UINT32* Buffer,
    IN  UINT32  Value
)
{
    ASSERT(Buffer != NULL);

    WriteUnaligned16((UINT16*)Buffer, (UINT16)Value);
    WriteUnaligned16((UINT16*)Buffer + 1, (UINT16)(Value >> 16));
    return Value;
}

/**
  Reads a 64-bit value from memory that may be unaligned.

  This function returns the 64-bit value pointed to by Buffer. The function
  guarantees that the read operation does not produce an alignment fault.

  If the Buffer is NULL, then ASSERT().

  @param  Buffer  The pointer to a 64-bit value that may be unaligned.

  @return The 64-bit value read from Buffer.

**/
UINT64
EFIAPI
ReadUnaligned64(
    IN CONST UINT64* Buffer
)
{
    UINT32  LowerBytes;
    UINT32  HigherBytes;

    ASSERT(Buffer != NULL);

    LowerBytes = ReadUnaligned32((UINT32*)Buffer);
    HigherBytes = ReadUnaligned32((UINT32*)Buffer + 1);

    return (UINT64)(LowerBytes | LShiftU64(HigherBytes, 32));
}

/**
  Writes a 64-bit value to memory that may be unaligned.

  This function writes the 64-bit value specified by Value to Buffer. Value is
  returned. The function guarantees that the write operation does not produce
  an alignment fault.

  If the Buffer is NULL, then ASSERT().

  @param  Buffer  The pointer to a 64-bit value that may be unaligned.
  @param  Value   64-bit value to write to Buffer.

  @return The 64-bit value to write to Buffer.

**/
UINT64
EFIAPI
WriteUnaligned64(
    OUT UINT64* Buffer,
    IN  UINT64  Value
)
{
    ASSERT(Buffer != NULL);

    WriteUnaligned32((UINT32*)Buffer, (UINT32)Value);
    WriteUnaligned32((UINT32*)Buffer + 1, (UINT32)RShiftU64(Value, 32));
    return Value;
}
UINT64
EFIAPI
DivU64x32Remainder(
	IN      UINT64  Dividend,
	IN      UINT32  Divisor,
	OUT     UINT32* Remainder OPTIONAL
)
{
	if (Remainder != NULL) {
		*Remainder = (UINT32)(Dividend % Divisor);
	}

	return Dividend / Divisor;
}

/**
  Returns the first occurrence of a Null-terminated ASCII sub-string
  in a Null-terminated ASCII string.

  This function scans the contents of the ASCII string specified by String
  and returns the first occurrence of SearchString. If SearchString is not
  found in String, then NULL is returned. If the length of SearchString is zero,
  then String is returned.

  If String is NULL, then ASSERT().
  If SearchString is NULL, then ASSERT().

  If PcdMaximumAsciiStringLength is not zero, and SearchString or
  String contains more than PcdMaximumAsciiStringLength Unicode characters
  not including the Null-terminator, then ASSERT().

  @param  String          A pointer to a Null-terminated ASCII string.
  @param  SearchString    A pointer to a Null-terminated ASCII string to search for.

  @retval NULL            If the SearchString does not appear in String.
  @retval others          If there is a match return the first occurrence of SearchingString.
                          If the length of SearchString is zero,return String.

**/
CHAR8*
EFIAPI
AsciiStrStr(
    IN      CONST CHAR8* String,
    IN      CONST CHAR8* SearchString
)
{
    CONST CHAR8* FirstMatch;
    CONST CHAR8* SearchStringTmp;

    //
    // ASSERT both strings are less long than PcdMaximumAsciiStringLength
    //
    ASSERT(AsciiStrSize(String) != 0);
    ASSERT(AsciiStrSize(SearchString) != 0);

    if (*SearchString == '\0') {
        return (CHAR8*)String;
    }

    while (*String != '\0') {
        SearchStringTmp = SearchString;
        FirstMatch = String;

        while ((*String == *SearchStringTmp)
            && (*String != '\0'))
        {
            String++;
            SearchStringTmp++;
        }

        if (*SearchStringTmp == '\0') {
            return (CHAR8*)FirstMatch;
        }

        if (*String == '\0') {
            return NULL;
        }

        String = FirstMatch + 1;
    }

    return NULL;
}

/**
  Returns the first occurrence of a Null-terminated Unicode sub-string
  in a Null-terminated Unicode string.

  This function scans the contents of the Null-terminated Unicode string
  specified by String and returns the first occurrence of SearchString.
  If SearchString is not found in String, then NULL is returned.  If
  the length of SearchString is zero, then String is
  returned.

  If String is NULL, then ASSERT().
  If String is not aligned on a 16-bit boundary, then ASSERT().
  If SearchString is NULL, then ASSERT().
  If SearchString is not aligned on a 16-bit boundary, then ASSERT().

  If PcdMaximumUnicodeStringLength is not zero, and SearchString
  or String contains more than PcdMaximumUnicodeStringLength Unicode
  characters, not including the Null-terminator, then ASSERT().

  @param  String          A pointer to a Null-terminated Unicode string.
  @param  SearchString    A pointer to a Null-terminated Unicode string to search for.

  @retval NULL            If the SearchString does not appear in String.
  @return others          If there is a match.

**/
CHAR16*
EFIAPI
StrStr(
    IN      CONST CHAR16* String,
    IN      CONST CHAR16* SearchString
)
{
    CONST CHAR16* FirstMatch;
    CONST CHAR16* SearchStringTmp;

    //
    // ASSERT both strings are less long than PcdMaximumUnicodeStringLength.
    // Length tests are performed inside StrLen().
    //
    ASSERT(StrSize(String) != 0);
    ASSERT(StrSize(SearchString) != 0);

    if (*SearchString == L'\0') {
        return (CHAR16*)String;
    }

    while (*String != L'\0') {
        SearchStringTmp = SearchString;
        FirstMatch = String;

        while ((*String == *SearchStringTmp)
            && (*String != L'\0'))
        {
            String++;
            SearchStringTmp++;
        }

        if (*SearchStringTmp == L'\0') {
            return (CHAR16*)FirstMatch;
        }

        if (*String == L'\0') {
            return NULL;
        }

        String = FirstMatch + 1;
    }

    return NULL;
}

/**
  Returns the length of a Null-terminated Unicode string.

  This function is similar as strlen_s defined in C11.

  If String is not aligned on a 16-bit boundary, then ASSERT().

  @param  String   A pointer to a Null-terminated Unicode string.
  @param  MaxSize  The maximum number of Destination Unicode
                   char, including terminating null char.

  @retval 0        If String is NULL.
  @retval MaxSize  If there is no null character in the first MaxSize characters of String.
  @return The number of characters that percede the terminating null character.

**/
UINTN
EFIAPI
StrnLenS(
    IN CONST CHAR16* String,
    IN UINTN         MaxSize
)
{
    UINTN  Length;

    ASSERT(((UINTN)String & BIT0) == 0);

    //
    // If String is a null pointer or MaxSize is 0, then the StrnLenS function returns zero.
    //
    if ((String == NULL) || (MaxSize == 0)) {
        return 0;
    }

    //
    // Otherwise, the StrnLenS function returns the number of characters that precede the
    // terminating null character. If there is no null character in the first MaxSize characters of
    // String then StrnLenS returns MaxSize. At most the first MaxSize characters of String shall
    // be accessed by StrnLenS.
    //
    Length = 0;
    while (String[Length] != 0) {
        if (Length >= MaxSize - 1) {
            return MaxSize;
        }

        Length++;
    }

    return Length;
}

/**
  Returns the length of a Null-terminated Ascii string.

  This function is similar as strlen_s defined in C11.

  @param  String   A pointer to a Null-terminated Ascii string.
  @param  MaxSize  The maximum number of Destination Ascii
                   char, including terminating null char.

  @retval 0        If String is NULL.
  @retval MaxSize  If there is no null character in the first MaxSize characters of String.
  @return The number of characters that percede the terminating null character.

**/
UINTN
EFIAPI
AsciiStrnLenS(
    IN CONST CHAR8* String,
    IN UINTN        MaxSize
)
{
    UINTN  Length;

    //
    // If String is a null pointer or MaxSize is 0, then the AsciiStrnLenS function returns zero.
    //
    if ((String == NULL) || (MaxSize == 0)) {
        return 0;
    }

    //
    // Otherwise, the AsciiStrnLenS function returns the number of characters that precede the
    // terminating null character. If there is no null character in the first MaxSize characters of
    // String then AsciiStrnLenS returns MaxSize. At most the first MaxSize characters of String shall
    // be accessed by AsciiStrnLenS.
    //
    Length = 0;
    while (String[Length] != 0) {
        if (Length >= MaxSize - 1) {
            return MaxSize;
        }

        Length++;
    }

    return Length;
}



/**
  Returns the length of a Null-terminated Unicode string.

  This function returns the number of Unicode characters in the Null-terminated
  Unicode string specified by String.

  If String is NULL, then ASSERT().
  If String is not aligned on a 16-bit boundary, then ASSERT().
  If PcdMaximumUnicodeStringLength is not zero, and String contains more than
  PcdMaximumUnicodeStringLength Unicode characters, not including the
  Null-terminator, then ASSERT().

  @param  String  A pointer to a Null-terminated Unicode string.

  @return The length of String.

**/
UINTN
EFIAPI
StrLen(
    IN      CONST CHAR16* String
)
{
    UINTN  Length;

    ASSERT(String != NULL);
    ASSERT(((UINTN)String & BIT0) == 0);

    for (Length = 0; *String != L'\0'; String++, Length++) {
        //
        // If PcdMaximumUnicodeStringLength is not zero,
        // length should not more than PcdMaximumUnicodeStringLength
        //
       
    }

    return Length;
}

/**
  Returns the size of a Null-terminated Unicode string in bytes, including the
  Null terminator.

  This function returns the size, in bytes, of the Null-terminated Unicode string
  specified by String.

  If String is NULL, then ASSERT().
  If String is not aligned on a 16-bit boundary, then ASSERT().
  If PcdMaximumUnicodeStringLength is not zero, and String contains more than
  PcdMaximumUnicodeStringLength Unicode characters, not including the
  Null-terminator, then ASSERT().

  @param  String  A pointer to a Null-terminated Unicode string.

  @return The size of String.

**/
UINTN
EFIAPI
StrSize(
    IN      CONST CHAR16* String
)
{
    return (StrLen(String) + 1) * sizeof(*String);
}

/**
  Compares two Null-terminated Unicode strings, and returns the difference
  between the first mismatched Unicode characters.

  This function compares the Null-terminated Unicode string FirstString to the
  Null-terminated Unicode string SecondString. If FirstString is identical to
  SecondString, then 0 is returned. Otherwise, the value returned is the first
  mismatched Unicode character in SecondString subtracted from the first
  mismatched Unicode character in FirstString.

  If FirstString is NULL, then ASSERT().
  If FirstString is not aligned on a 16-bit boundary, then ASSERT().
  If SecondString is NULL, then ASSERT().
  If SecondString is not aligned on a 16-bit boundary, then ASSERT().
  If PcdMaximumUnicodeStringLength is not zero, and FirstString contains more
  than PcdMaximumUnicodeStringLength Unicode characters, not including the
  Null-terminator, then ASSERT().
  If PcdMaximumUnicodeStringLength is not zero, and SecondString contains more
  than PcdMaximumUnicodeStringLength Unicode characters, not including the
  Null-terminator, then ASSERT().

  @param  FirstString   A pointer to a Null-terminated Unicode string.
  @param  SecondString  A pointer to a Null-terminated Unicode string.

  @retval 0      FirstString is identical to SecondString.
  @return others FirstString is not identical to SecondString.

**/
INTN
EFIAPI
StrCmp(
    IN      CONST CHAR16* FirstString,
    IN      CONST CHAR16* SecondString
)
{
    //
    // ASSERT both strings are less long than PcdMaximumUnicodeStringLength
    //
    ASSERT(StrSize(FirstString) != 0);
    ASSERT(StrSize(SecondString) != 0);

    while ((*FirstString != L'\0') && (*FirstString == *SecondString)) {
        FirstString++;
        SecondString++;
    }

    return *FirstString - *SecondString;
}

/**
  Compares up to a specified length the contents of two Null-terminated Unicode strings,
  and returns the difference between the first mismatched Unicode characters.

  This function compares the Null-terminated Unicode string FirstString to the
  Null-terminated Unicode string SecondString. At most, Length Unicode
  characters will be compared. If Length is 0, then 0 is returned. If
  FirstString is identical to SecondString, then 0 is returned. Otherwise, the
  value returned is the first mismatched Unicode character in SecondString
  subtracted from the first mismatched Unicode character in FirstString.

  If Length > 0 and FirstString is NULL, then ASSERT().
  If Length > 0 and FirstString is not aligned on a 16-bit boundary, then ASSERT().
  If Length > 0 and SecondString is NULL, then ASSERT().
  If Length > 0 and SecondString is not aligned on a 16-bit boundary, then ASSERT().
  If PcdMaximumUnicodeStringLength is not zero, and Length is greater than
  PcdMaximumUnicodeStringLength, then ASSERT().
  If PcdMaximumUnicodeStringLength is not zero, and FirstString contains more than
  PcdMaximumUnicodeStringLength Unicode characters, not including the Null-terminator,
  then ASSERT().
  If PcdMaximumUnicodeStringLength is not zero, and SecondString contains more than
  PcdMaximumUnicodeStringLength Unicode characters, not including the Null-terminator,
  then ASSERT().

  @param  FirstString   A pointer to a Null-terminated Unicode string.
  @param  SecondString  A pointer to a Null-terminated Unicode string.
  @param  Length        The maximum number of Unicode characters to compare.

  @retval 0      FirstString is identical to SecondString.
  @return others FirstString is not identical to SecondString.

**/
INTN
EFIAPI
StrnCmp(
    IN      CONST CHAR16* FirstString,
    IN      CONST CHAR16* SecondString,
    IN      UINTN         Length
)
{
    if (Length == 0) {
        return 0;
    }

    //
    // ASSERT both strings are less long than PcdMaximumUnicodeStringLength.
    // Length tests are performed inside StrLen().
    //
    ASSERT(StrSize(FirstString) != 0);
    ASSERT(StrSize(SecondString) != 0);

   

    while ((*FirstString != L'\0') &&
        (*SecondString != L'\0') &&
        (*FirstString == *SecondString) &&
        (Length > 1))
    {
        FirstString++;
        SecondString++;
        Length--;
    }

    return *FirstString - *SecondString;
}


/**
  Appends a copy of the string pointed to by Source (including the terminating
  null char) to the end of the string pointed to by Destination.

  This function is similar as strcat_s defined in C11.

  If Destination is not aligned on a 16-bit boundary, then ASSERT().
  If Source is not aligned on a 16-bit boundary, then ASSERT().

  If an error is returned, then the Destination is unmodified.

  @param  Destination              A pointer to a Null-terminated Unicode string.
  @param  DestMax                  The maximum number of Destination Unicode
                                   char, including terminating null char.
  @param  Source                   A pointer to a Null-terminated Unicode string.

  @retval RETURN_SUCCESS           String is appended.
  @retval RETURN_BAD_BUFFER_SIZE   If DestMax is NOT greater than
                                   StrLen(Destination).
  @retval RETURN_BUFFER_TOO_SMALL  If (DestMax - StrLen(Destination)) is NOT
                                   greater than StrLen(Source).
  @retval RETURN_INVALID_PARAMETER If Destination is NULL.
                                   If Source is NULL.
                                   If PcdMaximumUnicodeStringLength is not zero,
                                    and DestMax is greater than
                                    PcdMaximumUnicodeStringLength.
                                   If DestMax is 0.
  @retval RETURN_ACCESS_DENIED     If Source and Destination overlap.
**/
RETURN_STATUS
EFIAPI
StrCatS(
    IN OUT CHAR16* Destination,
    IN     UINTN         DestMax,
    IN     CONST CHAR16* Source
)
{
    UINTN  DestLen;
    UINTN  CopyLen;
    UINTN  SourceLen;

    ASSERT(((UINTN)Destination & BIT0) == 0);
    ASSERT(((UINTN)Source & BIT0) == 0);

    //
    // Let CopyLen denote the value DestMax - StrnLenS(Destination, DestMax) upon entry to StrCatS.
    //
    DestLen = StrnLenS(Destination, DestMax);
    CopyLen = DestMax - DestLen;

    //
    // 1. Neither Destination nor Source shall be a null pointer.
    //
    ASSERT_STRING((Destination != NULL), RETURN_INVALID_PARAMETER);
    ASSERT_STRING((Source != NULL), RETURN_INVALID_PARAMETER);

    //
    // 2. DestMax shall not be greater than RSIZE_MAX.
    //
    if (RSIZE_MAX != 0) {
        ASSERT_STRING((DestMax <= RSIZE_MAX), RETURN_INVALID_PARAMETER);
    }

    //
    // 3. DestMax shall not equal zero.
    //
    ASSERT_STRING((DestMax != 0), RETURN_INVALID_PARAMETER);

    //
    // 4. CopyLen shall not equal zero.
    //
    ASSERT_STRING((CopyLen != 0), RETURN_BAD_BUFFER_SIZE);

    //
    // 5. CopyLen shall be greater than StrnLenS(Source, CopyLen).
    //
    SourceLen = StrnLenS(Source, CopyLen);
    ASSERT_STRING((CopyLen > SourceLen), RETURN_BUFFER_TOO_SMALL);

    //
    // 6. Copying shall not take place between objects that overlap.
    //
   // ASSERT_STRING(InternalSafeStringNoStrOverlap(Destination, DestMax, (CHAR16*)Source, SourceLen + 1), RETURN_ACCESS_DENIED);

    //
    // The StrCatS function appends a copy of the string pointed to by Source (including the
    // terminating null character) to the end of the string pointed to by Destination. The initial character
    // from Source overwrites the null character at the end of Destination.
    //
    Destination = Destination + DestLen;
    while (*Source != 0) {
        *(Destination++) = *(Source++);
    }

    *Destination = 0;

    return RETURN_SUCCESS;
}

/**
  Copies the string pointed to by Source (including the terminating null char)
  to the array pointed to by Destination.

  This function is similar as strcpy_s defined in C11.

  If Destination is not aligned on a 16-bit boundary, then ASSERT().
  If Source is not aligned on a 16-bit boundary, then ASSERT().

  If an error is returned, then the Destination is unmodified.

  @param  Destination              A pointer to a Null-terminated Unicode string.
  @param  DestMax                  The maximum number of Destination Unicode
                                   char, including terminating null char.
  @param  Source                   A pointer to a Null-terminated Unicode string.

  @retval RETURN_SUCCESS           String is copied.
  @retval RETURN_BUFFER_TOO_SMALL  If DestMax is NOT greater than StrLen(Source).
  @retval RETURN_INVALID_PARAMETER If Destination is NULL.
                                   If Source is NULL.
                                   If PcdMaximumUnicodeStringLength is not zero,
                                    and DestMax is greater than
                                    PcdMaximumUnicodeStringLength.
                                   If DestMax is 0.
  @retval RETURN_ACCESS_DENIED     If Source and Destination overlap.
**/
RETURN_STATUS
EFIAPI
StrCpyS(
    OUT CHAR16* Destination,
    IN  UINTN         DestMax,
    IN  CONST CHAR16* Source
)
{
    UINTN  SourceLen;

    ASSERT(((UINTN)Destination & BIT0) == 0);
    ASSERT(((UINTN)Source & BIT0) == 0);

    //
    // 1. Neither Destination nor Source shall be a null pointer.
    //
    ASSERT_STRING((Destination != NULL), RETURN_INVALID_PARAMETER);
    ASSERT_STRING((Source != NULL), RETURN_INVALID_PARAMETER);

    //
    // 2. DestMax shall not be greater than RSIZE_MAX.
    //
    if (RSIZE_MAX != 0) {
        ASSERT_STRING((DestMax <= RSIZE_MAX), RETURN_INVALID_PARAMETER);
    }

    //
    // 3. DestMax shall not equal zero.
    //
    ASSERT_STRING((DestMax != 0), RETURN_INVALID_PARAMETER);

    //
    // 4. DestMax shall be greater than StrnLenS(Source, DestMax).
    //
    SourceLen = StrnLenS(Source, DestMax);
    ASSERT_STRING((DestMax > SourceLen), RETURN_BUFFER_TOO_SMALL);

    //
    // 5. Copying shall not take place between objects that overlap.
    //
   // ASSERT_STRING(InternalSafeStringNoStrOverlap(Destination, DestMax, (CHAR16*)Source, SourceLen + 1), RETURN_ACCESS_DENIED);

    //
    // The StrCpyS function copies the string pointed to by Source (including the terminating
    // null character) into the array pointed to by Destination.
    //
    while (*Source != 0) {
        *(Destination++) = *(Source++);
    }

    *Destination = 0;

    return RETURN_SUCCESS;
}


/**
  Internal function that places the character into the Buffer.

  Internal function that places ASCII or Unicode character into the Buffer.

  @param  Buffer      The buffer to place the Unicode or ASCII string.
  @param  EndBuffer   The end of the input Buffer. No characters will be
                      placed after that.
  @param  Length      The count of character to be placed into Buffer.
                      (Negative value indicates no buffer fill.)
  @param  Character   The character to be placed into Buffer.
  @param  Increment   The character increment in Buffer.

  @return Buffer.

**/
CHAR8*
BasePrintLibFillBuffer(
    OUT CHAR8* Buffer,
    IN  CHAR8* EndBuffer,
    IN  INTN   Length,
    IN  UINTN  Character,
    IN  INTN   Increment
)
{
    INTN  Index;

    for (Index = 0; Index < Length && Buffer < EndBuffer; Index++) {
        *Buffer = (CHAR8)Character;
        if (Increment != 1) {
            *(Buffer + 1) = (CHAR8)(Character >> 8);
        }

        Buffer += Increment;
    }

    return Buffer;
}



/**
  Internal function that convert a number to a string in Buffer.

  Print worker function that converts a decimal or hexadecimal number to an ASCII string in Buffer.

  @param  Buffer    Location to place the ASCII string of Value.
  @param  Value     The value to convert to a Decimal or Hexadecimal string in Buffer.
  @param  Radix     Radix of the value

  @return A pointer to the end of buffer filled with ASCII string.

**/
CHAR8*
BasePrintLibValueToString(
    IN OUT CHAR8* Buffer,
    IN INT64      Value,
    IN UINTN      Radix
)
{
    UINT32  Remainder;

    //
    // Loop to convert one digit at a time in reverse order
    //
    *Buffer = 0;
    do {
        Value = (INT64)DivU64x32Remainder((UINT64)Value, (UINT32)Radix, &Remainder);
        *(++Buffer) = mHexStr[Remainder];
    } while (Value != 0);

    //
    // Return pointer of the end of filled buffer.
    //
    return Buffer;
}

/**
  Internal function that converts a decimal value to a Null-terminated string.

  Converts the decimal number specified by Value to a Null-terminated
  string specified by Buffer containing at most Width characters.
  If Width is 0 then a width of  MAXIMUM_VALUE_CHARACTERS is assumed.
  The total number of characters placed in Buffer is returned.
  If the conversion contains more than Width characters, then only the first
  Width characters are returned, and the total number of characters
  required to perform the conversion is returned.
  Additional conversion parameters are specified in Flags.
  The Flags bit LEFT_JUSTIFY is always ignored.
  All conversions are left justified in Buffer.
  If Width is 0, PREFIX_ZERO is ignored in Flags.
  If COMMA_TYPE is set in Flags, then PREFIX_ZERO is ignored in Flags, and commas
  are inserted every 3rd digit starting from the right.
  If Value is < 0, then the fist character in Buffer is a '-'.
  If PREFIX_ZERO is set in Flags and PREFIX_ZERO is not being ignored,
  then Buffer is padded with '0' characters so the combination of the optional '-'
  sign character, '0' characters, digit characters for Value, and the Null-terminator
  add up to Width characters.

  If Buffer is NULL, then ASSERT().
  If unsupported bits are set in Flags, then ASSERT().
  If Width >= MAXIMUM_VALUE_CHARACTERS, then ASSERT()

  @param  Buffer    The pointer to the output buffer for the produced Null-terminated
                    string.
  @param  Flags     The bitmask of flags that specify left justification, zero pad,
                    and commas.
  @param  Value     The 64-bit signed value to convert to a string.
  @param  Width     The maximum number of characters to place in Buffer, not including
                    the Null-terminator.
  @param  Increment The character increment in Buffer.

  @return Total number of characters required to perform the conversion.

**/
UINTN
BasePrintLibConvertValueToString(
    IN OUT CHAR8* Buffer,
    IN UINTN      Flags,
    IN INT64      Value,
    IN UINTN      Width,
    IN UINTN      Increment
)
{
    CHAR8* OriginalBuffer;
    CHAR8* EndBuffer;
    CHAR8  ValueBuffer[MAXIMUM_VALUE_CHARACTERS];
    CHAR8* ValueBufferPtr;
    UINTN  Count;
    UINTN  Digits;
    UINTN  Index;
    UINTN  Radix;

    //
    // Make sure Buffer is not NULL and Width < MAXIMUM
    //
    ASSERT(Buffer != NULL);
    ASSERT(Width < MAXIMUM_VALUE_CHARACTERS);
    //
    // Make sure Flags can only contain supported bits.
    //
    ASSERT((Flags & ~(LEFT_JUSTIFY | COMMA_TYPE | PREFIX_ZERO | RADIX_HEX)) == 0);

    //
    // If both COMMA_TYPE and RADIX_HEX are set, then ASSERT ()
    //
    ASSERT(((Flags & COMMA_TYPE) == 0) || ((Flags & RADIX_HEX) == 0));

    OriginalBuffer = Buffer;

    //
    // Width is 0 or COMMA_TYPE is set, PREFIX_ZERO is ignored.
    //
    if ((Width == 0) || ((Flags & COMMA_TYPE) != 0)) {
        Flags &= ~((UINTN)PREFIX_ZERO);
    }

    //
    // If Width is 0 then a width of  MAXIMUM_VALUE_CHARACTERS is assumed.
    //
    if (Width == 0) {
        Width = MAXIMUM_VALUE_CHARACTERS - 1;
    }

    //
    // Set the tag for the end of the input Buffer.
    //
    EndBuffer = Buffer + Width * Increment;

    //
    // Convert decimal negative
    //
    if ((Value < 0) && ((Flags & RADIX_HEX) == 0)) {
        Value = -Value;
        Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, 1, '-', Increment);
        Width--;
    }

    //
    // Count the length of the value string.
    //
    Radix = ((Flags & RADIX_HEX) == 0) ? 10 : 16;
    ValueBufferPtr = BasePrintLibValueToString(ValueBuffer, Value, Radix);
    Count = ValueBufferPtr - ValueBuffer;

    //
    // Append Zero
    //
    if ((Flags & PREFIX_ZERO) != 0) {
        Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, Width - Count, '0', Increment);
    }

    //
    // Print Comma type for every 3 characters
    //
    Digits = Count % 3;
    if (Digits != 0) {
        Digits = 3 - Digits;
    }

    for (Index = 0; Index < Count; Index++) {
        Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, 1, *ValueBufferPtr--, Increment);
        if ((Flags & COMMA_TYPE) != 0) {
            Digits++;
            if (Digits == 3) {
                Digits = 0;
                if ((Index + 1) < Count) {
                    Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, 1, ',', Increment);
                }
            }
        }
    }

    //
    // Print Null-terminator
    //
    BasePrintLibFillBuffer(Buffer, EndBuffer + Increment, 1, 0, Increment);

    return ((Buffer - OriginalBuffer) / Increment);
}

/**
  Internal function that converts a decimal value to a Null-terminated string.

  Converts the decimal number specified by Value to a Null-terminated string
  specified by Buffer containing at most Width characters. If Width is 0 then a
  width of MAXIMUM_VALUE_CHARACTERS is assumed. If the conversion contains more
  than Width characters, then only the first Width characters are placed in
  Buffer. Additional conversion parameters are specified in Flags.
  The Flags bit LEFT_JUSTIFY is always ignored.
  All conversions are left justified in Buffer.
  If Width is 0, PREFIX_ZERO is ignored in Flags.
  If COMMA_TYPE is set in Flags, then PREFIX_ZERO is ignored in Flags, and
  commas are inserted every 3rd digit starting from the right.
  If Value is < 0, then the fist character in Buffer is a '-'.
  If PREFIX_ZERO is set in Flags and PREFIX_ZERO is not being ignored,
  then Buffer is padded with '0' characters so the combination of the optional
  '-' sign character, '0' characters, digit characters for Value, and the
  Null-terminator add up to Width characters.

  If an error would be returned, the function will ASSERT().

  @param  Buffer      The pointer to the output buffer for the produced
                      Null-terminated string.
  @param  BufferSize  The size of Buffer in bytes, including the
                      Null-terminator.
  @param  Flags       The bitmask of flags that specify left justification,
                      zero pad, and commas.
  @param  Value       The 64-bit signed value to convert to a string.
  @param  Width       The maximum number of characters to place in Buffer,
                      not including the Null-terminator.
  @param  Increment   The character increment in Buffer.

  @retval RETURN_SUCCESS           The decimal value is converted.
  @retval RETURN_BUFFER_TOO_SMALL  If BufferSize cannot hold the converted
                                   value.
  @retval RETURN_INVALID_PARAMETER If Buffer is NULL.
                                   If Increment is 1 and
                                   PcdMaximumAsciiStringLength is not zero,
                                   BufferSize is greater than
                                   PcdMaximumAsciiStringLength.
                                   If Increment is not 1 and
                                   PcdMaximumUnicodeStringLength is not zero,
                                   BufferSize is greater than
                                   (PcdMaximumUnicodeStringLength *
                                   sizeof (CHAR16) + 1).
                                   If unsupported bits are set in Flags.
                                   If both COMMA_TYPE and RADIX_HEX are set in
                                   Flags.
                                   If Width >= MAXIMUM_VALUE_CHARACTERS.

**/
RETURN_STATUS
BasePrintLibConvertValueToStringS(
    IN OUT CHAR8* Buffer,
    IN UINTN      BufferSize,
    IN UINTN      Flags,
    IN INT64      Value,
    IN UINTN      Width,
    IN UINTN      Increment
)
{
    CHAR8* EndBuffer;
    CHAR8  ValueBuffer[MAXIMUM_VALUE_CHARACTERS];
    CHAR8* ValueBufferPtr;
    UINTN  Count;
    UINTN  Digits;
    UINTN  Index;
    UINTN  Radix;

    //
    // 1. Buffer shall not be a null pointer.
    //
    SAFE_PRINT_CONSTRAINT_CHECK_STRING((Buffer != NULL), RETURN_INVALID_PARAMETER,L"(Buffer != NULL)\r\n");

    //
    // 2. BufferSize shall not be greater than (RSIZE_MAX * sizeof (CHAR16)) for
    //    Unicode output string or shall not be greater than ASCII_RSIZE_MAX for
    //    Ascii output string.
    //
    if (Increment == 1) {
        //
        // Ascii output string
        //
        if (ASCII_RSIZE_MAX != 0) {
            SAFE_PRINT_CONSTRAINT_CHECK_STRING((BufferSize <= ASCII_RSIZE_MAX), RETURN_INVALID_PARAMETER, L"(BufferSize <= ASCII_RSIZE_MAX)\r\n");
        }
    }
    else {
        //
        // Unicode output string
        //
        if (RSIZE_MAX != 0) {
            SAFE_PRINT_CONSTRAINT_CHECK_STRING((BufferSize <= RSIZE_MAX * sizeof(CHAR16) + 1), RETURN_INVALID_PARAMETER, L"(BufferSize <= RSIZE_MAX * sizeof(CHAR16) + 1)\r\n");
        }
    }

    //
    // 3. Flags shall be set properly.
    //
    SAFE_PRINT_CONSTRAINT_CHECK_STRING(((Flags & ~(LEFT_JUSTIFY | COMMA_TYPE | PREFIX_ZERO | RADIX_HEX)) == 0), RETURN_INVALID_PARAMETER, L"(Flags & ~(LEFT_JUSTIFY | COMMA_TYPE | PREFIX_ZERO | RADIX_HEX)) == 0)\r\n");
    SAFE_PRINT_CONSTRAINT_CHECK_STRING((((Flags & COMMA_TYPE) == 0) || ((Flags & RADIX_HEX) == 0)), RETURN_INVALID_PARAMETER, L"(Flags & COMMA_TYPE) == 0) || ((Flags & RADIX_HEX) == 0)\r\n");

    //
    // 4. Width shall be smaller than MAXIMUM_VALUE_CHARACTERS.
    //
    SAFE_PRINT_CONSTRAINT_CHECK_STRING((Width < MAXIMUM_VALUE_CHARACTERS), RETURN_INVALID_PARAMETER, L"Width < MAXIMUM_VALUE_CHARACTERS\r\n");

    //
    // Width is 0 or COMMA_TYPE is set, PREFIX_ZERO is ignored.
    //
    if ((Width == 0) || ((Flags & COMMA_TYPE) != 0)) {
        Flags &= ~((UINTN)PREFIX_ZERO);
    }

    //
    // If Width is 0 then a width of MAXIMUM_VALUE_CHARACTERS is assumed.
    //
    if (Width == 0) {
        Width = MAXIMUM_VALUE_CHARACTERS - 1;
    }

    //
    // Count the characters of the output string.
    //
    Count = 0;
    Radix = ((Flags & RADIX_HEX) == 0) ? 10 : 16;

    if ((Flags & PREFIX_ZERO) != 0) {
        Count = Width;
    }
    else {
        if ((Value < 0) && ((Flags & RADIX_HEX) == 0)) {
            Count++;  // minus sign
            ValueBufferPtr = BasePrintLibValueToString(ValueBuffer, -Value, Radix);
        }
        else {
            ValueBufferPtr = BasePrintLibValueToString(ValueBuffer, Value, Radix);
        }

        Digits = ValueBufferPtr - ValueBuffer;
        Count += Digits;

        if ((Flags & COMMA_TYPE) != 0) {
            Count += (Digits - 1) / 3;  // commas
        }
    }

    Width = MIN(Count, Width);

    //
    // 5. BufferSize shall be large enough to hold the converted string.
    //
    SAFE_PRINT_CONSTRAINT_CHECK_STRING((BufferSize >= (Width + 1) * Increment), RETURN_BUFFER_TOO_SMALL, L"(BufferSize >= (Width + 1) * Increment), RETURN_BUFFER_TOO_SMALL\r\n");

    //
    // Set the tag for the end of the input Buffer.
    //
    EndBuffer = Buffer + Width * Increment;

    //
    // Convert decimal negative
    //
    if ((Value < 0) && ((Flags & RADIX_HEX) == 0)) {
        Value = -Value;
        Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, 1, '-', Increment);
        Width--;
    }

    //
    // Count the length of the value string.
    //
    ValueBufferPtr = BasePrintLibValueToString(ValueBuffer, Value, Radix);
    Count = ValueBufferPtr - ValueBuffer;

    //
    // Append Zero
    //
    if ((Flags & PREFIX_ZERO) != 0) {
        Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, Width - Count, '0', Increment);
    }

    //
    // Print Comma type for every 3 characters
    //
    Digits = Count % 3;
    if (Digits != 0) {
        Digits = 3 - Digits;
    }

    for (Index = 0; Index < Count; Index++) {
        Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, 1, *ValueBufferPtr--, Increment);
        if ((Flags & COMMA_TYPE) != 0) {
            Digits++;
            if (Digits == 3) {
                Digits = 0;
                if ((Index + 1) < Count) {
                    Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, 1, ',', Increment);
                }
            }
        }
    }

    //
    // Print Null-terminator
    //
    BasePrintLibFillBuffer(Buffer, EndBuffer + Increment, 1, 0, Increment);

    return RETURN_SUCCESS;
}


/**
  Worker function that produces a Null-terminated string in an output buffer
  based on a Null-terminated format string and a VA_LIST argument list.

  VSPrint function to process format and place the results in Buffer. Since a
  VA_LIST is used this routine allows the nesting of Vararg routines. Thus
  this is the main print working routine.

  If COUNT_ONLY_NO_PRINT is set in Flags, Buffer will not be modified at all.

  @param[out] Buffer          The character buffer to print the results of the
                              parsing of Format into.
  @param[in]  BufferSize      The maximum number of characters to put into
                              buffer.
  @param[in]  Flags           Initial flags value.
                              Can only have FORMAT_UNICODE, OUTPUT_UNICODE,
                              and COUNT_ONLY_NO_PRINT set.
  @param[in]  Format          A Null-terminated format string.
  @param[in]  VaListMarker    VA_LIST style variable argument list consumed by
                              processing Format.
  @param[in]  BaseListMarker  BASE_LIST style variable argument list consumed
                              by processing Format.

  @return The number of characters printed not including the Null-terminator.
          If COUNT_ONLY_NO_PRINT was set returns the same, but without any
          modification to Buffer.

**/
UINTN
BasePrintLibSPrintMarker(
    OUT CHAR8* Buffer,
    IN  UINTN        BufferSize,
    IN  UINTN        Flags,
    IN  CONST CHAR8* Format,
    IN  VA_LIST      VaListMarker    OPTIONAL,
    IN  BASE_LIST    BaseListMarker  OPTIONAL
)
{
    CHAR8* OriginalBuffer;
    CHAR8* EndBuffer;
    CHAR8          ValueBuffer[MAXIMUM_VALUE_CHARACTERS];
    UINT32         BytesPerOutputCharacter;
    UINTN          BytesPerFormatCharacter;
    UINTN          FormatMask;
    UINTN          FormatCharacter;
    UINTN          Width;
    UINTN          Precision;
    INT64          Value;
    CONST CHAR8* ArgumentString;
    UINTN          Character;
    GUID* TmpGuid;
    TIME* TmpTime;
    UINTN          Count;
    UINTN          ArgumentMask;
    INTN           BytesPerArgumentCharacter;
    UINTN          ArgumentCharacter;
    BOOLEAN        Done;
    UINTN          Index;
    CHAR8          Prefix;
    BOOLEAN        ZeroPad;
    BOOLEAN        Comma;
    UINTN          Digits;
    UINTN          Radix;
    RETURN_STATUS  Status;
    UINT32         GuidData1;
    UINT16         GuidData2;
    UINT16         GuidData3;
    UINTN          LengthToReturn;

    //
    // If you change this code be sure to match the 2 versions of this function.
    // Nearly identical logic is found in the BasePrintLib and
    // DxePrintLibPrint2Protocol (both PrintLib instances).
    //

    //
    // 1. Buffer shall not be a null pointer when both BufferSize > 0 and
    //    COUNT_ONLY_NO_PRINT is not set in Flags.
    //
    if ((BufferSize > 0) && ((Flags & COUNT_ONLY_NO_PRINT) == 0)) {
        SAFE_PRINT_CONSTRAINT_CHECK_STRING((Buffer != NULL), 0, L"(Buffer != NULL)\r\n");
    }

    //
    // 2. Format shall not be a null pointer when BufferSize > 0 or when
    //    COUNT_ONLY_NO_PRINT is set in Flags.
    //
    if ((BufferSize > 0) || ((Flags & COUNT_ONLY_NO_PRINT) != 0)) {
        SAFE_PRINT_CONSTRAINT_CHECK_STRING((Format != NULL), 0, L"(Format != NULL)\r\n");
    }

    //
    // 3. BufferSize shall not be greater than RSIZE_MAX for Unicode output or
    //    ASCII_RSIZE_MAX for Ascii output.
    //
    if ((Flags & OUTPUT_UNICODE) != 0) {
        if (RSIZE_MAX != 0) {
            SAFE_PRINT_CONSTRAINT_CHECK_STRING((BufferSize <= RSIZE_MAX), 0, L"(BufferSize <= RSIZE_MAX)\r\n");
        }

        BytesPerOutputCharacter = 2;
    }
    else {
        if (ASCII_RSIZE_MAX != 0) {
            SAFE_PRINT_CONSTRAINT_CHECK_STRING((BufferSize <= ASCII_RSIZE_MAX), 0, L"(BufferSize <= ASCII_RSIZE_MAX)\r\n");
        }

        BytesPerOutputCharacter = 1;
    }

    //
    // 4. Format shall not contain more than RSIZE_MAX Unicode characters or
    //    ASCII_RSIZE_MAX Ascii characters.
    //
    if ((Flags & FORMAT_UNICODE) != 0) {
        if (RSIZE_MAX != 0) {
            SAFE_PRINT_CONSTRAINT_CHECK_STRING((StrnLenS((CHAR16*)Format, RSIZE_MAX + 1) <= RSIZE_MAX), 0, L"(StrnLenS((CHAR16*)Format, RSIZE_MAX + 1)\r\n");
        }

        BytesPerFormatCharacter = 2;
        FormatMask = 0xffff;
    }
    else {
        if (ASCII_RSIZE_MAX != 0) {
            SAFE_PRINT_CONSTRAINT_CHECK_STRING((AsciiStrnLenS(Format, ASCII_RSIZE_MAX + 1) <= ASCII_RSIZE_MAX), 0, L"(AsciiStrnLenS(Format, ASCII_RSIZE_MAX + 1)\r\n");
        }

        BytesPerFormatCharacter = 1;
        FormatMask = 0xff;
    }

    if ((Flags & COUNT_ONLY_NO_PRINT) != 0) {
        if (BufferSize == 0) {
            Buffer = NULL;
        }
    }
    else {
        //
        // We can run without a Buffer for counting only.
        //
        if (BufferSize == 0) {
            return 0;
        }
    }

    LengthToReturn = 0;
    EndBuffer = NULL;
    OriginalBuffer = NULL;

    //
    // Reserve space for the Null terminator.
    //
    if (Buffer != NULL) {
        BufferSize--;
        OriginalBuffer = Buffer;

        //
        // Set the tag for the end of the input Buffer.
        //
        EndBuffer = Buffer + BufferSize * BytesPerOutputCharacter;
    }

    //
    // Get the first character from the format string
    //
    FormatCharacter = ((*Format & 0xff) | ((BytesPerFormatCharacter == 1) ? 0 : (*(Format + 1) << 8))) & FormatMask;

    //
    // Loop until the end of the format string is reached or the output buffer is full
    //
    while (FormatCharacter != 0) {
        if ((Buffer != NULL) && (Buffer >= EndBuffer)) {
            break;
        }

        //
        // Clear all the flag bits except those that may have been passed in
        //
        Flags &= (UINTN)(OUTPUT_UNICODE | FORMAT_UNICODE | COUNT_ONLY_NO_PRINT);

        //
        // Set the default width to zero, and the default precision to 1
        //
        Width = 0;
        Precision = 1;
        Prefix = 0;
        Comma = FALSE;
        ZeroPad = FALSE;
        Count = 0;
        Digits = 0;

        switch (FormatCharacter) {
        case '%':
            //
            // Parse Flags and Width
            //
            for (Done = FALSE; !Done; ) {
                Format += BytesPerFormatCharacter;
                FormatCharacter = ((*Format & 0xff) | ((BytesPerFormatCharacter == 1) ? 0 : (*(Format + 1) << 8))) & FormatMask;
                switch (FormatCharacter) {
                case '.':
                    Flags |= PRECISION;
                    break;
                case '-':
                    Flags |= LEFT_JUSTIFY;
                    break;
                case '+':
                    Flags |= PREFIX_SIGN;
                    break;
                case ' ':
                    Flags |= PREFIX_BLANK;
                    break;
                case ',':
                    Flags |= COMMA_TYPE;
                    break;
                case 'L':
                case 'l':
                    Flags |= LONG_TYPE;
                    break;
                case '*':
                    if ((Flags & PRECISION) == 0) {
                        Flags |= PAD_TO_WIDTH;
                        if (BaseListMarker == NULL) {
                            Width = VA_ARG(VaListMarker, UINTN);
                        }
                        else {
                            Width = BASE_ARG(BaseListMarker, UINTN);
                        }
                    }
                    else {
                        if (BaseListMarker == NULL) {
                            Precision = VA_ARG(VaListMarker, UINTN);
                        }
                        else {
                            Precision = BASE_ARG(BaseListMarker, UINTN);
                        }
                    }

                    break;
                case '0':
                    if ((Flags & PRECISION) == 0) {
                        Flags |= PREFIX_ZERO;
                    }

                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    for (Count = 0; ((FormatCharacter >= '0') && (FormatCharacter <= '9')); ) {
                        Count = (Count * 10) + FormatCharacter - '0';
                        Format += BytesPerFormatCharacter;
                        FormatCharacter = ((*Format & 0xff) | ((BytesPerFormatCharacter == 1) ? 0 : (*(Format + 1) << 8))) & FormatMask;
                    }

                    Format -= BytesPerFormatCharacter;
                    if ((Flags & PRECISION) == 0) {
                        Flags |= PAD_TO_WIDTH;
                        Width = Count;
                    }
                    else {
                        Precision = Count;
                    }

                    break;

                case '\0':
                    //
                    // Make no output if Format string terminates unexpectedly when
                    // looking up for flag, width, precision and type.
                    //
                    Format -= BytesPerFormatCharacter;
                    Precision = 0;
                    //
                    // break skipped on purpose.
                    //
                default:
                    Done = TRUE;
                    break;
                }
            }

            //
            // Handle each argument type
            //
            switch (FormatCharacter) {
            case 'p':
                //
                // Flag space, +, 0, L & l are invalid for type p.
                //
                Flags &= ~((UINTN)(PREFIX_BLANK | PREFIX_SIGN | PREFIX_ZERO | LONG_TYPE));
                if (sizeof(VOID*) > 4) {
                    Flags |= LONG_TYPE;
                }

                //
                // break skipped on purpose
                //
            case 'X':
                Flags |= PREFIX_ZERO;
                //
                // break skipped on purpose
                //
            case 'x':
                Flags |= RADIX_HEX;
                //
                // break skipped on purpose
                //
            case 'u':
                if ((Flags & RADIX_HEX) == 0) {
                    Flags &= ~((UINTN)(PREFIX_SIGN));
                    Flags |= UNSIGNED_TYPE;
                }

                //
                // break skipped on purpose
                //
            case 'd':
                if ((Flags & LONG_TYPE) == 0) {
                    //
                    // 'd', 'u', 'x', and 'X' that are not preceded by 'l' or 'L' are assumed to be type "int".
                    // This assumption is made so the format string definition is compatible with the ANSI C
                    // Specification for formatted strings.  It is recommended that the Base Types be used
                    // everywhere, but in this one case, compliance with ANSI C is more important, and
                    // provides an implementation that is compatible with that largest possible set of CPU
                    // architectures.  This is why the type "int" is used in this one case.
                    //
                    if (BaseListMarker == NULL) {
                        Value = VA_ARG(VaListMarker, int);
                    }
                    else {
                        Value = BASE_ARG(BaseListMarker, int);
                    }
                }
                else {
                    if (BaseListMarker == NULL) {
                        Value = VA_ARG(VaListMarker, INT64);
                    }
                    else {
                        Value = BASE_ARG(BaseListMarker, INT64);
                    }
                }

                if ((Flags & PREFIX_BLANK) != 0) {
                    Prefix = ' ';
                }

                if ((Flags & PREFIX_SIGN) != 0) {
                    Prefix = '+';
                }

                if ((Flags & COMMA_TYPE) != 0) {
                    Comma = TRUE;
                }

                if ((Flags & RADIX_HEX) == 0) {
                    Radix = 10;
                    if (Comma) {
                        Flags &= ~((UINTN)PREFIX_ZERO);
                        Precision = 1;
                    }

                    if ((Value < 0) && ((Flags & UNSIGNED_TYPE) == 0)) {
                        Flags |= PREFIX_SIGN;
                        Prefix = '-';
                        Value = -Value;
                    }
                    else if (((Flags & UNSIGNED_TYPE) != 0) && ((Flags & LONG_TYPE) == 0)) {
                        //
                        // 'd', 'u', 'x', and 'X' that are not preceded by 'l' or 'L' are assumed to be type "int".
                        // This assumption is made so the format string definition is compatible with the ANSI C
                        // Specification for formatted strings.  It is recommended that the Base Types be used
                        // everywhere, but in this one case, compliance with ANSI C is more important, and
                        // provides an implementation that is compatible with that largest possible set of CPU
                        // architectures.  This is why the type "unsigned int" is used in this one case.
                        //
                        Value = (unsigned int)Value;
                    }
                }
                else {
                    Radix = 16;
                    Comma = FALSE;
                    if (((Flags & LONG_TYPE) == 0) && (Value < 0)) {
                        //
                        // 'd', 'u', 'x', and 'X' that are not preceded by 'l' or 'L' are assumed to be type "int".
                        // This assumption is made so the format string definition is compatible with the ANSI C
                        // Specification for formatted strings.  It is recommended that the Base Types be used
                        // everywhere, but in this one case, compliance with ANSI C is more important, and
                        // provides an implementation that is compatible with that largest possible set of CPU
                        // architectures.  This is why the type "unsigned int" is used in this one case.
                        //
                        Value = (unsigned int)Value;
                    }
                }

                //
                // Convert Value to a reversed string
                //
                Count = BasePrintLibValueToString(ValueBuffer, Value, Radix) - ValueBuffer;
                if ((Value == 0) && (Precision == 0)) {
                    Count = 0;
                }

                ArgumentString = (CHAR8*)ValueBuffer + Count;

                Digits = Count % 3;
                if (Digits != 0) {
                    Digits = 3 - Digits;
                }

                if (Comma && (Count != 0)) {
                    Count += ((Count - 1) / 3);
                }

                if (Prefix != 0) {
                    Count++;
                    Precision++;
                }

                Flags |= ARGUMENT_REVERSED;
                ZeroPad = TRUE;
                if ((Flags & PREFIX_ZERO) != 0) {
                    if ((Flags & LEFT_JUSTIFY) == 0) {
                        if ((Flags & PAD_TO_WIDTH) != 0) {
                            if ((Flags & PRECISION) == 0) {
                                Precision = Width;
                            }
                        }
                    }
                }

                break;

            case 's':
            case 'S':
                Flags |= ARGUMENT_UNICODE;
                //
                // break skipped on purpose
                //
            case 'a':
                if (BaseListMarker == NULL) {
                    ArgumentString = VA_ARG(VaListMarker, CHAR8*);
                }
                else {
                    ArgumentString = BASE_ARG(BaseListMarker, CHAR8*);
                }

                if (ArgumentString == NULL) {
                    Flags &= ~((UINTN)ARGUMENT_UNICODE);
                    ArgumentString = "<null string>";
                }

                //
                // Set the default precision for string to be zero if not specified.
                //
                if ((Flags & PRECISION) == 0) {
                    Precision = 0;
                }

                break;

            case 'c':
                if (BaseListMarker == NULL) {
                    Character = VA_ARG(VaListMarker, UINTN) & 0xffff;
                }
                else {
                    Character = BASE_ARG(BaseListMarker, UINTN) & 0xffff;
                }

                ArgumentString = (CHAR8*)&Character;
                Flags |= ARGUMENT_UNICODE;
                break;

            case 'g':
                if (BaseListMarker == NULL) {
                    TmpGuid = VA_ARG(VaListMarker, GUID*);
                }
                else {
                    TmpGuid = BASE_ARG(BaseListMarker, GUID*);
                }

                if (TmpGuid == NULL) {
                    ArgumentString = "<null guid>";
                }
                else {
                    GuidData1 = ReadUnaligned32(&(TmpGuid->Data1));
                    GuidData2 = ReadUnaligned16(&(TmpGuid->Data2));
                    GuidData3 = ReadUnaligned16(&(TmpGuid->Data3));
                    BasePrintLibSPrint(
                        ValueBuffer,
                        MAXIMUM_VALUE_CHARACTERS,
                        0,
                        "%08x-%04x-%04x-%02x%02x-%02x%02x%02x%02x%02x%02x",
                        GuidData1,
                        GuidData2,
                        GuidData3,
                        TmpGuid->Data4[0],
                        TmpGuid->Data4[1],
                        TmpGuid->Data4[2],
                        TmpGuid->Data4[3],
                        TmpGuid->Data4[4],
                        TmpGuid->Data4[5],
                        TmpGuid->Data4[6],
                        TmpGuid->Data4[7]
                    );
                    ArgumentString = ValueBuffer;
                }

                break;

            case 't':
                if (BaseListMarker == NULL) {
                    TmpTime = VA_ARG(VaListMarker, TIME*);
                }
                else {
                    TmpTime = BASE_ARG(BaseListMarker, TIME*);
                }

                if (TmpTime == NULL) {
                    ArgumentString = "<null time>";
                }
                else {
                    BasePrintLibSPrint(
                        ValueBuffer,
                        MAXIMUM_VALUE_CHARACTERS,
                        0,
                        "%02d/%02d/%04d  %02d:%02d",
                        TmpTime->Month,
                        TmpTime->Day,
                        TmpTime->Year,
                        TmpTime->Hour,
                        TmpTime->Minute
                    );
                    ArgumentString = ValueBuffer;
                }

                break;

            case 'r':
                if (BaseListMarker == NULL) {
                    Status = VA_ARG(VaListMarker, RETURN_STATUS);
                }
                else {
                    Status = BASE_ARG(BaseListMarker, RETURN_STATUS);
                }

                ArgumentString = ValueBuffer;
                if (RETURN_ERROR(Status)) {
                    //
                    // Clear error bit
                    //
                    Index = Status & ~MAX_BIT;
                    if ((Index > 0) && (Index <= ERROR_STATUS_NUMBER)) {
                        ArgumentString = mErrorString[Index - 1];
                    }
                }
                else {
                    Index = Status;
                    if (Index <= WARNING_STATUS_NUMBER) {
                        ArgumentString = mWarningString[Index];
                    }
                }

                if (ArgumentString == ValueBuffer) {
                    BasePrintLibSPrint((CHAR8*)ValueBuffer, MAXIMUM_VALUE_CHARACTERS, 0, "%08X", Status);
                }

                break;

            case '\r':
                Format += BytesPerFormatCharacter;
                FormatCharacter = ((*Format & 0xff) | ((BytesPerFormatCharacter == 1) ? 0 : (*(Format + 1) << 8))) & FormatMask;
                if (FormatCharacter == '\n') {
                    //
                    // Translate '\r\n' to '\r\n'
                    //
                    ArgumentString = "\r\n";
                }
                else {
                    //
                    // Translate '\r' to '\r'
                    //
                    ArgumentString = "\r";
                    Format -= BytesPerFormatCharacter;
                }

                break;

            case '\n':
                //
                // Translate '\n' to '\r\n' and '\n\r' to '\r\n'
                //
                ArgumentString = "\r\n";
                Format += BytesPerFormatCharacter;
                FormatCharacter = ((*Format & 0xff) | ((BytesPerFormatCharacter == 1) ? 0 : (*(Format + 1) << 8))) & FormatMask;
                if (FormatCharacter != '\r') {
                    Format -= BytesPerFormatCharacter;
                }

                break;

            case '%':
            default:
                //
                // if the type is '%' or unknown, then print it to the screen
                //
                ArgumentString = (CHAR8*)&FormatCharacter;
                Flags |= ARGUMENT_UNICODE;
                break;
            }

            break;

        case '\r':
            Format += BytesPerFormatCharacter;
            FormatCharacter = ((*Format & 0xff) | ((BytesPerFormatCharacter == 1) ? 0 : (*(Format + 1) << 8))) & FormatMask;
            if (FormatCharacter == '\n') {
                //
                // Translate '\r\n' to '\r\n'
                //
                ArgumentString = "\r\n";
            }
            else {
                //
                // Translate '\r' to '\r'
                //
                ArgumentString = "\r";
                Format -= BytesPerFormatCharacter;
            }

            break;

        case '\n':
            //
            // Translate '\n' to '\r\n' and '\n\r' to '\r\n'
            //
            ArgumentString = "\r\n";
            Format += BytesPerFormatCharacter;
            FormatCharacter = ((*Format & 0xff) | ((BytesPerFormatCharacter == 1) ? 0 : (*(Format + 1) << 8))) & FormatMask;
            if (FormatCharacter != '\r') {
                Format -= BytesPerFormatCharacter;
            }

            break;

        default:
            ArgumentString = (CHAR8*)&FormatCharacter;
            Flags |= ARGUMENT_UNICODE;
            break;
        }

        //
        // Retrieve the ArgumentString attriubutes
        //
        if ((Flags & ARGUMENT_UNICODE) != 0) {
            ArgumentMask = 0xffff;
            BytesPerArgumentCharacter = 2;
        }
        else {
            ArgumentMask = 0xff;
            BytesPerArgumentCharacter = 1;
        }

        if ((Flags & ARGUMENT_REVERSED) != 0) {
            BytesPerArgumentCharacter = -BytesPerArgumentCharacter;
        }
        else {
            //
            // Compute the number of characters in ArgumentString and store it in Count
            // ArgumentString is either null-terminated, or it contains Precision characters
            //
            for (Count = 0;
                (ArgumentString[Count * BytesPerArgumentCharacter] != '\0' ||
                    (BytesPerArgumentCharacter > 1 &&
                        ArgumentString[Count * BytesPerArgumentCharacter + 1] != '\0')) &&
                (Count < Precision || ((Flags & PRECISION) == 0));
                Count++)
            {
                ArgumentCharacter = ((ArgumentString[Count * BytesPerArgumentCharacter] & 0xff) | ((ArgumentString[Count * BytesPerArgumentCharacter + 1]) << 8)) & ArgumentMask;
                if (ArgumentCharacter == 0) {
                    break;
                }
            }
        }

        if (Precision < Count) {
            Precision = Count;
        }

        //
        // Pad before the string
        //
        if ((Flags & (PAD_TO_WIDTH | LEFT_JUSTIFY)) == (PAD_TO_WIDTH)) {
            LengthToReturn += ((Width - Precision) * BytesPerOutputCharacter);
            if (((Flags & COUNT_ONLY_NO_PRINT) == 0) && (Buffer != NULL)) {
                Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, Width - Precision, ' ', BytesPerOutputCharacter);
            }
        }

        if (ZeroPad) {
            if (Prefix != 0) {
                LengthToReturn += (1 * BytesPerOutputCharacter);
                if (((Flags & COUNT_ONLY_NO_PRINT) == 0) && (Buffer != NULL)) {
                    Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, 1, Prefix, BytesPerOutputCharacter);
                }
            }

            LengthToReturn += ((Precision - Count) * BytesPerOutputCharacter);
            if (((Flags & COUNT_ONLY_NO_PRINT) == 0) && (Buffer != NULL)) {
                Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, Precision - Count, '0', BytesPerOutputCharacter);
            }
        }
        else {
            LengthToReturn += ((Precision - Count) * BytesPerOutputCharacter);
            if (((Flags & COUNT_ONLY_NO_PRINT) == 0) && (Buffer != NULL)) {
                Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, Precision - Count, ' ', BytesPerOutputCharacter);
            }

            if (Prefix != 0) {
                LengthToReturn += (1 * BytesPerOutputCharacter);
                if (((Flags & COUNT_ONLY_NO_PRINT) == 0) && (Buffer != NULL)) {
                    Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, 1, Prefix, BytesPerOutputCharacter);
                }
            }
        }

        //
        // Output the Prefix character if it is present
        //
        Index = 0;
        if (Prefix != 0) {
            Index++;
        }

        //
        // Copy the string into the output buffer performing the required type conversions
        //
        while (Index < Count &&
            (ArgumentString[0] != '\0' ||
                (BytesPerArgumentCharacter > 1 && ArgumentString[1] != '\0')))
        {
            ArgumentCharacter = ((*ArgumentString & 0xff) | (((UINT8) * (ArgumentString + 1)) << 8)) & ArgumentMask;

            LengthToReturn += (1 * BytesPerOutputCharacter);
            if (((Flags & COUNT_ONLY_NO_PRINT) == 0) && (Buffer != NULL)) {
                Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, 1, ArgumentCharacter, BytesPerOutputCharacter);
            }

            ArgumentString += BytesPerArgumentCharacter;
            Index++;
            if (Comma) {
                Digits++;
                if (Digits == 3) {
                    Digits = 0;
                    Index++;
                    if (Index < Count) {
                        LengthToReturn += (1 * BytesPerOutputCharacter);
                        if (((Flags & COUNT_ONLY_NO_PRINT) == 0) && (Buffer != NULL)) {
                            Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, 1, ',', BytesPerOutputCharacter);
                        }
                    }
                }
            }
        }

        //
        // Pad after the string
        //
        if ((Flags & (PAD_TO_WIDTH | LEFT_JUSTIFY)) == (PAD_TO_WIDTH | LEFT_JUSTIFY)) {
            LengthToReturn += ((Width - Precision) * BytesPerOutputCharacter);
            if (((Flags & COUNT_ONLY_NO_PRINT) == 0) && (Buffer != NULL)) {
                Buffer = BasePrintLibFillBuffer(Buffer, EndBuffer, Width - Precision, ' ', BytesPerOutputCharacter);
            }
        }

        //
        // Get the next character from the format string
        //
        Format += BytesPerFormatCharacter;

        //
        // Get the next character from the format string
        //
        FormatCharacter = ((*Format & 0xff) | ((BytesPerFormatCharacter == 1) ? 0 : (*(Format + 1) << 8))) & FormatMask;
    }

    if ((Flags & COUNT_ONLY_NO_PRINT) != 0) {
        return (LengthToReturn / BytesPerOutputCharacter);
    }

    ASSERT(Buffer != NULL);
    //
    // Null terminate the Unicode or ASCII string
    //
    BasePrintLibFillBuffer(Buffer, EndBuffer + BytesPerOutputCharacter, 1, 0, BytesPerOutputCharacter);

    return ((Buffer - OriginalBuffer) / BytesPerOutputCharacter);
}

/**
  Worker function that produces a Null-terminated string in an output buffer
  based on a Null-terminated format string and variable argument list.

  VSPrint function to process format and place the results in Buffer. Since a
  VA_LIST is used this routine allows the nesting of Vararg routines. Thus
  this is the main print working routine

  @param  StartOfBuffer The character buffer to print the results of the parsing
                        of Format into.
  @param  BufferSize    The maximum number of characters to put into buffer.
                        Zero means no limit.
  @param  Flags         Initial flags value.
                        Can only have FORMAT_UNICODE and OUTPUT_UNICODE set
  @param  FormatString  A Null-terminated format string.
  @param  ...           The variable argument list.

  @return The number of characters printed.

**/
UINTN
EFIAPI
BasePrintLibSPrint(
    OUT CHAR8* StartOfBuffer,
    IN  UINTN        BufferSize,
    IN  UINTN        Flags,
    IN  CONST CHAR8* FormatString,
    ...
)
{
    VA_LIST  Marker;
    UINTN    NumberOfPrinted;

    VA_START(Marker, FormatString);
    NumberOfPrinted = BasePrintLibSPrintMarker(StartOfBuffer, BufferSize, Flags, FormatString, Marker, NULL);
    VA_END(Marker);
    return NumberOfPrinted;
}



UINTN
EFIAPI
UnicodeVSPrint(
	OUT CHAR16 * StartOfBuffer,
	IN  UINTN         BufferSize,
	IN  CONST CHAR16 * FormatString,
	IN  VA_LIST       Marker
)
{
    ASSERT_STRING(StartOfBuffer!=NULL, L"StartOfBuffer NULL\r\n");
    ASSERT_STRING(FormatString != NULL,L"FormatString NULL\r\n");
	return BasePrintLibSPrintMarker((CHAR8*)StartOfBuffer, BufferSize >> 1, FORMAT_UNICODE | OUTPUT_UNICODE, (CHAR8*)FormatString, Marker, NULL);
}

//call csharp




/**
  Internal function which prints a formatted Unicode string to the console output device
  specified by Console

  This function prints a formatted Unicode string to the console output device
  specified by Console and returns the number of Unicode characters that printed
  to it.  If the length of the formatted Unicode string is greater than PcdUefiLibMaxPrintBufferSize,
  then only the first PcdUefiLibMaxPrintBufferSize characters are sent to Console.
  If Format is NULL, then ASSERT().
  If Format is not aligned on a 16-bit boundary, then ASSERT().

  @param Format   A Null-terminated Unicode format string.
  @param Console  The output console.
  @param Marker   A VA_LIST marker for the variable argument list.

  @return The number of Unicode characters in the produced
          output buffer, not including the Null-terminator.
**/
UINTN
InternalPrint(
    IN  CONST CHAR16* Format,   
    IN  VA_LIST                          Marker
)
{
    //EFI_STATUS  Status;
    UINTN       Return;
    CHAR16* Buffer;
    UINTN       BufferSize;

    ASSERT(Format != NULL);
    ASSERT(((UINTN)Format & BIT0) == 0);
   

    BufferSize = ALIGN_UP(((PcdUefiLibMaxPrintBufferSize) + 1) * sizeof(CHAR16));

    Buffer = (CHAR16*)AllocatePool(BufferSize);
    SAFE_PRINT_CONSTRAINT_CHECK_STRING(Buffer != NULL,0,L"AllocatePool NULL\r\n"); 

    Return = UnicodeVSPrint(Buffer, BufferSize, Format, Marker);

   //call csharp
    OutputStringWrapper(Buffer);

    FreePool(Buffer);

    return Return;
}


/**
  Returns the number of characters that would be produced by if the formatted
  output were produced not including the Null-terminator.

  If FormatString is not aligned on a 16-bit boundary, then ASSERT().

  If FormatString is NULL, then ASSERT() and 0 is returned.
  If PcdMaximumUnicodeStringLength is not zero, and FormatString contains more
  than PcdMaximumUnicodeStringLength Unicode characters not including the
  Null-terminator, then ASSERT() and 0 is returned.

  @param[in]  FormatString    A Null-terminated Unicode format string.
  @param[in]  Marker          VA_LIST marker for the variable argument list.

  @return The number of characters that would be produced, not including the
          Null-terminator.
**/
UINTN
EFIAPI
SPrintLength(
    IN  CONST CHAR16* FormatString,
    IN  VA_LIST       Marker
)
{
    ASSERT_UNICODE_BUFFER(FormatString);
    return BasePrintLibSPrintMarker(NULL, 0, FORMAT_UNICODE | OUTPUT_UNICODE | COUNT_ONLY_NO_PRINT, (CHAR8*)FormatString, Marker, NULL);
}


/**
  Appends a formatted Unicode string to a Null-terminated Unicode string

  This function appends a formatted Unicode string to the Null-terminated
  Unicode string specified by String.   String is optional and may be NULL.
  Storage for the formatted Unicode string returned is allocated using
  AllocatePool().  The pointer to the appended string is returned.  The caller
  is responsible for freeing the returned string.

  If String is not NULL and not aligned on a 16-bit boundary, then ASSERT().
  If FormatString is NULL, then ASSERT().
  If FormatString is not aligned on a 16-bit boundary, then ASSERT().

  @param[in] String         A Null-terminated Unicode string.
  @param[in] FormatString   A Null-terminated Unicode format string.
  @param[in]  Marker        VA_LIST marker for the variable argument list.

  @retval NULL    There was not enough available memory.
  @return         Null-terminated Unicode string is that is the formatted
                  string appended to String.
**/
CHAR16*
EFIAPI
CatVSPrint(
    IN  CHAR16* String  OPTIONAL,
    IN  CONST CHAR16* FormatString,
    IN  VA_LIST       Marker
)
{
    UINTN    CharactersRequired;
    UINTN    SizeRequired;
    CHAR16* BufferToReturn;
    VA_LIST  ExtraMarker;

    VA_COPY(ExtraMarker, Marker);
    CharactersRequired = SPrintLength(FormatString, ExtraMarker);
    VA_END(ExtraMarker);

    if (String != NULL) {
        SizeRequired = StrSize(String) + (CharactersRequired * sizeof(CHAR16));
    }
    else {
        SizeRequired = sizeof(CHAR16) + (CharactersRequired * sizeof(CHAR16));
    }

    BufferToReturn = AllocatePool(SizeRequired);

    if (BufferToReturn == NULL) {
        return NULL;
    }
    else {
        BufferToReturn[0] = L'\0';
    }

    if (String != NULL) {
        StrCpyS(BufferToReturn, SizeRequired / sizeof(CHAR16), String);
    }

    UnicodeVSPrint(BufferToReturn + StrLen(BufferToReturn), (CharactersRequired + 1) * sizeof(CHAR16), FormatString, Marker);

    ASSERT(StrSize(BufferToReturn) == SizeRequired);

    return (BufferToReturn);
}




/**
  Appends a formatted Unicode string to a Null-terminated Unicode string

  This function appends a formatted Unicode string to the Null-terminated
  Unicode string specified by String.   String is optional and may be NULL.
  Storage for the formatted Unicode string returned is allocated using
  AllocatePool().  The pointer to the appended string is returned.  The caller
  is responsible for freeing the returned string.

  If String is not NULL and not aligned on a 16-bit boundary, then ASSERT().
  If FormatString is NULL, then ASSERT().
  If FormatString is not aligned on a 16-bit boundary, then ASSERT().

  @param[in] String         A Null-terminated Unicode string.
  @param[in] FormatString   A Null-terminated Unicode format string.
  @param[in] ...            The variable argument list whose contents are
                            accessed based on the format string specified by
                            FormatString.

  @retval NULL    There was not enough available memory.
  @return         Null-terminated Unicode string is that is the formatted
                  string appended to String.
**/
CHAR16*
EFIAPI
CatSPrint(
    IN  CHAR16* String  OPTIONAL,
    IN  CONST CHAR16* FormatString,
    ...
)
{
    VA_LIST  Marker;
    CHAR16* NewString;

    VA_START(Marker, FormatString);
    NewString = CatVSPrint(String, FormatString, Marker);
    VA_END(Marker);
    return NewString;
}
