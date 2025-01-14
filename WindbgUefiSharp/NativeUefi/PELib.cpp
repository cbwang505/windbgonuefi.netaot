
#include "stdint.h"

#ifdef __cplusplus
extern "C" {
#endif


#include <Uefi.h>
#include <Library/UefiLib.h>

#include "utils.h"

#include "PEHelper.h"

#ifdef __cplusplus
}
#endif
#define EXTERN_C       extern "C"


EXTERN_C extern UINT64 gmodbase;

typedef struct _IMAGE_RUNTIME_FUNCTION_ENTRY {
	DWORD BeginAddress;
	DWORD EndAddress;
	union {
		DWORD UnwindInfoAddress;
		DWORD UnwindData;
	} u;
} _IMAGE_RUNTIME_FUNCTION_ENTRY, * _PIMAGE_RUNTIME_FUNCTION_ENTRY;


typedef struct _IMAGE_RUNTIME_FUNCTION_ENTRY RUNTIME_FUNCTION, * PRUNTIME_FUNCTION;


static  PRUNTIME_FUNCTION pImageRuntimeFunctionDirectory = nullptr;
static  PRUNTIME_FUNCTION pImageRuntimeFunctionDirectoryEnd = nullptr;

static void abort()
{
	while (TRUE)
	{
		stall(10);
	}
	return;
}
typedef enum _UNWIND_OP_CODES {
	UWOP_PUSH_NONVOL = 0, /* info == register number */
	UWOP_ALLOC_LARGE,     /* no info, alloc size in next 2 slots */
	UWOP_ALLOC_SMALL,     /* info == size of allocation / 8 - 1 */
	UWOP_SET_FPREG,       /* no info, FP = RSP + UNWIND_INFO.FPRegOffset*16 */
	UWOP_SAVE_NONVOL,     /* info == register number, offset in next slot */
	UWOP_SAVE_NONVOL_FAR, /* info == register number, offset in next 2 slots */
	UWOP_SAVE_XMM128 = 8, /* info == XMM reg number, offset in next slot */
	UWOP_SAVE_XMM128_FAR, /* info == XMM reg number, offset in next 2 slots */
	UWOP_PUSH_MACHFRAME   /* info == 0: no error-code, 1: error-code */
} UNWIND_CODE_OPS;

typedef unsigned char UBYTE;

typedef union _UNWIND_CODE {
	struct {
		UBYTE CodeOffset;
		UBYTE UnwindOp : 4;
		UBYTE OpInfo : 4;
	};
	USHORT FrameOffset;
} UNWIND_CODE, * PUNWIND_CODE;

#define UNW_FLAG_EHANDLER  0x01
#define UNW_FLAG_UHANDLER  0x02
#define UNW_FLAG_CHAININFO 0x04

typedef struct _UNWIND_INFO {
	UBYTE Version : 3;
	UBYTE Flags : 5;
	UBYTE SizeOfProlog;
	UBYTE CountOfCodes;
	UBYTE FrameRegister : 4;
	UBYTE FrameOffset : 4;
} UNWIND_INFO, * PUNWIND_INFO;

UINT64 RVAToFOA(DWORD ulRva, LPVOID m_pNtHeader, EFI_IMAGE_FILE_HEADER* m_pImageFileHeader)
{
	if (m_pNtHeader && m_pImageFileHeader)
	{
		EFI_IMAGE_SECTION_HEADER* pImageSectionHeader =
			((EFI_IMAGE_SECTION_HEADER*)((ULONG_PTR)(m_pImageFileHeader)+sizeof(EFI_IMAGE_FILE_HEADER) + m_pImageFileHeader->SizeOfOptionalHeader));

		// 遍历节表，查找目标RVA所属的节表，并且算出FOA
		for (int i = 0; i < m_pImageFileHeader->NumberOfSections; i++)
		{
			if ((ulRva >= pImageSectionHeader[i].VirtualAddress) &&
				(ulRva <= pImageSectionHeader[i].VirtualAddress + pImageSectionHeader[i].SizeOfRawData))
			{
				return pImageSectionHeader[i].PointerToRawData + (ulRva - pImageSectionHeader[i].VirtualAddress);
			}
		}
	}

	return 0;
}
static  UINT64 PtrToUlonglong(const void* p)
{
	return (UINT64)p;
}
static void* ULonglongToPtr(ULONG64 ul)
{
	return (void*)ul;
}

#define IfFalseGoExit(x) { br=(x); if (!br) return 0; }

//************************************
// Method:    GetPDBFilePath
// FullName:  GetPDBFilePath
// Access:    public 
// Returns:   VOID
// Qualifier:
// Parameter: char* & strPdbPath
//			 从PE文件中读取调试信息中的PDB信息(如果存在)
//************************************
EXTERN_C UINT64 EFIAPI GetUnwindInfo(UINT8* m_pBuffer)
{

	EFI_IMAGE_DOS_HEADER* m_pImageDosHeader = (EFI_IMAGE_DOS_HEADER*)(m_pBuffer);
	BOOLEAN br = FALSE;

	EFI_IMAGE_FILE_HEADER* m_pImageFileHeader;
	//LPVOID pNtHeaders = NULL;
	DWORD dwE_lfanew = 0;
	DWORD dwPESignature = 0;
	UINT64 ulAddressTemp = 0;
	int m_dwMchine = 0;
	UINT8* m_pNtHeader = 0;
	// 对比MZ签名	
	IfFalseGoExit(IMAGE_DOS_SIGNATURE == m_pImageDosHeader->e_magic);

	// 对比PE签名
	dwE_lfanew = m_pImageDosHeader->e_lfanew & 0x0ffff;
	ulAddressTemp = PtrToUlonglong(m_pBuffer) + dwE_lfanew;

	dwPESignature = *((PDWORD)ULonglongToPtr(ulAddressTemp));
	IfFalseGoExit(IMAGE_NT_SIGNATURE == dwPESignature);

	// 因为还不确定是PE32或PE64，所以先保存IMAGE_NT_HEADER的地址
	m_pNtHeader = (UINT8*)ulAddressTemp;

	// 获取IMAGE_FILE_HEADER，然后判断目标平台CPU类型
	ulAddressTemp = ulAddressTemp + sizeof(IMAGE_NT_SIGNATURE);
	m_pImageFileHeader = (EFI_IMAGE_FILE_HEADER*)ULonglongToPtr(ulAddressTemp);

	if (IMAGE_FILE_MACHINE_I386 == m_pImageFileHeader->Machine)
	{
		// 初步判断为PE32，继续检测OptionalHeader中的Magic
		EFI_IMAGE_NT_HEADERS32* pImageNtHeader32 =
			(EFI_IMAGE_NT_HEADERS32*)m_pNtHeader;

		IfFalseGoExit(
			IMAGE_NT_OPTIONAL_HDR32_MAGIC == pImageNtHeader32->OptionalHeader.Magic);
		// 确定为PE32
		m_dwMchine = IMAGE_FILE_MACHINE_I386;
	}
	else if (IMAGE_FILE_MACHINE_AMD64 == m_pImageFileHeader->Machine)
	{
		// 初步判断为PE64，继续检测OptionalHeader中的Magic
		EFI_IMAGE_NT_HEADERS64* pImageNtHeader64 =
			(EFI_IMAGE_NT_HEADERS64*)m_pNtHeader;

		IfFalseGoExit(
			IMAGE_NT_OPTIONAL_HDR64_MAGIC == pImageNtHeader64->OptionalHeader.Magic);
		// 确定为PE64
		m_dwMchine = IMAGE_FILE_MACHINE_AMD64;
	}
	else
	{
		// 不支持的类型
		IfFalseGoExit(FALSE);
	}



	//char* strPDBSignature;
	//DWORD dwPDBAge = 0;
	if (NULL == m_pNtHeader)
	{
		return 0;
	}
	
	ULONG ulDebugDirectoryRVA = 0;
	int nDirectoryItemCount = 0;
	DWORD dwSize = 0;
	if (IMAGE_FILE_MACHINE_I386 == m_dwMchine)
	{
		EFI_IMAGE_NT_HEADERS32* pImageNtHeader32 =
			(EFI_IMAGE_NT_HEADERS32*)m_pNtHeader;

		ulDebugDirectoryRVA =
			pImageNtHeader32->OptionalHeader.DataDirectory[EFI_IMAGE_DIRECTORY_ENTRY_EXCEPTION].VirtualAddress;
		 dwSize =
			pImageNtHeader32->OptionalHeader.DataDirectory[EFI_IMAGE_DIRECTORY_ENTRY_EXCEPTION].Size;

		nDirectoryItemCount = dwSize / sizeof(IMAGE_DEBUG_DIRECTORY);

	
	}
	else if (IMAGE_FILE_MACHINE_AMD64 == m_dwMchine)
	{
		EFI_IMAGE_NT_HEADERS64* pImageNtHeader64 =
			(EFI_IMAGE_NT_HEADERS64*)m_pNtHeader;

		ulDebugDirectoryRVA =
			pImageNtHeader64->OptionalHeader.DataDirectory[EFI_IMAGE_DIRECTORY_ENTRY_EXCEPTION].VirtualAddress;
		 dwSize =
			pImageNtHeader64->OptionalHeader.DataDirectory[EFI_IMAGE_DIRECTORY_ENTRY_EXCEPTION].Size;

		nDirectoryItemCount = dwSize / sizeof(IMAGE_DEBUG_DIRECTORY);
		
	}
	else
	{
		return 0;
	}



	//UINT64 ulDebugDirectoryFOA = RVAToFOA(ulDebugDirectoryRVA, m_pNtHeader, m_pImageFileHeader) + (UINT64)m_pImageDosHeader;
	UINT64 ulDebugDirectoryFOA = ulDebugDirectoryRVA + (UINT64)m_pImageDosHeader;

	pImageRuntimeFunctionDirectory =
		(PRUNTIME_FUNCTION)ULonglongToPtr(ulDebugDirectoryFOA);

	pImageRuntimeFunctionDirectoryEnd = (PRUNTIME_FUNCTION)ULonglongToPtr(ulDebugDirectoryFOA + dwSize);
	Print(L"ulDebugDirectoryRVA %p RuntimeFunctionDirectory %p RuntimeFunctionDirectoryEnd %p \r\n", ulDebugDirectoryRVA,pImageRuntimeFunctionDirectory, pImageRuntimeFunctionDirectoryEnd);
_Error:

	return 0;
}





#define assert_ehdata(a) if (!(a)) { abort(); }



// Higher-level abstraction of compressed cxxFrameHandler4 data structures

struct UnwindMapEntry4 {
	enum Type
	{
		NoUW = 0b00,
		DtorWithObj = 0b01,
		DtorWithPtrToObj = 0b10,
		RVA = 0b11
	};
	uint32_t        nextOffset;         // State this action takes us to (now in offset form!)
	Type            type;               // Type of entry
	int32_t         action;             // Image relative offset of funclet
	uint32_t        object;             // Frame offset of object pointer to be destructed
};

constexpr uint32_t MAX_CONT_ADDRESSES = 2;

struct HandlerTypeHeader
{
	enum contType
	{
		NONE = 0b00,
		ONE = 0b01,
		TWO = 0b10,
		RESERVED = 0b11
	};
	union
	{
		struct
		{
			uint8_t adjectives : 1; // Existence of Handler Type adjectives (bitfield)
			uint8_t dispType : 1; // Existence of Image relative offset of the corresponding type descriptor
			uint8_t dispCatchObj : 1; // Existence of Displacement of catch object from base
			uint8_t contIsRVA : 1; // Continuation addresses are RVAs rather than function relative, used for separated code
			uint8_t contAddr : 2; // 1.   00: no continuation address in metadata, use what the catch funclet returns
			// 2.   01: one function-relative continuation address
			// 3.   10: two function-relative continuation addresses
			// 4.   11: reserved
			uint8_t unused : 2;
		};
		uint8_t value;
	};
};

static_assert(sizeof(HandlerTypeHeader) == sizeof(uint8_t), "Size of HandlerTypeHeader not 1 Byte");

struct HandlerType4 {
	HandlerTypeHeader header;
	uint32_t          adjectives;                              // Handler Type adjectives (bitfield)
	int32_t           dispType;                                // Image relative offset of the corresponding type descriptor
	uint32_t          dispCatchObj;                            // Displacement of catch object from base
	int32_t           dispOfHandler;                           // Image relative offset of 'catch' code
	uintptr_t         continuationAddress[MAX_CONT_ADDRESSES]; // Continuation address(es) of catch funclet

	void reset()
	{
		header.value = 0;
		adjectives = 0;
		dispType = 0;
		dispCatchObj = 0;
		dispOfHandler = 0;
		hvresetmemory(continuationAddress, sizeof(continuationAddress));
	}

	HandlerType4()
	{
		reset();
	}
};

struct TryBlockMapEntry4 {
	int    tryLow;             // Lowest state index of try
	int    tryHigh;            // Highest state index of try
	int    catchHigh;          // Highest state index of any associated catch
	int32_t        dispHandlerArray;   // Image relative offset of list of handlers for this try
};

struct IPtoStateMapEntry4 {
	int32_t     Ip;     // Image relative offset of IP
	int State;
};

struct SepIPtoStateMapEntry4
{
	int32_t addrStartRVA; // Start address of the function contribution
	int32_t dispOfIPMap;  // RVA to IP map corresponding to this function contribution
};

struct FuncInfoHeader
{
	union
	{
		struct
		{
			uint8_t isCatch : 1;  // 1 if this represents a catch funclet, 0 otherwise
			uint8_t isSeparated : 1;  // 1 if this function has separated code segments, 0 otherwise
			uint8_t BBT : 1;  // Flags set by Basic Block Transformations
			uint8_t UnwindMap : 1;  // Existence of Unwind Map RVA
			uint8_t TryBlockMap : 1;  // Existence of Try Block Map RVA
			uint8_t EHs : 1;  // EHs flag set
			uint8_t NoExcept : 1;  // NoExcept flag set
			uint8_t reserved : 1;
		};
		uint8_t value;
	};

	FuncInfoHeader()
	{
		value = 0;
	}
};

static_assert(sizeof(FuncInfoHeader) == sizeof(uint8_t), "Size of FuncInfoHeader not 1 Byte");

struct FuncInfo4
{
	FuncInfoHeader      header;
	uint32_t            bbtFlags;            // flags that may be set by BBT processing

	int32_t             dispUnwindMap;       // Image relative offset of the unwind map
	int32_t             dispTryBlockMap;     // Image relative offset of the handler map
	int32_t             dispIPtoStateMap;    // Image relative offset of the IP to state map
	uint32_t            dispFrame;           // displacement of address of function frame wrt establisher frame, only used for catch funclets

	FuncInfo4()
	{
		header.value = 0;
		bbtFlags = 0;
		dispUnwindMap = 0;
		dispTryBlockMap = 0;
		dispIPtoStateMap = 0;
		dispFrame = 0;
	}

};

// Constants for decompression.
constexpr int8_t s_negLengthTab[16] =
{
	-1,    // 0
	-2,    // 1
	-1,    // 2
	-3,    // 3

	-1,    // 4
	-2,    // 5
	-1,    // 6
	-4,    // 7

	-1,    // 8
	-2,    // 9
	-1,    // 10
	-3,    // 11

	-1,    // 12
	-2,    // 13
	-1,    // 14
	-5,    // 15
};

constexpr uint8_t s_shiftTab[16] =
{
	32 - 7 * 1,    // 0
	32 - 7 * 2,    // 1
	32 - 7 * 1,    // 2
	32 - 7 * 3,    // 3

	32 - 7 * 1,    // 4
	32 - 7 * 2,    // 5
	32 - 7 * 1,    // 6
	32 - 7 * 4,    // 7

	32 - 7 * 1,    // 8
	32 - 7 * 2,    // 9
	32 - 7 * 1,    // 10
	32 - 7 * 3,    // 11

	32 - 7 * 1,    // 12
	32 - 7 * 2,    // 13
	32 - 7 * 1,    // 14
	0,             // 15
};

inline int32_t ReadInt(uint8_t** buffer)
{
	int value = *(reinterpret_cast<int*>(*buffer));
	*buffer += sizeof(int32_t);
	return value;
}

// TODO: make sure our overflow read is covered by xdata$aa so if we
// end up reading the first entry we don't go over a section.
inline uint32_t ReadUnsigned(uint8_t** pbEncoding)
{
	uint32_t lengthBits = **pbEncoding & 0x0F;
	size_t negLength = s_negLengthTab[lengthBits];
	uint32_t shift = s_shiftTab[lengthBits];
	uint32_t result = *(reinterpret_cast<uint32_t*>(*pbEncoding - negLength - 4));

	result >>= shift;
	*pbEncoding -= negLength;

	return result;
}

inline uint8_t* imageRelToByteBuffer(uintptr_t imageBase, int32_t disp)
{
	return reinterpret_cast<uint8_t*>(imageBase + disp);
}

inline ptrdiff_t DecompFuncInfo(uint8_t* buffer, FuncInfo4& FuncInfoDe, uintptr_t imageBase, int32_t functionStart, bool rawIP2StateRVA)
{
	uint8_t* buffer_start = buffer;
	FuncInfoDe.header.value = buffer[0];
	++buffer;

	if (FuncInfoDe.header.BBT) {
		FuncInfoDe.bbtFlags = ReadUnsigned(&buffer);
	}

	if (FuncInfoDe.header.UnwindMap) {
		FuncInfoDe.dispUnwindMap = ReadInt(&buffer);
	}

	if (FuncInfoDe.header.TryBlockMap) {
		FuncInfoDe.dispTryBlockMap = ReadInt(&buffer);
	}

	if (rawIP2StateRVA)
	{
		FuncInfoDe.dispIPtoStateMap = ReadInt(&buffer);
	}
	else
	{
		// Find the correct one if this is a separated segment
		if (FuncInfoDe.header.isSeparated)
		{
			int dispToSegMap = ReadInt(&buffer);
			uint8_t* segBuffer = imageRelToByteBuffer(imageBase, dispToSegMap);
			uint32_t numSegEntries = ReadUnsigned(&segBuffer);

			// By default, an entry not found in the table corresponds to no
			// states associated with the segment
			FuncInfoDe.dispIPtoStateMap = 0;
			for (uint32_t i = 0; i < numSegEntries; i++)
			{
				int32_t segRVA = ReadInt(&segBuffer);
				int dispSegTable = ReadInt(&segBuffer);
				if (segRVA == functionStart)
				{
					FuncInfoDe.dispIPtoStateMap = dispSegTable;
					break;
				}
			}
		}
		// Otherwise, the table is directly encoded in the function info
		else
		{
			FuncInfoDe.dispIPtoStateMap = ReadInt(&buffer);
		}
	}

	if (FuncInfoDe.header.isCatch) {
		FuncInfoDe.dispFrame = ReadUnsigned(&buffer);
	}

	return buffer - buffer_start;
}

inline ptrdiff_t DecompFuncInfo(uint8_t* buffer, FuncInfo4& FuncInfoDe, uintptr_t imageBase, int32_t functionStart)
{
	return DecompFuncInfo(buffer, FuncInfoDe, imageBase, functionStart, false);
}

// Set of functions and values for compressing into the format used in the image
namespace Compress
{
	static constexpr uint32_t maxRVAs = 4;        // From HandlerMap (dispType, handlerAddr, 2 continuation addresses)
	static constexpr uint32_t maxBufferSize = 27; // Maximum size of a single structure, from HandlerType
	static constexpr uint32_t compIntSize = 5;    // Maximum compressed integer buffer size
	struct RVAoffsets
	{
		uint32_t count;
		uint32_t values[maxRVAs];
	};

	using buffer = uint8_t[maxBufferSize];
	using singleBuffer = uint8_t[compIntSize];

	inline void addOffset(RVAoffsets* offsets, uint32_t& offset)
	{
		assert_ehdata(offsets->count < maxRVAs);

		offsets->values[offsets->count] = offset;
		offsets->count++;

		offset += sizeof(int32_t);
	}

	// .NET uint32_t integer compression scheme:
	// Compresses up to 32 bits into 1-5 bytes, depending on value
	// Lower 4 bits of the MSB determine the number of bytes to read:
	// XXX0: 1 byte
	// XX01: 2 bytes
	// X011: 3 bytes
	// 0111: 4 bytes
	// 1111: 5 bytes
	inline uint32_t getNETencoded(uint8_t* buffer, uint32_t value)
	{
		int i = 0;
		if (value < 128)
		{
			buffer[i++] = (BYTE)((value << 1) + 0);
		}
		else if (value < 128 * 128)
		{
			buffer[i++] = (BYTE)((value << 2) + 1);
			buffer[i++] = (BYTE)(value >> 6);
		}
		else if (value < 128 * 128 * 128)
		{
			buffer[i++] = (BYTE)((value << 3) + 3);
			buffer[i++] = (BYTE)(value >> 5);
			buffer[i++] = (BYTE)(value >> 13);
		}
		else if (value < 128 * 128 * 128 * 128)
		{
			buffer[i++] = (BYTE)((value << 4) + 7);
			buffer[i++] = (BYTE)(value >> 4);
			buffer[i++] = (BYTE)(value >> 12);
			buffer[i++] = (BYTE)(value >> 20);
		}
		else
		{
			buffer[i++] = 15;
			buffer[i++] = (BYTE)(value);
			buffer[i++] = (BYTE)(value >> 8);
			buffer[i++] = (BYTE)(value >> 16);
			buffer[i++] = (BYTE)(value >> 24);
		}

		return i;
	}

	inline uint32_t compFuncInfo4(FuncInfo4 funcInfo, buffer buffer, RVAoffsets* offsets)
	{
		uint32_t index = 0;
		offsets->count = 0;
		buffer[index] = funcInfo.header.value;
		index++;

		if (funcInfo.header.BBT) {
			index += getNETencoded(&buffer[index], funcInfo.bbtFlags);
		}

		if (funcInfo.header.UnwindMap)
		{
			*(reinterpret_cast<int32_t*>(&buffer[index])) = funcInfo.dispUnwindMap;
			addOffset(offsets, index);
		}

		if (funcInfo.header.TryBlockMap)
		{
			*(reinterpret_cast<int32_t*>(&buffer[index])) = funcInfo.dispTryBlockMap;
			addOffset(offsets, index);
		}

		*(reinterpret_cast<int32_t*>(&buffer[index])) = funcInfo.dispIPtoStateMap;
		addOffset(offsets, index);

		if (funcInfo.header.isCatch) {
			index += getNETencoded(&buffer[index], funcInfo.dispFrame);
		}

		assert_ehdata(index < maxBufferSize);
		return index;
	}

	inline uint32_t compUWMapEntry(UnwindMapEntry4 UWEntry, buffer buffer, RVAoffsets* offsets)
	{
		uint32_t index = 0;
		offsets->count = 0;
		// Pack type with next offset
		uint32_t fieldValue = (UWEntry.nextOffset << 2) | static_cast<uint32_t>(UWEntry.type);
		index += getNETencoded(&buffer[index], fieldValue);

		if (UWEntry.type == UnwindMapEntry4::Type::DtorWithObj || UWEntry.type == UnwindMapEntry4::Type::DtorWithPtrToObj)
		{
			*(reinterpret_cast<int32_t*>(&buffer[index])) = UWEntry.action;
			addOffset(offsets, index);

			index += getNETencoded(&buffer[index], UWEntry.object);
		}
		else if (UWEntry.type == UnwindMapEntry4::Type::RVA)
		{
			*(reinterpret_cast<int32_t*>(&buffer[index])) = UWEntry.action;
			addOffset(offsets, index);
		}

		assert_ehdata(index < maxBufferSize);
		return index;
	}

	// Assumes the Ip in ipMap has already been delta-encoded and made function relative.
	// State passed in is the real state.
	inline uint32_t compIP2State(IPtoStateMapEntry4 ipMap, buffer buffer)
	{
		uint32_t index = 0;
		index += getNETencoded(&buffer[index], ipMap.Ip);
		index += getNETencoded(&buffer[index], ipMap.State + 1);

		assert_ehdata(index < maxBufferSize);
		return index;
	}

	inline uint32_t compTryBlockMap(TryBlockMapEntry4 tryBlockEntry, buffer buffer, RVAoffsets* offsets)
	{
		uint32_t index = 0;
		offsets->count = 0;
		index += getNETencoded(&buffer[index], tryBlockEntry.tryLow);
		index += getNETencoded(&buffer[index], tryBlockEntry.tryHigh);
		index += getNETencoded(&buffer[index], tryBlockEntry.catchHigh);

		*(reinterpret_cast<int32_t*>(&buffer[index])) = tryBlockEntry.dispHandlerArray;
		addOffset(offsets, index);

		assert_ehdata(index < maxBufferSize);
		return index;
	}

	inline uint32_t compHandlerMap(HandlerType4 handlerEntry, buffer buffer, RVAoffsets* offsets)
	{
		uint32_t index = 0;
		offsets->count = 0;

		buffer[index] = handlerEntry.header.value;
		index++;
		if (handlerEntry.header.adjectives) {
			index += getNETencoded(&buffer[index], handlerEntry.adjectives);
		}

		if (handlerEntry.header.dispType)
		{
			*(reinterpret_cast<int32_t*>(&buffer[index])) = handlerEntry.dispType;
			addOffset(offsets, index);
		}

		if (handlerEntry.header.dispCatchObj) {
			index += getNETencoded(&buffer[index], handlerEntry.dispCatchObj);
		}

		*(reinterpret_cast<int32_t*>(&buffer[index])) = handlerEntry.dispOfHandler;
		addOffset(offsets, index);

		if (handlerEntry.header.contIsRVA)
		{
			if (handlerEntry.header.contAddr == HandlerTypeHeader::contType::ONE)
			{
				*(reinterpret_cast<int32_t*>(&buffer[index])) = (int32_t)handlerEntry.continuationAddress[0];
				addOffset(offsets, index);
			}
			else if (handlerEntry.header.contAddr == HandlerTypeHeader::contType::TWO)
			{
				*(reinterpret_cast<int32_t*>(&buffer[index])) = (int32_t)handlerEntry.continuationAddress[0];
				addOffset(offsets, index);

				*(reinterpret_cast<int32_t*>(&buffer[index])) = (int32_t)handlerEntry.continuationAddress[1];
				addOffset(offsets, index);
			}
			else {
				// no encoded cont addresses or unknown
			}
		}
		else
		{
			if (handlerEntry.header.contAddr == HandlerTypeHeader::contType::ONE)
			{
				index += getNETencoded(&buffer[index], (uint32_t)handlerEntry.continuationAddress[0]);
			}
			else if (handlerEntry.header.contAddr == HandlerTypeHeader::contType::TWO)
			{
				index += getNETencoded(&buffer[index], (uint32_t)handlerEntry.continuationAddress[0]);
				index += getNETencoded(&buffer[index], (uint32_t)handlerEntry.continuationAddress[1]);
			}
			else {
				// no encoded cont addresses or unknown
			}
		}

		assert_ehdata(index < maxBufferSize);
		return index;
	}
};

// High level representations for easy processing and traversal of low level structures
class UWMap4
{
public:
	UWMap4(const FuncInfo4* pFuncInfo, uintptr_t imageBase)
	{
		if (pFuncInfo->dispUnwindMap != 0)
		{
			uint8_t* buffer = imageRelToByteBuffer(imageBase, pFuncInfo->dispUnwindMap);
			_numEntries = ReadUnsigned(&buffer);
			_bufferStart = buffer;
		}
		else
		{
			_numEntries = 0;
		}
	}

	class iterator
	{
	public:
		iterator(UWMap4& UWMap, uint8_t* currEntry)
			: _UWMap(UWMap), _currEntry(currEntry)
		{}

		void WalkBack()
		{
			_UWMap.WalkBack(&_currEntry);
		}

		iterator& operator++()
		{
			_UWMap.WalkForward(&_currEntry);
			return *this;
		}

		UnwindMapEntry4 operator*()
		{
			uint8_t* origOffset = _currEntry;
			_UWMap.ReadEntry(&_currEntry);
			_currEntry = origOffset;
			return _UWMap._UWEntry;
		}

		iterator& operator=(const iterator& other)
		{
			this->_currEntry = other._currEntry;
			this->_UWMap = other._UWMap;
			return *this;
		}

		bool operator!=(const iterator& other) const
		{
			return (_currEntry != other._currEntry);
		}

		bool operator>(const iterator& other) const
		{
			return (_currEntry > other._currEntry);
		}

		bool operator>=(const iterator& other) const
		{
			return (_currEntry >= other._currEntry);
		}

		ptrdiff_t operator-(const iterator& other) const
		{
			return (_currEntry - other._currEntry);
		}

	private:
		UWMap4& _UWMap;
		uint8_t* _currEntry;
	};

	iterator begin()
	{
		return iterator(*this, _bufferStart);
	}

	iterator end()
	{
		uint8_t* currOffset = _bufferStart;
		for (int i = 0; i < _numEntries; i++)
		{
			ReadEntry(&currOffset);
		}

		return iterator(*this, currOffset);
	}

	void getStartStop(int start, int stop, iterator& startIter, iterator& stopIter)
	{
		uint8_t* startOffset = _bufferStart - 1;
		uint8_t* stopOffset = _bufferStart - 1;
		uint8_t* currOffset = _bufferStart;
		bool foundStart = false;
		bool foundStop = false;
		for (int i = 0; i < _numEntries; i++)
		{
			if (i == start) {
				startOffset = currOffset;
				foundStart = true;
			}

			if (i == stop) {
				stopOffset = currOffset;
				foundStop = true;
			}

			if (foundStart && foundStop) {
				break;
			}

			ReadEntry(&currOffset);
		}

		startIter = iterator(*this, startOffset);
		stopIter = iterator(*this, stopOffset);
	}

	// Using known offsets/state values, find the state of an unknown iterator known to be between
	// lowState and highState
	static int getStateFromIterators(const iterator& lowStateIter, const int lowState,
		const iterator& highStateIter, const int highState, const iterator& toStateIter)
	{
		int state = -1;

		// Out of range
		// TODO: error condition on exceeding max
		// TODO: check to make sure if below min, below by 1 byte
		if (toStateIter > highStateIter || lowStateIter > toStateIter)
		{
			return state;
		}

		// Find from lowState
		if ((toStateIter - lowStateIter) < (highStateIter - toStateIter))
		{
			auto lowStateIterCopy = lowStateIter;
			state = lowState;
			while (toStateIter > lowStateIterCopy)
			{
				++lowStateIterCopy;
				++state;
			}
		}
		// Find from highState
		else
		{
			auto toStateIterCopy = toStateIter;
			state = highState;
			// Can't effectively walk backwards, so walk forwards
			// from the state we want.
			while (highStateIter > toStateIterCopy)
			{
				++toStateIterCopy;
				--state;
			}
		}

		return state;
	}

	uint32_t getNumEntries()
	{
		return _numEntries;
	}

private:
	int _numEntries;
	uint8_t* _bufferStart;
	UnwindMapEntry4 _UWEntry;

	void ReadEntry(uint8_t** currOffset)
	{
		// First field has 2 lower bits indicating type, rest indicating next offset
		_UWEntry.nextOffset = ReadUnsigned(currOffset);
		_UWEntry.type = static_cast<UnwindMapEntry4::Type>(_UWEntry.nextOffset & 0b11);
		_UWEntry.nextOffset >>= 2;

		if (_UWEntry.type == UnwindMapEntry4::Type::DtorWithObj || _UWEntry.type == UnwindMapEntry4::Type::DtorWithPtrToObj)
		{
			_UWEntry.action = ReadInt(currOffset);
			_UWEntry.object = ReadUnsigned(currOffset);
		}
		else if (_UWEntry.type == UnwindMapEntry4::Type::RVA)
		{
			_UWEntry.action = ReadInt(currOffset);
		}
	}

	void WalkBack(uint8_t** currOffset)
	{
		uint8_t* origOffset = *currOffset;

		ReadEntry(currOffset);

		*currOffset = origOffset - _UWEntry.nextOffset;
	}

	void WalkForward(uint8_t** currOffset)
	{
		ReadEntry(currOffset);
	}

};

class TryBlockMap4
{
public:
	TryBlockMap4(const FuncInfo4* pFuncInfo, uintptr_t imageBase)
	{
		if (pFuncInfo->dispTryBlockMap != 0)
		{
			_buffer = imageRelToByteBuffer(imageBase, pFuncInfo->dispTryBlockMap);
			_numTryBlocks = ReadUnsigned(&_buffer);
			// Set reset position after count field
			_bufferStart = _buffer;
			DecompTryBlock();
		}
		else
		{
			_numTryBlocks = 0;
		}
	}

	class iterator
	{
	public:
		iterator(TryBlockMap4& tryBlockMap, uint32_t currBlock)
			: _tryBlockMap(tryBlockMap), _currBlock(currBlock)
		{}

		iterator& operator++()
		{
			_tryBlockMap.DecompTryBlock();
			_currBlock++;
			return *this;
		}

		TryBlockMapEntry4 operator*()
		{
			return _tryBlockMap._tryBlock;
		}

		bool operator!=(const iterator& other) const
		{
			return (_currBlock != other._currBlock);
		}

		bool operator<(const iterator& other) const
		{
			return (_currBlock < other._currBlock);
		}

		iterator& operator=(const iterator& other)
		{
			this->_currBlock = other._currBlock;
			return *this;
		}

		// End iterators can exceed total length but we
		// don't actually want to try and read it.
		void incrementToSentinel()
		{
			_currBlock++;
		}

	private:
		TryBlockMap4& _tryBlockMap;
		uint32_t _currBlock;
	};

	iterator begin()
	{
		return iterator(*this, 0);
	}

	iterator end()
	{
		return iterator(*this, _numTryBlocks);
	}

	uint32_t getNumTryBlocks()
	{
		return _numTryBlocks;
	}

	// Iterators are detached from the main data structure for easy
	// creation of the end iterator. To get meaningful reads from the structure
	// it needs to be set to the start iterator before traversal.
	void setBuffer(iterator iter)
	{
		_buffer = _bufferStart;
		DecompTryBlock();
		for (iterator start = begin(); start != iter; ++start)
		{
			DecompTryBlock();
		}
	}

	//using IteratorPair = std::pair<iterator, iterator>;


	uint32_t _numTryBlocks;
	uint8_t* _buffer;
	uint8_t* _bufferStart;
	TryBlockMapEntry4 _tryBlock;
	// Assumes Number of Try Blocks field has been read out already
	void DecompTryBlock()
	{
		_tryBlock.tryLow = ReadUnsigned(&_buffer);
		_tryBlock.tryHigh = ReadUnsigned(&_buffer);
		_tryBlock.catchHigh = ReadUnsigned(&_buffer);
		_tryBlock.dispHandlerArray = ReadInt(&_buffer);
	}

};

class HandlerMap4
{
public:
	HandlerMap4(const TryBlockMapEntry4* tryMap, uintptr_t imageBase, int32_t functionStart) : _imageBase(imageBase), _functionStart(functionStart)
	{
		if (tryMap->dispHandlerArray != 0)
		{
			_buffer = imageRelToByteBuffer(_imageBase, tryMap->dispHandlerArray);
			_numHandlers = ReadUnsigned(&_buffer);
			_bufferStart = _buffer;
			DecompHandler();
		}
		else
		{
			_numHandlers = 0;
		}
	}

	class iterator
	{
	public:
		iterator(HandlerMap4& handlerMap, uint32_t currBlock)
			: _handlerMap(handlerMap), _currBlock(currBlock)
		{}

		iterator& operator++()
		{
			_handlerMap.DecompHandler();
			_currBlock++;
			return *this;
		}

		iterator operator++(int)
		{
			iterator tmp(*this);
			operator++();
			return tmp;
		}

		HandlerType4 operator*()
		{
			return _handlerMap._handler;
		}

		bool operator==(const iterator& other) const
		{
			return (_currBlock == other._currBlock);
		}

		bool operator!=(const iterator& other) const
		{
			return !(*this == other);
		}

	private:
		HandlerMap4& _handlerMap;
		uint32_t _currBlock;
	};

	iterator begin()
	{
		return iterator(*this, 0);
	}

	iterator end()
	{
		return iterator(*this, _numHandlers);
	}

	void setBuffer(uint32_t index)
	{
		_buffer = _bufferStart;
		DecompHandler();
		for (uint32_t i = 0; i < index; i++) {
			DecompHandler();
		}
	}

	void resetBuffer()
	{
		setBuffer(0);
	}

	uint32_t getNumHandlers()
	{
		return _numHandlers;
	}

	// This is destructive to the current state
	HandlerType4* getLastEntry()
	{
		resetBuffer();
		setBuffer(_numHandlers - 1);
		return &_handler;
	}


	uint32_t _numHandlers;
	uint8_t* _buffer;
	uint8_t* _bufferStart;
	HandlerType4 _handler;
	uintptr_t _imageBase;
	int32_t _functionStart;
	// Assumes Number of Try Blocks field has been read out already
	void DecompHandler()
	{
		_handler.reset();
		_handler.header.value = _buffer[0];
		++_buffer;

		if (_handler.header.adjectives) {
			_handler.adjectives = ReadUnsigned(&_buffer);
		}

		if (_handler.header.dispType) {
			_handler.dispType = ReadInt(&_buffer);
		}

		if (_handler.header.dispCatchObj) {
			_handler.dispCatchObj = ReadUnsigned(&_buffer);
		}

		_handler.dispOfHandler = ReadInt(&_buffer);

		if (_handler.header.contIsRVA)
		{
			if (_handler.header.contAddr == HandlerTypeHeader::contType::ONE) {
				_handler.continuationAddress[0] = ReadInt(&_buffer);
			}
			else if (_handler.header.contAddr == HandlerTypeHeader::contType::TWO) {
				_handler.continuationAddress[0] = ReadInt(&_buffer);
				_handler.continuationAddress[1] = ReadInt(&_buffer);
			}
			else {
				// no encoded cont addresses or unknown
			}
		}
		else
		{
			if (_handler.header.contAddr == HandlerTypeHeader::contType::ONE) {
				_handler.continuationAddress[0] = _functionStart + ReadUnsigned(&_buffer);
			}
			else if (_handler.header.contAddr == HandlerTypeHeader::contType::TWO) {
				_handler.continuationAddress[0] = _functionStart + ReadUnsigned(&_buffer);
				_handler.continuationAddress[1] = _functionStart + ReadUnsigned(&_buffer);
			}
			else {
				// no encoded cont addresses or unknown
			}
		}
	}
};

class IPtoStateMap4
{
public:
	IPtoStateMap4(const FuncInfo4* pFuncInfo, uintptr_t imageBase, uint32_t funcStart) :
		_imageBase(imageBase), _funcStart(funcStart)
	{
		if (pFuncInfo->dispIPtoStateMap)
		{
			uint8_t* buffer = imageRelToByteBuffer(imageBase, pFuncInfo->dispIPtoStateMap);
			_numEntries = ReadUnsigned(&buffer);
			_bufferStart = buffer;
		}
		else
		{
			_numEntries = 0;
		}
	}

	IPtoStateMap4(const SepIPtoStateMapEntry4* mapEntry, uintptr_t imageBase) :
		_imageBase(imageBase)
	{
		_funcStart = mapEntry->addrStartRVA;

		uint8_t* buffer = imageRelToByteBuffer(imageBase, mapEntry->dispOfIPMap);
		_numEntries = ReadUnsigned(&buffer);
		_bufferStart = buffer;
	}

	class iterator
	{
	public:
		iterator(IPtoStateMap4& IptoStateMap, uint32_t entry, uint8_t* offset = nullptr)
			: _IPtoStateMap(IptoStateMap), _entry(entry), _offset(offset), _prevIp(0)
		{}

		iterator& operator++()
		{
			IPtoStateMapEntry4 mapEntry = _IPtoStateMap.decompIP2State(&_offset, _prevIp);
			_entry++;
			_prevIp = mapEntry.Ip - _IPtoStateMap._funcStart;
			return *this;
		}

		iterator operator++(int)
		{
			iterator tmp(*this);
			operator++();
			return tmp;
		}

		IPtoStateMapEntry4 operator*()
		{
			uint8_t* prevOffset = _offset;
			IPtoStateMapEntry4 mapEntry = _IPtoStateMap.decompIP2State(&_offset, _prevIp);
			_offset = prevOffset;
			return mapEntry;
		}

		bool operator==(const iterator& other) const
		{
			return (_entry == other._entry);
		}

		bool operator!=(const iterator& other) const
		{
			return !(*this == other);
		}

		bool operator<(const iterator& other) const
		{
			return (_entry < other._entry);
		}

	private:
		IPtoStateMap4& _IPtoStateMap;
		uint32_t _entry;
		uint8_t* _offset;
		uint32_t _prevIp; // values are delta-encoded, keep track of previous values
	};

	iterator begin()
	{
		return iterator(*this, 0, _bufferStart);
	}

	iterator end()
	{
		return iterator(*this, _numEntries);
	}

	uint32_t getNumEntries()
	{
		return _numEntries;
	}

private:
	uint32_t _numEntries;
	uint8_t* _bufferStart;
	uintptr_t _imageBase;
	uint32_t _funcStart;

	IPtoStateMapEntry4 decompIP2State(uint8_t** currOffset, uint32_t prevIp)
	{
		IPtoStateMapEntry4 IPEntry;
		IPEntry.Ip = _funcStart + prevIp + ReadUnsigned(currOffset);
		// States are encoded +1 so as to not encode a negative
		IPEntry.State = ReadUnsigned(currOffset) - 1;

		return IPEntry;
	}
};


class SepIPtoStateMap4
{
public:
	SepIPtoStateMap4(const FuncInfo4* pFuncInfo, uintptr_t imageBase, int32_t functionStart) :
		_imageBase(imageBase)
	{
		if (pFuncInfo->header.isSeparated == true)
		{
			uint8_t* buffer = imageRelToByteBuffer(imageBase, pFuncInfo->dispIPtoStateMap);
			_numEntries = ReadUnsigned(&buffer);
			_bufferStart = buffer;
			isSingle = false;
		}
		// Supports non separated as well, for easy high level integration
		else
		{
			singleEntry.addrStartRVA = functionStart;
			singleEntry.dispOfIPMap = pFuncInfo->dispIPtoStateMap;
			_numEntries = 1;
			_bufferStart = nullptr;
			isSingle = true;
		}
	}

	class iterator
	{
	public:
		iterator(SepIPtoStateMap4& SepIPtoStateMap, uint32_t entry, uint8_t* offset = nullptr)
			: _SepIPtoStateMap(SepIPtoStateMap), _entry(entry), _offset(offset)
		{}

		iterator& operator++()
		{
			SepIPtoStateMapEntry4 mapEntry = _SepIPtoStateMap.decompSepIp2State(&_offset);
			_entry++;
			return *this;
		}

		iterator operator++(int)
		{
			iterator tmp(*this);
			operator++();
			return tmp;
		}

		SepIPtoStateMapEntry4 operator*()
		{
			uint8_t* prevOffset = _offset;
			SepIPtoStateMapEntry4 mapEntry = _SepIPtoStateMap.decompSepIp2State(&_offset);
			_offset = prevOffset;
			return mapEntry;
		}

		bool operator==(const iterator& other) const
		{
			return (_entry == other._entry);
		}

		bool operator!=(const iterator& other) const
		{
			return !(*this == other);
		}

		bool operator<(const iterator& other) const
		{
			return (_entry < other._entry);
		}

	private:
		SepIPtoStateMap4& _SepIPtoStateMap;
		uint32_t _entry;
		uint8_t* _offset;
	};

	iterator begin()
	{
		return iterator(*this, 0, _bufferStart);
	}

	iterator end()
	{
		return iterator(*this, _numEntries);
	}

private:
	uint32_t _numEntries;
	uint8_t* _bufferStart;
	uintptr_t _imageBase;
	SepIPtoStateMapEntry4 singleEntry;
	bool isSingle;

	SepIPtoStateMapEntry4 decompSepIp2State(uint8_t** buffer)
	{
		SepIPtoStateMapEntry4 ipMap;

		if (isSingle) {
			return singleEntry;
		}

		ipMap.addrStartRVA = ReadInt(buffer);
		ipMap.dispOfIPMap = ReadInt(buffer);

		return ipMap;
	}

};





EXTERN_C  UINT64 EFIAPI FindRuntimeFunction(UINT64 fakefun)
{
	UINT64 m_pBuffer = gmodbase;
	FuncInfo4 FuncInfoDe;		
	UINT32 fakefunstart = (UINT64)fakefun - m_pBuffer;	;
	for (PRUNTIME_FUNCTION RuntimeFunctionEntry = pImageRuntimeFunctionDirectory; RuntimeFunctionEntry < pImageRuntimeFunctionDirectoryEnd; RuntimeFunctionEntry++)
	{
		if (RuntimeFunctionEntry->BeginAddress + m_pBuffer <= fakefun && RuntimeFunctionEntry->EndAddress + m_pBuffer > fakefun)
		{
			//Print(L"BeginAddress %p EndAddress  %p UnwindInfoAddress %p UnwindData %p\r\n", RuntimeFunctionEntry->BeginAddress, RuntimeFunctionEntry->EndAddress, RuntimeFunctionEntry->u.UnwindInfoAddress, RuntimeFunctionEntry->u.UnwindData);

			PUNWIND_INFO UnwindDatainfo = (PUNWIND_INFO)(m_pBuffer + RuntimeFunctionEntry->u.UnwindInfoAddress);

			PUNWIND_CODE UnwindCode = (PUNWIND_CODE)((UINT64)UnwindDatainfo + sizeof(UNWIND_INFO));
			int len = ((UnwindDatainfo->CountOfCodes + 1) & ~1) + 2;
			//int len = UnwindDatainfo->CountOfCodes;

			UINT32 ExceptionData = *(UINT32*)(UnwindCode + len);
			if(ExceptionData==0)
			{
				return 0;
			}
			//Print(L"ExceptionData %p \r\n", ExceptionData);
			uint8_t* buffer = (uint8_t*)m_pBuffer + ExceptionData;
			DecompFuncInfo(buffer, FuncInfoDe, m_pBuffer, RuntimeFunctionEntry->BeginAddress);
			if (!FuncInfoDe.dispTryBlockMap)
			{
				return 0;
			}
		//	Print(L"dispTryBlockMap %p \r\n", FuncInfoDe.dispTryBlockMap);
			TryBlockMap4 trymap(&FuncInfoDe, m_pBuffer);
		//	Print(L"_numTryBlocks %d  dispHandlerArray %p \r\n", trymap._numTryBlocks, trymap._tryBlock.dispHandlerArray);
			if (!trymap._numTryBlocks)
			{
				return 0;
			}
			HandlerMap4 handlermap(&trymap._tryBlock, m_pBuffer, RuntimeFunctionEntry->BeginAddress);		
			if (!handlermap._numHandlers)
			{
				return 0;
			}
			UINT64 realHandler = handlermap._handler.dispOfHandler + m_pBuffer;
			//abort();
			Print(L"_numHandlers %d  dispOfHandler %p continuationAddress %p\r\n", handlermap._numHandlers, handlermap._handler.dispOfHandler, handlermap._handler.continuationAddress[0]);
			return (UINT64)realHandler;
		}
	}
	return 0;

}




EXTERN_C  UINT64 EFIAPI CheckRuntimeFunction(UINT64 fakefun)
{

	UINT64 m_pBuffer = gmodbase;
	for (PRUNTIME_FUNCTION RuntimeFunctionEntry = pImageRuntimeFunctionDirectory; RuntimeFunctionEntry < pImageRuntimeFunctionDirectoryEnd; RuntimeFunctionEntry++)
	{
		if (RuntimeFunctionEntry->BeginAddress + m_pBuffer <= fakefun && RuntimeFunctionEntry->EndAddress + m_pBuffer > fakefun)
		{
			return (UINT64)RuntimeFunctionEntry;

		}
	}
	return 0;
}