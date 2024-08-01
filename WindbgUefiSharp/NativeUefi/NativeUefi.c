//
// Basic UEFI Libraries
//
#include <Uefi.h>
#include <Library/UefiLib.h>
#include <Library/DebugLib.h>
#include <Library/MemoryAllocationLib.h>
#include <Library/BaseMemoryLib.h>

//
// Boot and Runtime Services
//
#include <Library/UefiBootServicesTableLib.h>
#include <Library/UefiRuntimeServicesTableLib.h>

//
// Shell Library
//
#include <Library/ShellLib.h>



#include "stdint.h"
#include "apic-defs.h"
#include "registers.h"
#include "windbg.h"
#include "hvgdk.h"
#include <windbgkd.h>
#include <wdbgexts.h>
#include <ketypes.h>
#include <ke.h>
#include <rtlfuncs.h>
#include <intrin.h>

EFI_BOOT_SERVICES* gBS;

extern UINT64 __Module;
UINT64 gmodbase=0;



typedef enum {
	NativeCom,
	VmbusChannel,
	VmbusMirror
}VmbusWindbgProtocol;

VmbusWindbgProtocol gVmbusWindbgProtocol = VmbusChannel;
//VmbusWindbgProtocol gVmbusWindbgProtocol = NativeCom;


#define KD_SYMBOLS_MAX 0x100
#define EXCEPTION_EXECUTE_HANDLER 1

/* And reload the definitions with these new names */

//#include <kddll.h>


EFI_GUID gEfiWindbgProtocolGUID = { 0xd6ef2483,0xa5de,0x4fa4,{0x8b,0xc3,0x83,0x92,0x50,0xb6,0xff,0xfd} };

EFI_WINDBGPROTOCOL gWindbgProtocol = { 0 };

//extern  DEBUG_MP_CONTEXT volatile  mDebugMpContext;

//extern DEBUG_AGENT_MAILBOX       mMailbox;
//extern DEBUG_AGENT_MAILBOX* mMailboxPointer;
extern IA32_IDT_GATE_DESCRIPTOR  mIdtEntryTable[33];
extern BOOLEAN                   mDxeCoreFlag;
extern BOOLEAN                   mMultiProcessorDebugSupport;
extern VOID* mSavedIdtTable;
extern UINTN                     mSaveIdtTableSize;
BOOLEAN                   mDebugAgentInitialized;
extern BOOLEAN                   mSkipBreakpoint;

EFI_SHELL_PROTOCOL* gEfiShellptr = NULL;

UINT64 VmbusSintIdtWindbgEntry = 0;

UINT16 iretqopcode = 0xcf48;
UINT64 SintVectorRestore = 0;
UINT32 ExceptionStubHeaderSize = 0x13;
UINT32 ReplyreqCache[KD_SYMBOLS_MAX] = { 0 };
typedef struct _KD_SYMBOLS_MAP
{
	UINT64 BaseOfAddr;
	UINT64 MapOfAddr;
	ULONG SizeOfAddr;
} KD_SYMBOLS_MAP, * PKD_SYMBOLS_MAP;

#define ctxchgimpl(a,b,x,y) \
a->x = b->y

#define ctxchg(a,b,x,y,reverse) \
if(reverse==TRUE)\
{\
	ctxchgimpl(b, a, y,x);\
}else\
{\
	ctxchgimpl(a, b,x,y);\
}



#define ctxchg64to16(a,b,x,y,reverse) \
if(reverse==TRUE)\
{\
	ctxchgimpl(b, a, y,x);\
}else\
{\
	a->x = (UINT16)b->y;\
}

#define ctxchgsame(a,b,x,reverse) \
if(reverse==TRUE)\
{\
	ctxchgimpl(b, a, x, x);\
}else\
{\
	ctxchgimpl(a, b, x, x);\
}




#define CHECKASSERT(Expression,Text)\
if(Expression)\
{\
	Print(L"%s %s %s\r\n", __FILE__, __LINE__, Text);\
}
/* GLOBALS ********************************************************************/
//
// Buffers
//
CHAR KdpMessageBuffer[KDP_MSG_BUFFER_SIZE];
CHAR KdpPathBuffer[KDP_MSG_BUFFER_SIZE];
CHAR KdpPathAuxBuffer[KDP_MSG_BUFFER_SIZE];
CHAR KdpPathPrintBuffer[KDP_MSG_BUFFER_SIZE];
CHAR KdpPathSymbolBuffer[KDP_MSG_BUFFER_SIZE];
CHAR KdpPathSafeBuffer[KDP_MSG_BUFFER_SIZE];
BOOLEAN ForceConsoleOutput = FALSE;
BOOLEAN ForcePorteOutput = FALSE;
ULONG CurrentPacketId = INITIAL_PACKET_ID | SYNC_PACKET_ID;
ULONG RemotePacketId = INITIAL_PACKET_ID;
BOOLEAN KdpContextSent = FALSE;
BOOLEAN KdpContextSyncPacket = FALSE;
BOOLEAN KdDebuggerNotPresent = FALSE;
BOOLEAN VmbusServiceProtocolLoaded = FALSE;
BOOLEAN VmbusKdInitSystemLoaded = FALSE;
int ReportSynthetic = 0;
//
BOOLEAN KdBreakAfterSymbolLoad;
BOOLEAN KdPitchDebugger = FALSE;

BOOLEAN KdDebuggerEnabled = TRUE;
BOOLEAN KdAutoEnableOnEvent;
BOOLEAN KdBlockEnable;
BOOLEAN KdIgnoreUmExceptions;
BOOLEAN KdPreviouslyEnabled;
BOOLEAN KdpDebuggerStructuresInitialized;
BOOLEAN KdEnteredDebugger;
BOOLEAN KdpPortLocked;
KIRQL KdpNowKIRQL = PASSIVE_LEVEL;
ULONG KdDisableCount;
LARGE_INTEGER KdPerformanceCounterRate;

LONG KdpTimeSlipPending = 1;
PVOID KdpTimeSlipEvent;
//KSPIN_LOCK KdpTimeSlipEventLock;
LARGE_INTEGER KdTimerStop, KdTimerStart, KdTimerDifference;

//
BREAKPOINT_ENTRY KdpBreakpointTable[KD_BREAKPOINT_MAX]={0};
KD_BREAKPOINT_TYPE KdpBreakpointInstruction = KD_BREAKPOINT_VALUE;
BOOLEAN KdpOweBreakpoint;
BOOLEAN BreakpointsSuspended;
BOOLEAN KdpControlCPressed;
/* NT System Info */
ULONG NtGlobalFlag = 0;
ULONG ExSuiteMask;
ULONG KdpNumInternalBreakpoints;
KD_CONTEXT KdpContext = { 0 };
//
// Symbol Data
//
ULONG_PTR KdpCurrentSymbolStart, KdpCurrentSymbolEnd;

// KdPrint Buffers
//
CHAR KdPrintDefaultCircularBuffer[KD_DEFAULT_LOG_BUFFER_SIZE];
PCHAR KdPrintWritePointer = KdPrintDefaultCircularBuffer;
ULONG KdPrintRolloverCount;
PCHAR KdPrintCircularBuffer = KdPrintDefaultCircularBuffer;
ULONG KdPrintBufferSize = sizeof(KdPrintDefaultCircularBuffer);
ULONG KdPrintBufferChanges = 0;


KIPCR gPcr = { 0 };
KPRCB gPrcb = { 0 };
UEFI_SYMBOLS_INFO mSyntheticSymbolInfo[KD_SYMBOLS_MAX] = { 0 };

PKD_PACKETEXTRA pPengdingManipulatePacket = NULL;
UINT32  FailedOperateMemoryCount = 0;
UINT64  FailedOperateMemoryAddress1 = 0;
UINT64  FailedOperateMemoryAddress2 = 0;
UINT64  FailedOperateMemoryAddressArray[KD_SYMBOLS_MAX] = { 0 };

KD_SYMBOLS_MAP gsymmap[KD_SYMBOLS_MAX] = { 0 };
static BOOLEAN termconfirmed = FALSE;
static BOOLEAN reportshellefi = FALSE;
BOOLEAN SintVectorModify = TRUE;

__declspec(align(VSM_PAGE_SIZE)) UINT8 vmbus_output_page[VSM_PAGE_SIZE_DOUBLE];
__declspec(align(VSM_PAGE_SIZE)) UINT8 vmbus_input_page[VSM_PAGE_SIZE_DOUBLE];

volatile UINT32 vmbus_output_start = 0;

volatile UINT32 vmbus_input_start = 0;
volatile UINT32 vmbus_input_end = 0;
volatile UINT32 vmbus_input_len = 0;
//

DBGKD_GET_VERSION64 KdVersionBlock =
{
	0xf,
	0x4a61,
	DBGKD_64BIT_PROTOCOL_VERSION2,
	KD_SECONDARY_VERSION_AMD64_CONTEXT,
	0x47,
	IMAGE_FILE_MACHINE_NATIVE,
	PACKET_TYPE_MAX,
	3,
	0x33,
	DBGKD_SIMULATION_NONE,
	{0},
	0,
	0,
	0
};
//
extern UINT64 gapicpage;

void fakedump();
void hdlmsgint();
void TimerInterruptHandle();
void Exception0Handle();
VOID
EFIAPI
SendApicEoi(
	VOID
);

VOID
EFIAPI
DumpThis();

VOID
NTAPI
AddCachedMemoryRegion(UINT64 MapAddress, UINT64 Buffer, UINT32 len);

VOID
NTAPI
KdpSendControlPacket(
	IN USHORT PacketType,
	IN ULONG PacketId OPTIONAL);


void  ConsoleOutputString(CHAR16* Buffer);

EFI_STATUS NTAPI HvVmbusServiceDxeInitialize();
NTSTATUS NTAPI InitGlobalHv();
KCONTINUE_STATUS
NTAPI KdpSymbolReportSynthetic(PUEFI_SYMBOLS_INFO pSyntheticSymbolInfo);

static void abort()
{
	while (TRUE)
	{
		stall(10);
	}
	return;
}
BOOLEAN
NTAPI
KdEnterDebugger(IN PKTRAP_FRAME TrapFrame,
	IN PKEXCEPTION_FRAME ExceptionFrame)
{

	return TRUE;
}
VOID
NTAPI
KdExitDebugger(IN BOOLEAN Enable)
{
	return;
}

BOOLEAN UefiMemoryCanFetchByType(EFI_MEMORY_TYPE Type)
{    //case EfiPersistentMemory:
		//case EfiConventionalMemory:
	switch (Type)
	{
	case EfiLoaderCode:
		//case EfiLoaderData:
	case EfiBootServicesCode:
		//case EfiBootServicesData:
	case EfiRuntimeServicesCode:
		//case EfiRuntimeServicesData:
	case EfiReservedMemoryType:
	{
		return TRUE;
	}
	default:
	{

		return FALSE;
	}

	}
	//return FALSE;
}


BOOLEAN UefiMemoryPresent(UINT64 StartingAddr, UINTN Size)
{
	EFI_MEMORY_DESCRIPTOR* MemoryMap;
	EFI_MEMORY_DESCRIPTOR* MemoryMapEntry;
	EFI_MEMORY_DESCRIPTOR* MemoryMapEnd;
	UINTN    MemoryMapSize;
	UINTN    MapKey;
	UINTN    DescriptorSize;
	UINT32   DescriptorVersion;
	//UINT64   CurrentData;
	//UINT8    Checksum;
	BOOLEAN  MemoryFound;
	EFI_STATUS                   Status;
	BOOLEAN forcedump = FALSE;
	//
	// Get the EFI memory map.
	//
	MemoryMapSize = 0;
	MemoryMap = NULL;
	MemoryFound = FALSE;
	/*if (!LowCheckMemoryAddr(StartingAddr))
	{
		return FALSE;
	}*/
	Status = gBS->GetMemoryMap(
		&MemoryMapSize,
		MemoryMap,
		&MapKey,
		&DescriptorSize,
		&DescriptorVersion
	);
	ASSERT(Status == EFI_BUFFER_TOO_SMALL);
	do {
		MemoryMap = (EFI_MEMORY_DESCRIPTOR*)AllocatePool(MemoryMapSize);
		ASSERT(MemoryMap != NULL);
		Status = gBS->GetMemoryMap(
			&MemoryMapSize,
			MemoryMap,
			&MapKey,
			&DescriptorSize,
			&DescriptorVersion
		);
		if (EFI_ERROR(Status)) {
			FreePool(MemoryMap);
			MemoryMap = NULL;
		}
	} while (Status == EFI_BUFFER_TOO_SMALL);

	ASSERT_EFI_ERROR(Status);
	if (EFI_ERROR(Status)) {
		if (MemoryMap)
		{
			FreePool(MemoryMap);
			MemoryMap = NULL;
		}

		return FALSE;
	}
	MemoryMapEntry = MemoryMap;
	MemoryMapEnd = (EFI_MEMORY_DESCRIPTOR*)((UINT64)MemoryMap + MemoryMapSize);
	while ((UINTN)MemoryMapEntry < (UINTN)MemoryMapEnd) {
		if ((MemoryMapEntry->PhysicalStart <= StartingAddr) &&
			((MemoryMapEntry->PhysicalStart +
				MultU64x32(MemoryMapEntry->NumberOfPages, EFI_PAGE_SIZE))
				>= (StartingAddr + Size)))
		{

			if (forcedump)
			{
				UINT64 startval = MemoryMapEntry->PhysicalStart;
				UINT64 endval = MemoryMapEntry->PhysicalStart +
					MultU64x32(MemoryMapEntry->NumberOfPages, EFI_PAGE_SIZE);

				KdpDprintf(L"UefiMemoryPresent mSyntheticSymbolInfo StartingAddr % p start %p  end %p type %08x attr %08x size %08x ok\r\n", StartingAddr, startval, endval, MemoryMapEntry->Type, MemoryMapEntry->Attribute, MemoryMapSize);
			}

			if ((UefiMemoryCanFetchByType((EFI_MEMORY_TYPE)MemoryMapEntry->Type) == TRUE))
			{
				MemoryFound = TRUE;
			}
			else
			{
				MemoryFound = FALSE;
			}

			break;
		}

		MemoryMapEntry = NEXT_MEMORY_DESCRIPTOR(MemoryMapEntry, DescriptorSize);
	}
	if (MemoryMap)
	{
		FreePool(MemoryMap);
		MemoryMap = NULL;
	}
	return MemoryFound;
}

void  DumpStackTracFrame(UINT64 ripval, UINT64 rspval, UINT64 startval, UINT64 endval);

BOOLEAN UefiMemoryStackTrace(UINT64 ripval, UINT64 rspval)
{
	EFI_MEMORY_DESCRIPTOR* MemoryMap;
	EFI_MEMORY_DESCRIPTOR* MemoryMapEntry;
	EFI_MEMORY_DESCRIPTOR* MemoryMapEnd;
	UINTN    MemoryMapSize;
	UINTN    MapKey;
	UINTN    DescriptorSize;
	UINT32   DescriptorVersion;
	//UINT64   CurrentData;
	//UINT8    Checksum;
	BOOLEAN  MemoryFound;
	EFI_STATUS                   Status;
	UINTN Size = 0;
	UINT64 StartingAddr = rspval;
	//
	// Get the EFI memory map.
	//
	MemoryMapSize = 0;
	MemoryMap = NULL;
	MemoryFound = FALSE;
	/*if (!LowCheckMemoryAddr(StartingAddr))
	{
		return FALSE;
	}*/
	Status = gBS->GetMemoryMap(
		&MemoryMapSize,
		MemoryMap,
		&MapKey,
		&DescriptorSize,
		&DescriptorVersion
	);
	ASSERT(Status == EFI_BUFFER_TOO_SMALL);
	do {
		MemoryMap = (EFI_MEMORY_DESCRIPTOR*)AllocatePool(MemoryMapSize);
		ASSERT(MemoryMap != NULL);
		Status = gBS->GetMemoryMap(
			&MemoryMapSize,
			MemoryMap,
			&MapKey,
			&DescriptorSize,
			&DescriptorVersion
		);
		if (EFI_ERROR(Status)) {
			FreePool(MemoryMap);
			MemoryMap = NULL;
		}
	} while (Status == EFI_BUFFER_TOO_SMALL);

	ASSERT_EFI_ERROR(Status);
	if (EFI_ERROR(Status)) {
		if (MemoryMap)
		{
			FreePool(MemoryMap);
			MemoryMap = NULL;
		}

		return FALSE;
	}
	MemoryMapEntry = MemoryMap;
	MemoryMapEnd = (EFI_MEMORY_DESCRIPTOR*)((UINT64)MemoryMap + MemoryMapSize);
	while ((UINTN)MemoryMapEntry < (UINTN)MemoryMapEnd) {
		if ((MemoryMapEntry->PhysicalStart <= StartingAddr) &&
			((MemoryMapEntry->PhysicalStart +
				MultU64x32(MemoryMapEntry->NumberOfPages, EFI_PAGE_SIZE))
				>= (StartingAddr + Size)))
		{


			UINT64 startval = MemoryMapEntry->PhysicalStart;
			UINT64 endval = MemoryMapEntry->PhysicalStart +
				MultU64x32(MemoryMapEntry->NumberOfPages, EFI_PAGE_SIZE);

			KdpDprintf(L"UefiMemoryStackTrace rip %p rsp %p start %p end %p type %08x attr %08x size %08x ok\r\n", ripval, StartingAddr, startval, endval, MemoryMapEntry->Type, MemoryMapEntry->Attribute, MemoryMapSize);

			DumpStackTracFrame(ripval, StartingAddr, startval, endval);
			break;


		}

		MemoryMapEntry = NEXT_MEMORY_DESCRIPTOR(MemoryMapEntry, DescriptorSize);
	}
	if (MemoryMap)
	{
		FreePool(MemoryMap);
		MemoryMap = NULL;
	}
	return MemoryFound;
}

ULONG GetProcessorIndex()
{
	return 0;
}

VOID
NTAPI
KdpMoveMemory(
	_In_ PVOID Destination,
	_In_ PVOID Source,
	_In_ UINT32 Length)
{

	hvcopymemory(Destination, Source, Length);
	return;
}
VOID
NTAPI
KdpZeroMemory(
	_In_ PVOID Destination,
	_In_ UINT32 Length)
{
	hvresetmemory(Destination, Length);
	return;

}
NTSTATUS
NTAPI
MmDbgCopyMemory(IN ULONG64 Address,
	IN PVOID Buffer,
	IN ULONG Size,
	IN ULONG Flags)
{

	PVOID CopyDestination, CopySource;
	/* Check what kind of operation this is */
	if (Flags & MMDBG_COPY_WRITE)
	{
		/* Write */
		CopyDestination = (PVOID)Address;
		CopySource = (PVOID)Buffer;
	}
	else
	{
		/* Read */
		CopyDestination = (PVOID)Buffer;
		CopySource = (PVOID)Address;
	}

	KdpMoveMemory(CopyDestination, CopySource, Size);
	return STATUS_SUCCESS;

}
NTSTATUS
NTAPI
KdpCopyMemoryChunks(
	_In_ ULONG64 Address,
	_In_ PVOID Buffer,
	_In_ ULONG TotalSize,
	_In_ ULONG ChunkSize,
	_In_ ULONG Flags,
	_Out_opt_ PULONG ActualSize)
{
	NTSTATUS Status;
	ULONG RemainingLength, CopyChunk;

	/* Check if we didn't get a chunk size or if it is too big */
	if (ChunkSize == 0)
	{
		/* Default to 4 byte chunks */
		ChunkSize = 4;
	}
	else if (ChunkSize > MMDBG_COPY_MAX_SIZE)
	{
		/* Normalize to maximum size */
		ChunkSize = MMDBG_COPY_MAX_SIZE;
	}

	/* Copy the whole range in aligned chunks */
	RemainingLength = TotalSize;
	CopyChunk = 1;
	while (RemainingLength > 0)
	{
		/*
		 * Determine the best chunk size for this round.
		 * The ideal size is aligned, isn't larger than the
		 * the remaining length and respects the chunk limit.
		 */
		while (((CopyChunk * 2) <= RemainingLength) &&
			(CopyChunk < ChunkSize) &&
			((Address & ((CopyChunk * 2) - 1)) == 0))
		{
			/* Increase it */
			CopyChunk *= 2;
		}

		/*
		 * The chunk size can be larger than the remaining size if this
		 * isn't the first round, so check if we need to shrink it back.
		 */
		while (CopyChunk > RemainingLength)
		{
			/* Shrink it */
			CopyChunk /= 2;
		}

		/* Do the copy */
		Status = MmDbgCopyMemory(Address, Buffer, CopyChunk, Flags);
		if (!NT_SUCCESS(Status))
		{
			/* Copy failed, break out */
			break;
		}

		/* Update pointers and length for the next run */
		Address = Address + CopyChunk;
		Buffer = (PVOID)((ULONG_PTR)Buffer + CopyChunk);
		RemainingLength = RemainingLength - CopyChunk;
	}

	/* We may have modified executable code, flush the instruction cache */
	KeSweepICache((PVOID)(ULONG_PTR)Address, TotalSize);

	/*
	 * Return the size we managed to copy and return
	 * success if we could copy the whole range.
	 */
	if (ActualSize) *ActualSize = TotalSize - RemainingLength;
	return RemainingLength == 0 ? STATUS_SUCCESS : STATUS_UNSUCCESSFUL;
}



ULONG
NTAPI
KdpAddBreakpoint(IN PVOID Address)
{
	KD_BREAKPOINT_TYPE Content;
	ULONG i;
	NTSTATUS Status;

	/* Check whether we are not setting a breakpoint twice */
	for (i = 0; i < KD_BREAKPOINT_MAX; i++)
	{
		/* Check if the breakpoint is valid */
		if ((KdpBreakpointTable[i].Flags & KD_BREAKPOINT_ACTIVE) &&
			(KdpBreakpointTable[i].Address == Address))
		{
			/* Were we not able to remove it earlier? */
			if (KdpBreakpointTable[i].Flags & KD_BREAKPOINT_EXPIRED)
			{
				/* Just re-use it! */
				KdpBreakpointTable[i].Flags &= ~KD_BREAKPOINT_EXPIRED;
				return i + 1;
			}
			else
			{
				/* Fail */
				return 0;
			}
		}
	}

	/* Find a free entry */
	for (i = 0; i < KD_BREAKPOINT_MAX; i++)
	{
		if (KdpBreakpointTable[i].Flags == 0)
			break;
	}

	/* Fail if no free entry was found */
	if (i == KD_BREAKPOINT_MAX) return 0;

	/* Save the breakpoint */
	KdpBreakpointTable[i].Address = Address;

	/* If we are setting the breakpoint in user space, save the active process context */
	/*if (Address < KD_HIGHEST_USER_BREAKPOINT_ADDRESS)
		KdpBreakpointTable[i].DirectoryTableBase = KeGetCurrentThread()->ApcState.Process->DirectoryTableBase[0];
		*/

		/* Try to save the old instruction */
	Status = KdpCopyMemoryChunks((ULONG_PTR)Address,
		&Content,
		KD_BREAKPOINT_SIZE,
		0,
		MMDBG_COPY_UNSAFE,
		NULL);
	if (NT_SUCCESS(Status))
	{
		/* Memory accessible, set the breakpoint */
		KdpBreakpointTable[i].Content = Content;
		KdpBreakpointTable[i].Flags = KD_BREAKPOINT_ACTIVE;

		/* Write the breakpoint */
		Status = KdpCopyMemoryChunks((ULONG_PTR)Address,
			&KdpBreakpointInstruction,
			KD_BREAKPOINT_SIZE,
			0,
			MMDBG_COPY_UNSAFE | MMDBG_COPY_WRITE,
			NULL);
		if (!NT_SUCCESS(Status))
		{
			/* This should never happen */
			KdpDprintf(L"Unable to write breakpoint to address 0x%p\n", Address);
			return 0;
		}
		UINT8 KdpBreakpointInstructionChk = *((UINT8*)Address);
		if (KdpBreakpointInstructionChk != (UINT8)KdpBreakpointInstruction)
		{
			KdpDprintf(L"Unable to write breakpoint to address 0x%p,unmatch KdpBreakpointInstruction\n", Address);
			return 0;
		}



	}
	else
	{
		/* Memory is inaccessible now, setting breakpoint is deferred */
		KdpDprintf(L"Failed to set breakpoint at address 0x%p, adding deferred breakpoint.\n", Address);
		KdpBreakpointTable[i].Flags = KD_BREAKPOINT_ACTIVE | KD_BREAKPOINT_PENDING;
		KdpOweBreakpoint = TRUE;
		return 0;
	}

	/* Return the breakpoint handle */
	return i + 1;
}

VOID
NTAPI
KdSetOwedBreakpoints(VOID)
{
	BOOLEAN Enable;
	KD_BREAKPOINT_TYPE Content;
	ULONG i;
	NTSTATUS Status;

	/* If we don't owe any breakpoints, just return */
	if (!KdpOweBreakpoint) return;

	/* Enter the debugger */
	Enable = KdEnterDebugger(NULL, NULL);

	/*
	 * Suppose we succeed in setting all the breakpoints.
	 * If we fail to do so, the flag will be set again.
	 */
	KdpOweBreakpoint = FALSE;

	/* Loop through current breakpoints and try to set or delete the pending ones */
	for (i = 0; i < KD_BREAKPOINT_MAX; i++)
	{
		if (KdpBreakpointTable[i].Flags & (KD_BREAKPOINT_PENDING | KD_BREAKPOINT_EXPIRED))
		{
			/*
			 * Set the breakpoint only if it is in kernel space, or if it is
			 * in user space and the active process context matches.
			 */
			if (KdpBreakpointTable[i].Address < KD_HIGHEST_USER_BREAKPOINT_ADDRESS)
				//&&KdpBreakpointTable[i].DirectoryTableBase != KeGetCurrentThread()->ApcState.Process->DirectoryTableBase[0])
			{
				KdpOweBreakpoint = TRUE;
				continue;
			}

			/* Try to save the old instruction */
			Status = KdpCopyMemoryChunks((ULONG_PTR)KdpBreakpointTable[i].Address,
				&Content,
				KD_BREAKPOINT_SIZE,
				0,
				MMDBG_COPY_UNSAFE,
				NULL);
			if (!NT_SUCCESS(Status))
			{
				/* Memory is still inaccessible, breakpoint setting will be deferred again */
				// KdpDprintf(L"Failed to set deferred breakpoint at address 0x%p\n",
				//            KdpBreakpointTable[i].Address);
				KdpOweBreakpoint = TRUE;
				continue;
			}

			/* Check if we need to write the breakpoint */
			if (KdpBreakpointTable[i].Flags & KD_BREAKPOINT_PENDING)
			{
				/* Memory accessible, set the breakpoint */
				KdpBreakpointTable[i].Content = Content;

				/* Write the breakpoint */
				Status = KdpCopyMemoryChunks((ULONG_PTR)KdpBreakpointTable[i].Address,
					&KdpBreakpointInstruction,
					KD_BREAKPOINT_SIZE,
					0,
					MMDBG_COPY_UNSAFE | MMDBG_COPY_WRITE,
					NULL);
				if (!NT_SUCCESS(Status))
				{
					/* This should never happen */
					KdpDprintf(L"Unable to write deferred breakpoint to address 0x%p\n",
						KdpBreakpointTable[i].Address);
					KdpOweBreakpoint = TRUE;
				}
				else
				{
					KdpBreakpointTable[i].Flags = KD_BREAKPOINT_ACTIVE;
				}

				continue;
			}

			/* Check if we need to restore the original instruction */
			if (KdpBreakpointTable[i].Flags & KD_BREAKPOINT_EXPIRED)
			{
				/* Write it back */
				Status = KdpCopyMemoryChunks((ULONG_PTR)KdpBreakpointTable[i].Address,
					&KdpBreakpointTable[i].Content,
					KD_BREAKPOINT_SIZE,
					0,
					MMDBG_COPY_UNSAFE | MMDBG_COPY_WRITE,
					NULL);
				if (!NT_SUCCESS(Status))
				{
					/* This should never happen */
					KdpDprintf(L"Unable to delete deferred breakpoint at address 0x%p\n",
						KdpBreakpointTable[i].Address);
					KdpOweBreakpoint = TRUE;
				}
				else
				{
					/* Check if the breakpoint is suspended */
					if (KdpBreakpointTable[i].Flags & KD_BREAKPOINT_SUSPENDED)
					{
						KdpBreakpointTable[i].Flags = KD_BREAKPOINT_SUSPENDED | KD_BREAKPOINT_ACTIVE;
					}
					else
					{
						/* Invalidate it */
						KdpBreakpointTable[i].Flags = 0;
					}
				}

				continue;
			}
		}
	}

	/* Exit the debugger */
	KdExitDebugger(Enable);
}

BOOLEAN
NTAPI
KdpLowWriteContent(IN ULONG BpIndex)
{
	NTSTATUS Status;

	/* Make sure that the breakpoint is actually active */
	if (KdpBreakpointTable[BpIndex].Flags & KD_BREAKPOINT_PENDING)
	{
		/* So we have a valid breakpoint, but it hasn't been used yet... */
		KdpBreakpointTable[BpIndex].Flags &= ~KD_BREAKPOINT_PENDING;
		return TRUE;
	}

	/* Is the original instruction a breakpoint anyway? */
	if (KdpBreakpointTable[BpIndex].Content == KdpBreakpointInstruction)
	{
		/* Then leave it that way... */
		return TRUE;
	}

	/* We have an active breakpoint with an instruction to bring back. Do it. */
	Status = KdpCopyMemoryChunks((ULONG_PTR)KdpBreakpointTable[BpIndex].Address,
		&KdpBreakpointTable[BpIndex].Content,
		KD_BREAKPOINT_SIZE,
		0,
		MMDBG_COPY_UNSAFE | MMDBG_COPY_WRITE,
		NULL);
	if (!NT_SUCCESS(Status))
	{
		/* Memory is inaccessible now, restoring original instruction is deferred */
		// KdpDprintf(L"Failed to delete breakpoint at address 0x%p\n",
		//            KdpBreakpointTable[BpIndex].Address);
		KdpBreakpointTable[BpIndex].Flags |= KD_BREAKPOINT_EXPIRED;
		KdpOweBreakpoint = TRUE;
		return FALSE;
	}

	/* Everything went fine, return */
	return TRUE;
}


BOOLEAN
NTAPI
KdpLowRestoreBreakpoint(IN ULONG BpIndex)
{
	NTSTATUS Status;

	/* Were we not able to remove it earlier? */
	if (KdpBreakpointTable[BpIndex].Flags & KD_BREAKPOINT_EXPIRED)
	{
		/* Just re-use it! */
		KdpBreakpointTable[BpIndex].Flags &= ~KD_BREAKPOINT_EXPIRED;
		return TRUE;
	}

	/* Are we merely writing a breakpoint on top of another breakpoint? */
	if (KdpBreakpointTable[BpIndex].Content == KdpBreakpointInstruction)
	{
		/* Nothing to do */
		return TRUE;
	}

	/* Ok, we actually have to overwrite the instruction now */
	Status = KdpCopyMemoryChunks((ULONG_PTR)KdpBreakpointTable[BpIndex].Address,
		&KdpBreakpointInstruction,
		KD_BREAKPOINT_SIZE,
		0,
		MMDBG_COPY_UNSAFE | MMDBG_COPY_WRITE,
		NULL);
	if (!NT_SUCCESS(Status))
	{
		/* Memory is inaccessible now, restoring breakpoint is deferred */
		// KdpDprintf(L"Failed to restore breakpoint at address 0x%p\n",
		//            KdpBreakpointTable[BpIndex].Address);
		KdpBreakpointTable[BpIndex].Flags |= KD_BREAKPOINT_PENDING;
		KdpOweBreakpoint = TRUE;
		return FALSE;
	}

	/* Clear any possible previous pending flag and return success */
	KdpBreakpointTable[BpIndex].Flags &= ~KD_BREAKPOINT_PENDING;
	return TRUE;
}

BOOLEAN
NTAPI
KdpDeleteBreakpoint(IN ULONG BpEntry)
{
	ULONG BpIndex = BpEntry - 1;

	/* Check for invalid breakpoint entry */
	if (!BpEntry || (BpEntry > KD_BREAKPOINT_MAX)) return FALSE;

	/* If the specified breakpoint table entry is not valid, then return FALSE. */
	if (!KdpBreakpointTable[BpIndex].Flags) return FALSE;

	/* Check if the breakpoint is suspended */
	if (KdpBreakpointTable[BpIndex].Flags & KD_BREAKPOINT_SUSPENDED)
	{
		/* Check if breakpoint is not being deleted */
		if (!(KdpBreakpointTable[BpIndex].Flags & KD_BREAKPOINT_EXPIRED))
		{
			/* Invalidate it and return success */
			KdpBreakpointTable[BpIndex].Flags = 0;
			return TRUE;
		}
	}

	/* Restore original data, then invalidate it and return success */
	if (KdpLowWriteContent(BpIndex)) KdpBreakpointTable[BpIndex].Flags = 0;
	return TRUE;
}

BOOLEAN
NTAPI
KdpDeleteBreakpointRange(IN PVOID Base,
	IN PVOID Limit)
{
	ULONG BpIndex;
	BOOLEAN DeletedBreakpoints;

	/* Assume no breakpoints will be deleted */
	DeletedBreakpoints = FALSE;

	/* Loop the breakpoint table */
	for (BpIndex = 0; BpIndex < KD_BREAKPOINT_MAX; BpIndex++)
	{
		/* Make sure that the breakpoint is active and matches the range. */
		if ((KdpBreakpointTable[BpIndex].Flags & KD_BREAKPOINT_ACTIVE) &&
			((KdpBreakpointTable[BpIndex].Address >= Base) &&
				(KdpBreakpointTable[BpIndex].Address <= Limit)))
		{
			/* Delete it, and remember if we succeeded at least once */
			if (KdpDeleteBreakpoint(BpIndex + 1)) DeletedBreakpoints = TRUE;
		}
	}

	/* Return whether we deleted anything */
	return DeletedBreakpoints;
}

VOID
NTAPI
KdpRestoreAllBreakpoints(VOID)
{
	ULONG BpIndex;

	/* No more suspended Breakpoints */
	BreakpointsSuspended = FALSE;

	/* Loop the breakpoints */
	for (BpIndex = 0; BpIndex < KD_BREAKPOINT_MAX; BpIndex++)
	{
		/* Check if they are valid, suspended breakpoints */
		if ((KdpBreakpointTable[BpIndex].Flags & KD_BREAKPOINT_ACTIVE) &&
			(KdpBreakpointTable[BpIndex].Flags & KD_BREAKPOINT_SUSPENDED))
		{
			/* Unsuspend them */
			KdpBreakpointTable[BpIndex].Flags &= ~KD_BREAKPOINT_SUSPENDED;
			KdpLowRestoreBreakpoint(BpIndex);
		}
	}
}

VOID
NTAPI
KdpSuspendBreakPoint(IN ULONG BpEntry)
{
	ULONG BpIndex = BpEntry - 1;

	/* Check if this is a valid, unsuspended breakpoint */
	if ((KdpBreakpointTable[BpIndex].Flags & KD_BREAKPOINT_ACTIVE) &&
		!(KdpBreakpointTable[BpIndex].Flags & KD_BREAKPOINT_SUSPENDED))
	{
		/* Suspend it */
		KdpBreakpointTable[BpIndex].Flags |= KD_BREAKPOINT_SUSPENDED;
		KdpLowWriteContent(BpIndex);
	}
}

VOID
NTAPI
KdpSuspendAllBreakPoints(VOID)
{
	ULONG BpEntry;

	/* Breakpoints are suspended */
	BreakpointsSuspended = TRUE;

	/* Loop every breakpoint */
	for (BpEntry = 1; BpEntry <= KD_BREAKPOINT_MAX; BpEntry++)
	{
		/* Suspend it */
		KdpSuspendBreakPoint(BpEntry);
	}
}



int gintif = 0;
int gintstep = 0;

/**
  Execute Stepping command.

  @param[in] CpuContext        Pointer to saved CPU context.

**/
VOID
CommandStepping(
	IN DEBUG_CPU_CONTEXT* CpuContext
)
{
	IA32_EFLAGS32* Eflags;

	Eflags = (IA32_EFLAGS32*)&CpuContext->Eflags;
	Eflags->Bits.TF = 1;
	Eflags->Bits.RF = 1;
	//
	// Save and clear EFLAGS.IF to avoid interrupt happen when executing Stepping
	//
	gintif = Eflags->Bits.IF;
	Eflags->Bits.IF = 0;
	//
	// Set Stepping Flag
	//
	gintstep = 1;


	return;
}

/**
  Do some cleanup after Stepping command done.

  @param[in] CpuContext        Pointer to saved CPU context.

**/
VOID
CommandSteppingCleanup(
	IN DEBUG_CPU_CONTEXT* CpuContext
)
{
	IA32_EFLAGS32* Eflags;

	Eflags = (IA32_EFLAGS32*)&CpuContext->Eflags;
	Eflags->Bits.TF = 0;
	Eflags->Bits.RF = 0;
	//
	// Restore EFLAGS.IF
	//
	Eflags->Bits.IF = gintif;
	//Eflags->Bits.IF = 0;
	//
	// Clear Stepping flag
	//
	gintstep = 0;
	return;
}

static void HVGetApicBase()
{
	MSR_IA32_APIC_BASE_REGISTER ApicBaseMsr = { 0 };
	ApicBaseMsr.Uint64 = __readmsr(X86_MSR_IA32_APIC_BASE);

	gapicpage |= ApicBaseMsr.Bits.ApicBase << VSM_PAGE_SHIFT;
	gapicpage |= ApicBaseMsr.Bits.ApicBaseHi << 32;
	return;
}


static BOOLEAN is_xapic_enabled()
{
	UINT64 apicbase = __readmsr(X86_MSR_IA32_APIC_BASE);
	BOOLEAN xapic_enabled = (apicbase & (APIC_EN)) == (APIC_EN);
	if (xapic_enabled && gapicpage == 0)
	{
		HVGetApicBase();
	}
	return xapic_enabled;
}

static BOOLEAN is_x2apic_enabled()
{
	UINT64 apicbase = __readmsr(X86_MSR_IA32_APIC_BASE);
	BOOLEAN x2apic_enabled = (apicbase & (APIC_EN | APIC_EXTD)) == (APIC_EN | APIC_EXTD);
	if (!x2apic_enabled)
	{
		apicbase = (apicbase | (APIC_EN | APIC_EXTD));
	}
	else
	{
		return x2apic_enabled;
	}
	__writemsr(X86_MSR_IA32_APIC_BASE, apicbase);

	apicbase = __readmsr(X86_MSR_IA32_APIC_BASE);
	return (apicbase & (APIC_EN | APIC_EXTD)) == (APIC_EN | APIC_EXTD);
}

UINT32
EFIAPI
ReadLocalApicReg(
	IN UINTN  MmioOffset
)
{
	UINT32 ret = 0;
	if (is_xapic_enabled())
	{
		ret = *(UINT32*)(gapicpage + MmioOffset);
	}
	else if (is_x2apic_enabled())
	{


		UINT32	MsrIndex = (UINT32)(MmioOffset >> 4) + X2APIC_MSR_BASE_ADDRESS;
		ret = (UINT32)__readmsr(MsrIndex);


	}

	return ret;

}

VOID
EFIAPI
WriteLocalApicReg(
	IN UINTN   MmioOffset,
	IN UINT32  Value
)
{

	if (is_xapic_enabled())
	{
		*(UINT32*)(gapicpage + MmioOffset) = Value;
	}
	else if (is_x2apic_enabled())
	{
		UINT32	MsrIndex = (UINT32)(MmioOffset >> 4) + X2APIC_MSR_BASE_ADDRESS;
		__writemsr(MsrIndex, Value);
	}

	return;
}

VOID
EFIAPI
SendApicEoi(
	VOID
)
{

	if (is_xapic_enabled())
	{
		WriteLocalApicReg(XAPIC_EOI_OFFSET, 0);
	}
	else if (is_x2apic_enabled())
	{
		__writemsr(HvSyntheticMsrEom, 0);

	}
	return;
}


VOID
NTAPI
UefiCtx2WindbgCtxImpl(PDEBUG_CPU_CONTEXT pUefiCtx, PCONTEXT pWindbgCtx, BOOLEAN reverse)
{

	ctxchgsame(pWindbgCtx, pUefiCtx, Rax, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Rbx, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Rcx, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Rdx, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Rsi, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Rdi, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Rbp, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Rsp, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, R8, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, R9, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, R10, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, R11, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, R12, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, R13, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, R14, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, R15, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Dr0, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Dr1, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Dr2, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Dr3, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Dr6, reverse);
	ctxchgsame(pWindbgCtx, pUefiCtx, Dr7, reverse);
	if (reverse == FALSE)
	{
		ctxchg(pWindbgCtx, pUefiCtx, Rip, Eip, reverse);
	}

	if (reverse == FALSE)
	{
		pWindbgCtx->EFlags = ((ULONG)pUefiCtx->Eflags);

	}
	else
	{
		//pUefiCtx->Eflags = pWindbgCtx->EFlags;
	}
	ctxchg64to16(pWindbgCtx, pUefiCtx, SegGs, Gs, reverse);
	ctxchg64to16(pWindbgCtx, pUefiCtx, SegFs, Fs, reverse);
	ctxchg64to16(pWindbgCtx, pUefiCtx, SegEs, Es, reverse);
	ctxchg64to16(pWindbgCtx, pUefiCtx, SegDs, Ds, reverse);
	ctxchg64to16(pWindbgCtx, pUefiCtx, SegCs, Cs, reverse);
	ctxchg64to16(pWindbgCtx, pUefiCtx, SegSs, Ss, reverse);

	return;

}
VOID
NTAPI
UefiCtx2WindbgSpecialRegistersCtxImpl(PDEBUG_CPU_CONTEXT pUefiCtx, PKSPECIAL_REGISTERS pWindbgSpecialRegistersCtx, BOOLEAN reverse)
{
	ctxchgsame(pWindbgSpecialRegistersCtx, pUefiCtx, Cr0, reverse);
	ctxchgsame(pWindbgSpecialRegistersCtx, pUefiCtx, Cr2, reverse);
	ctxchgsame(pWindbgSpecialRegistersCtx, pUefiCtx, Cr3, reverse);
	ctxchgsame(pWindbgSpecialRegistersCtx, pUefiCtx, Cr4, reverse);
	ctxchgsame(pWindbgSpecialRegistersCtx, pUefiCtx, Cr8, reverse);
	ctxchg(pWindbgSpecialRegistersCtx, pUefiCtx, KernelDr0, Dr0, reverse);
	ctxchg(pWindbgSpecialRegistersCtx, pUefiCtx, KernelDr1, Dr1, reverse);
	ctxchg(pWindbgSpecialRegistersCtx, pUefiCtx, KernelDr2, Dr2, reverse);
	ctxchg(pWindbgSpecialRegistersCtx, pUefiCtx, KernelDr3, Dr3, reverse);
	ctxchg(pWindbgSpecialRegistersCtx, pUefiCtx, KernelDr6, Dr6, reverse);
	ctxchg(pWindbgSpecialRegistersCtx, pUefiCtx, KernelDr7, Dr7, reverse);
	if (reverse == FALSE)
	{
		//KdpMoveMemory(&pWindbgSpecialRegistersCtx->Gdtr, pUefiCtx->Gdtr, 0x10);


		_sgdt((IA32_DESCRIPTOR*)&pWindbgSpecialRegistersCtx->Gdtr.Limit);
	}
	else
	{
		KdpMoveMemory(pUefiCtx->Gdtr, &pWindbgSpecialRegistersCtx->Gdtr, 0x10);
	}
	if (reverse == FALSE)
	{
		//KdpMoveMemory(&pWindbgSpecialRegistersCtx->Idtr, pUefiCtx->Idtr, 0x10);
		__sidt((IA32_DESCRIPTOR*)&pWindbgSpecialRegistersCtx->Idtr.Limit);
	}
	else
	{
		KdpMoveMemory(pUefiCtx->Idtr, &pWindbgSpecialRegistersCtx->Idtr, 0x10);
	}
	ctxchg64to16(pWindbgSpecialRegistersCtx, pUefiCtx, Tr, Tr, reverse);
	return;
}

VOID
NTAPI
UefiCtx2WindbgCtx(PDEBUG_CPU_CONTEXT pUefiCtx, PCONTEXT pWindbgCtx)
{
	UefiCtx2WindbgCtxImpl(pUefiCtx, pWindbgCtx, FALSE);
	return;
}
VOID
NTAPI
UefiCtx2WindbgSpecialRegistersCtx(PDEBUG_CPU_CONTEXT pUefiCtx, PKSPECIAL_REGISTERS pWindbgSpecialRegistersCtx)
{
	UefiCtx2WindbgSpecialRegistersCtxImpl(pUefiCtx, pWindbgSpecialRegistersCtx, FALSE);
	return;
}
VOID
NTAPI
WindbgSpecialRegistersCtx2UefiCtx(PKSPECIAL_REGISTERS pWindbgSpecialRegistersCtx, PDEBUG_CPU_CONTEXT pUefiCtx)
{
	UefiCtx2WindbgSpecialRegistersCtxImpl(pUefiCtx, pWindbgSpecialRegistersCtx, TRUE);
	return;
}
VOID
NTAPI
WindbgCtx2UefiCtx(PCONTEXT pWindbgCtx, PDEBUG_CPU_CONTEXT pUefiCtx)
{
	UefiCtx2WindbgCtxImpl(pUefiCtx, pWindbgCtx, TRUE);
	return;
}

//1秒
void stall(int multi)
{
	int basecount = 100000 * multi;
	gBS->Stall(basecount);
	return;
}

/**
  Copies a source GUID to a destination GUID.

  This function copies the contents of the 128-bit GUID specified by SourceGuid to
  DestinationGuid, and returns DestinationGuid.

  If DestinationGuid is NULL, then ASSERT().
  If SourceGuid is NULL, then ASSERT().

  @param  DestinationGuid   The pointer to the destination GUID.
  @param  SourceGuid        The pointer to the source GUID.

  @return DestinationGuid.

**/
GUID*
EFIAPI
CopyGuid(
	OUT GUID* DestinationGuid,
	IN CONST GUID* SourceGuid
)
{
	WriteUnaligned64(
		(UINT64*)DestinationGuid,
		ReadUnaligned64((CONST UINT64*)SourceGuid)
	);
	WriteUnaligned64(
		(UINT64*)DestinationGuid + 1,
		ReadUnaligned64((CONST UINT64*)SourceGuid + 1)
	);
	return DestinationGuid;
}


/**
  Compares two GUIDs.

  This function compares Guid1 to Guid2.  If the GUIDs are identical then TRUE is returned.
  If there are any bit differences in the two GUIDs, then FALSE is returned.

  If Guid1 is NULL, then ASSERT().
  If Guid2 is NULL, then ASSERT().

  @param  Guid1       A pointer to a 128 bit GUID.
  @param  Guid2       A pointer to a 128 bit GUID.

  @retval TRUE        Guid1 and Guid2 are identical.
  @retval FALSE       Guid1 and Guid2 are not identical.

**/
BOOLEAN
EFIAPI
CompareGuid(
	IN CONST GUID* Guid1,
	IN CONST GUID* Guid2
)
{
	UINT64  LowPartOfGuid1;
	UINT64  LowPartOfGuid2;
	UINT64  HighPartOfGuid1;
	UINT64  HighPartOfGuid2;

	LowPartOfGuid1 = ReadUnaligned64((CONST UINT64*)Guid1);
	LowPartOfGuid2 = ReadUnaligned64((CONST UINT64*)Guid2);
	HighPartOfGuid1 = ReadUnaligned64((CONST UINT64*)Guid1 + 1);
	HighPartOfGuid2 = ReadUnaligned64((CONST UINT64*)Guid2 + 1);

	return (BOOLEAN)(LowPartOfGuid1 == LowPartOfGuid2 && HighPartOfGuid1 == HighPartOfGuid2);
}
/**
  Allocates one or more 4KB pages of a certain memory type at a specified alignment.

  Allocates the number of 4KB pages specified by Pages of a certain memory type with an alignment
  specified by Alignment.  The allocated buffer is returned.  If Pages is 0, then NULL is returned.
  If there is not enough memory at the specified alignment remaining to satisfy the request, then
  NULL is returned.
  If Alignment is not a power of two and Alignment is not zero, then ASSERT().
  If Pages plus EFI_SIZE_TO_PAGES (Alignment) overflows, then ASSERT().

  @param  MemoryType            The type of memory to allocate.
  @param  Pages                 The number of 4 KB pages to allocate.
  @param  Alignment             The requested alignment of the allocation.  Must be a power of two.
								If Alignment is zero, then byte alignment is used.

  @return A pointer to the allocated buffer or NULL if allocation fails.

**/
VOID*
InternalAllocateAlignedPages(
	IN EFI_MEMORY_TYPE  MemoryType,
	IN UINTN            Pages,
	IN UINTN            Alignment
)
{
	EFI_STATUS            Status;
	EFI_PHYSICAL_ADDRESS  Memory;
	UINTN                 AlignedMemory;
	UINTN                 AlignmentMask;
	UINTN                 UnalignedPages;
	UINTN                 RealPages;

	//
	// Alignment must be a power of two or zero.
	//
	ASSERT((Alignment & (Alignment - 1)) == 0);

	if (Pages == 0) {
		return NULL;
	}

	if (Alignment > EFI_PAGE_SIZE) {
		//
		// Calculate the total number of pages since alignment is larger than page size.
		//
		AlignmentMask = Alignment - 1;
		RealPages = Pages + EFI_SIZE_TO_PAGES(Alignment);
		//
		// Make sure that Pages plus EFI_SIZE_TO_PAGES (Alignment) does not overflow.
		//
		ASSERT(RealPages > Pages);

		Status = gBS->AllocatePages(AllocateAnyPages, MemoryType, RealPages, &Memory);
		if (EFI_ERROR(Status)) {
			return NULL;
		}

		AlignedMemory = ((UINTN)Memory + AlignmentMask) & ~AlignmentMask;
		UnalignedPages = EFI_SIZE_TO_PAGES(AlignedMemory - (UINTN)Memory);
		if (UnalignedPages > 0) {
			//
			// Free first unaligned page(s).
			//
			Status = gBS->FreePages(Memory, UnalignedPages);
			ASSERT_EFI_ERROR(Status);
		}

		Memory = AlignedMemory + EFI_PAGES_TO_SIZE(Pages);
		UnalignedPages = RealPages - Pages - UnalignedPages;
		if (UnalignedPages > 0) {
			//
			// Free last unaligned page(s).
			//
			Status = gBS->FreePages(Memory, UnalignedPages);
			ASSERT_EFI_ERROR(Status);
		}
	}
	else {
		//
		// Do not over-allocate pages in this case.
		//
		Status = gBS->AllocatePages(AllocateAnyPages, MemoryType, Pages, &Memory);
		if (EFI_ERROR(Status)) {
			return NULL;
		}

		AlignedMemory = (UINTN)Memory;
	}

	return (VOID*)AlignedMemory;
}

/**
  Allocates a buffer of a certain pool type.

  Allocates the number bytes specified by AllocationSize of a certain pool type and returns a
  pointer to the allocated buffer.  If AllocationSize is 0, then a valid buffer of 0 size is
  returned.  If there is not enough memory remaining to satisfy the request, then NULL is returned.

  @param  MemoryType            The type of memory to allocate.
  @param  AllocationSize        The number of bytes to allocate.

  @return A pointer to the allocated buffer or NULL if allocation fails.

**/
VOID*
InternalAllocatePool(
	IN EFI_MEMORY_TYPE  MemoryType,
	IN UINTN            AllocationSize
)
{
	EFI_STATUS  Status;
	VOID* Memory;

	Status = gBS->AllocatePool(MemoryType, AllocationSize, &Memory);
	if (EFI_ERROR(Status)) {
		Memory = NULL;
	}

	return Memory;
}

/**
  Allocates one or more 4KB pages of type EfiBootServicesData at a specified alignment.

  Allocates the number of 4KB pages specified by Pages of type EfiBootServicesData with an
  alignment specified by Alignment.  The allocated buffer is returned.  If Pages is 0, then NULL is
  returned.  If there is not enough memory at the specified alignment remaining to satisfy the
  request, then NULL is returned.

  If Alignment is not a power of two and Alignment is not zero, then ASSERT().
  If Pages plus EFI_SIZE_TO_PAGES (Alignment) overflows, then ASSERT().

  @param  Pages                 The number of 4 KB pages to allocate.
  @param  Alignment             The requested alignment of the allocation.
								Must be a power of two.
								If Alignment is zero, then byte alignment is used.

  @return A pointer to the allocated buffer or NULL if allocation fails.

**/
VOID*
EFIAPI
AllocateAlignedPages(
	IN UINTN  Pages,
	IN UINTN  Alignment
)
{
	return InternalAllocateAlignedPages(EfiBootServicesData, Pages, Alignment);
}

/**
  Allocates and zeros a buffer of a certain pool type.

  Allocates the number bytes specified by AllocationSize of a certain pool type, clears the buffer
  with zeros, and returns a pointer to the allocated buffer.  If AllocationSize is 0, then a valid
  buffer of 0 size is returned.  If there is not enough memory remaining to satisfy the request,
  then NULL is returned.

  @param  PoolType              The type of memory to allocate.
  @param  AllocationSize        The number of bytes to allocate and zero.

  @return A pointer to the allocated buffer or NULL if allocation fails.

**/
VOID*
InternalAllocateZeroPool(
	IN EFI_MEMORY_TYPE  PoolType,
	IN UINTN            AllocationSize
)
{
	VOID* Memory;

	Memory = InternalAllocatePool(PoolType, AllocationSize);
	if (Memory != NULL) {
		Memory = ZeroMem(Memory, AllocationSize);
	}

	return Memory;
}


/**
  Allocates and zeros a buffer of type EfiRuntimeServicesData.

  Allocates the number bytes specified by AllocationSize of type EfiRuntimeServicesData,
  clears the buffer with zeros, and returns a pointer to the allocated buffer.
  If AllocationSize is 0, then a valid buffer of 0 size is returned.  If there is
  not enough memory remaining to satisfy the request, then NULL is returned.

  @param  AllocationSize        The number of bytes to allocate and zero.

  @return A pointer to the allocated buffer or NULL if allocation fails.

**/
VOID*
EFIAPI
AllocateZeroPool(
	IN UINTN  AllocationSize
)
{
	VOID* Buffer;

	Buffer = InternalAllocateZeroPool(EfiRuntimeServicesData, AllocationSize);

	return Buffer;
}


KDP_STATUS
NTAPI
CopyRingBuferrMemoryInputAvail(
	OUT UINT8* Buffer,
	IN  UINT32   NumberOfBytes)
{
	return 0;
}
KDP_STATUS
NTAPI
CopyRingBuferrMemoryInput(
	OUT UINT8* Buffer,
	IN  UINT32   NumberOfBytes, IN  UINT32   WaiteSeq)
{
	return 0;
}
void DumpRspFunction(UINT64 rspval)
{
	for (UINT64 rspnow = rspval; rspnow > rspval - 0x1000; rspnow -= 8)
	{
		UINT64 funcaddr = *(UINT64*)rspnow;
		if (funcaddr > (UINT64)mSyntheticSymbolInfo[0].SymbolInfo.BaseOfDll && funcaddr < (UINT64)mSyntheticSymbolInfo[0].SymbolInfo.BaseOfDll + mSyntheticSymbolInfo[0].SymbolInfo.SizeOfImage)
		{
			Print(L"%p\r\n", funcaddr);
		}
	}
	Print(L"BaseOfDll:=> %p\r\n", mSyntheticSymbolInfo[0].SymbolInfo.BaseOfDll);
	return;

}


/**
  Get current break cause.

  @param[in] Vector      Vector value of exception or interrupt.
  @param[in] CpuContext  Pointer to save CPU context.

  @return The type of break cause defined by XXXX

**/
UINT8
GetBreakCause(
	IN UINTN              Vector,
	IN DEBUG_CPU_CONTEXT* CpuContext
)
{
	UINT8  Cause;

	Cause = DEBUG_DATA_BREAK_CAUSE_UNKNOWN;

	switch (Vector) {
	case DEBUG_INT1_VECTOR:
	case DEBUG_INT3_VECTOR:

		if (Vector == DEBUG_INT1_VECTOR) {
			//
			// INT 1
			//
			if ((CpuContext->Dr6 & BIT14) != 0) {
				Cause = DEBUG_DATA_BREAK_CAUSE_STEPPING;
				//
				// DR6.BIT14 Indicates (when set) that the debug exception was
				// triggered by the single step execution mode.
				// The single-step mode is the highest priority debug exception.
				// This is single step, no need to check DR0, to ensure single step
				// work in PeCoffExtraActionLib (right after triggering a breakpoint
				// to report image load/unload).
				//
				return Cause;
			}
			else {
				Cause = DEBUG_DATA_BREAK_CAUSE_HW_BREAKPOINT;
			}
		}
		else {
			//
			// INT 3
			//
			Cause = DEBUG_DATA_BREAK_CAUSE_SW_BREAKPOINT;
		}

		switch (CpuContext->Dr0) {
		case IMAGE_LOAD_SIGNATURE:
		case IMAGE_UNLOAD_SIGNATURE:
		{

			//if (CpuContext->Dr3 == IO_PORT_BREAKPOINT_ADDRESS) {
			Cause = (UINT8)((CpuContext->Dr0 == IMAGE_LOAD_SIGNATURE) ?
				DEBUG_DATA_BREAK_CAUSE_IMAGE_LOAD : DEBUG_DATA_BREAK_CAUSE_IMAGE_UNLOAD);
			//}

			break;
		}
		case SOFT_INTERRUPT_SIGNATURE:

			if (CpuContext->Dr1 == MEMORY_READY_SIGNATURE) {
				Cause = DEBUG_DATA_BREAK_CAUSE_MEMORY_READY;
				CpuContext->Dr0 = 0;
			}
			else if (CpuContext->Dr1 == SYSTEM_RESET_SIGNATURE) {
				Cause = DEBUG_DATA_BREAK_CAUSE_SYSTEM_RESET;
				CpuContext->Dr0 = 0;
			}

			break;

		default:
			break;
		}

		break;

	case DEBUG_TIMER_VECTOR:
		Cause = DEBUG_DATA_BREAK_CAUSE_USER_HALT;
		break;

	default:
		if (Vector < 20) {

			Cause = DEBUG_DATA_BREAK_CAUSE_EXCEPTION;

		}

		break;
	}

	return Cause;
}




void
EFIAPI  WindbgTearDown()
{
	KdpDprintf(L"WindbgTearDown \r\n");
	return;
}


void
EFIAPI  InterruptProcessExit()
{
	KdpDprintf(L"InterruptProcessExit \r\n");
	return;
}

void
EFIAPI  DumpHexFunction()
{
	KdpDprintf(L"DumpHexFunction \r\n");
	return;
}





UINT64 GetReadyToRunHeader()
{
	return __Module;
}


VOID EFIAPI  DumpPdb(UINT64 modbase)
{
	UINT32 CheckSum = 0;
	UINT32 SizeOfImage = 0;
	if (modbase == NULL)
	{
		KdpDprintf(L"GetModuleName modbase invalid\r\n");

		return;
	}

	KdpDprintf(L"GetModuleName modbase %p\r\n", modbase);
	WCHAR* pdbpath = GetModuleName((UINT8*)modbase, &CheckSum, &SizeOfImage);
	if (pdbpath)
	{
		KdpDprintf(L"%s %08x %08x\r\n", pdbpath, CheckSum, SizeOfImage);
	}
	else
	{
		KdpDprintf(L"GetModuleName Not Found\r\n");
	}

	return;
}

BOOLEAN
NTAPI
KdpReport(IN PEXCEPTION_RECORD ExceptionRecord,	IN PCONTEXT ContextRecord, IN PKSPECIAL_REGISTERS pWindbgSpecialRegistersCtx, IN BOOLEAN SecondChanceException);

VOID
CommandCommunication(
	IN     UINTN              Vector,
	IN OUT DEBUG_CPU_CONTEXT* CpuContext,
	UINT8 BreakCause,
	IN     BOOLEAN            BreakReceived
)
{
	CONTEXT WindbgCtx = { 0 };
	KEXCEPTION_FRAME ExceptionFrameObj = { 0 };
	EXCEPTION_RECORD ExceptionRecordObj = { 0 };
	KSPECIAL_REGISTERS WindbgSpecialRegistersCtx = { 0 };
	PEXCEPTION_RECORD ExceptionRecord = &ExceptionRecordObj;
	PKEXCEPTION_FRAME ExceptionFrame = &ExceptionFrameObj;
	PCONTEXT ContextRecord = &WindbgCtx;
	KTRAP_FRAME TrapFrameObj = { 0 };
	PKTRAP_FRAME TrapFrame = &TrapFrameObj;
	UefiCtx2WindbgCtx(CpuContext, &WindbgCtx);
	UefiCtx2WindbgSpecialRegistersCtx(CpuContext, &WindbgSpecialRegistersCtx);
	KPROCESSOR_MODE PreviousMode = KernelMode;
	BOOLEAN SecondChanceException = FALSE;
	//ULONG_PTR ExceptionCommand;

	//RETURN_STATUS                      Status;
	//UINT8                              InputPacketBuffer[DEBUG_DATA_UPPER_LIMIT + sizeof(UINT64) - 1];
	//DEBUG_PACKET_HEADER* DebugHeader;
	//UINT8                              Width;
	/*UINT8                              Data8;
	UINT32                             Data32;
	UINT64                             Data64;*/
	/*DEBUG_DATA_READ_MEMORY* MemoryRead;
	DEBUG_DATA_WRITE_MEMORY* MemoryWrite;*/
	/*DEBUG_DATA_READ_IO* IoRead;
	DEBUG_DATA_WRITE_IO* IoWrite;
	DEBUG_DATA_READ_REGISTER* RegisterRead;
	DEBUG_DATA_WRITE_REGISTER* RegisterWrite;*/
	//UINT8* RegisterBuffer;
	/*DEBUG_DATA_READ_MSR* MsrRegisterRead;
	DEBUG_DATA_WRITE_MSR* MsrRegisterWrite;*/
	//DEBUG_DATA_CPUID* Cpuid;
	//UINT8    BreakCause;
	/*DEBUG_DATA_RESPONSE_CPUID          CpuidResponse;
	DEBUG_DATA_SEARCH_SIGNATURE* SearchSignature;
	DEBUG_DATA_RESPONSE_GET_EXCEPTION  Exception;*/
	//DEBUG_DATA_RESPONSE_GET_REVISION   DebugAgentRevision;
	//DEBUG_DATA_SET_VIEWPOINT* SetViewPoint;
	BOOLEAN                            HaltDeferred;
	UINT32                             ProcessorIndex;
	//DEBUG_AGENT_EXCEPTION_BUFFER       AgentExceptionBuffer;
	UINT32                             IssuedViewPoint;
	//DEBUG_AGENT_MAILBOX* Mailbox;
	//UINT8* AlignedDataPtr;

	ProcessorIndex = 0;
	IssuedViewPoint = 0;
	HaltDeferred = BreakReceived;
	ExceptionRecord->ExceptionCode = 0;

	switch (BreakCause)
	{
		case DEBUG_DATA_BREAK_CAUSE_HW_BREAKPOINT:
		case DEBUG_DATA_BREAK_CAUSE_SW_BREAKPOINT:
		case DEBUG_DATA_BREAK_CAUSE_EXCEPTION:
		{
			//CommandSteppingCleanup(CpuContext);
			ExceptionRecord->ExceptionCode = STATUS_BREAKPOINT;
			ExceptionRecord->ExceptionInformation[0] = BREAKPOINT_BREAK;
			break;
		}
		default:
		{
			return;
			break;
		}
	}
	if ((ExceptionRecord->ExceptionCode == STATUS_BREAKPOINT) &&
		(ExceptionRecord->ExceptionInformation[0]== BREAKPOINT_BREAK))
	{
		KdpReport(ExceptionRecord, ContextRecord, &WindbgSpecialRegistersCtx, FALSE);
	}



	return;
}

UINT64 EFIAPI CallExceptionHandlerIdt(UINT64 rspval);

UINT64 EFIAPI CheckRuntimeFunction(UINT64 fakefun);


void*
EFIAPI
InterruptProcess(
	IN UINT32             Vector,
	IN DEBUG_CPU_CONTEXT* CpuContext
)
{
	UINTN                         SavedEip = 0;
	BOOLEAN                       BreakReceived = FALSE;
	BOOLEAN                       BreakResume = FALSE;
	DEBUG_CPU_CONTEXT* CpuContextSave = CpuContext;


	if (CpuContext->Sig1 != 0x8728367956867960 && CpuContext->Sig0 != 0x5686796087283679)
	{
		KdpDprintf(L"sig %p %p \r\n", CpuContext->Sig0, CpuContext->Sig1);
	}
	UINT8 BreakCause = GetBreakCause(Vector, CpuContext);




	if (Vector != DEBUG_TIMER_VECTOR && Vector != DEBUG_INT3_VECTOR)
	{
		Print(L"InterruptProcess %08x %08x %p %p %p\r\n",  Vector, BreakCause, CpuContext->Eip, CpuContext->Rsp, mSyntheticSymbolInfo[0].SymbolInfo.BaseOfDll);
		UINT64 saversp = CpuContext->Rsp;
		UINT64 saverip = CpuContext->Eip;
		UefiMemoryStackTrace(saverip, saversp);
		UINT64 retrsp=CallExceptionHandlerIdt(CpuContext->Rsp);		
	
		UINT64 retrspsave = retrsp;
		//retrsp += 8;
		int idx = 0;
		while (TRUE)
		{
			UINT64 tmprip = *(UINT64*)retrsp;
			if(tmprip)
			{
				if(CheckRuntimeFunction(tmprip))
				{
					CpuContext->Rsp = retrsp + 8;

					CpuContext->Eip = tmprip;
					BreakResume = TRUE;
					break;
				}
			}
			if(idx>0x100)
			{
				KdpDprintf(L"CallExceptionHandlerIdtMax %p %p %p %p \r\n", gmodbase, CpuContext->Rsp, retrspsave, CpuContext->Eip);
				while (TRUE)
				{
					stall(10);
				}
			}
			idx++;
			retrsp += 8;
		}
		KdpDprintf(L"CallExceptionHandlerIdt %p %p %p %p \r\n", gmodbase, CpuContext->Rsp, retrspsave, CpuContext->Eip);

		/*while (TRUE)
		{
			stall(10);
		}*/
		/*//__fastfail(1);
		DumpRspFunction(CpuContext->Rsp);
		KdpDprintf(L"InterruptProcess %08x %08x %p\r\n", Vector, BreakCause, mSyntheticSymbolInfo[0].SymbolInfo.BaseOfDll);

		return CpuContext;*/
	}
	if (BreakResume == FALSE) {
		switch (Vector) {
		case DEBUG_INT1_VECTOR:
		case DEBUG_INT3_VECTOR:
		case DEBUG_EXCEPT_GP_FAULT: {
			switch (BreakCause) {
			case DEBUG_DATA_BREAK_CAUSE_SYSTEM_RESET:

				/*if (AttachHost(BreakCause, READ_PACKET_TIMEOUT, &BreakReceived) != RETURN_SUCCESS) {
					//
					// Try to connect HOST, return if fails
					//
					break;
				}*/

				CommandCommunication(Vector, CpuContext, BreakCause, BreakReceived);
				break;

			case DEBUG_DATA_BREAK_CAUSE_STEPPING:
				//
				// Stepping is finished, send Ack package.


				//
				// Clear Stepping Flag and restore EFLAGS.IF
				//
				CommandSteppingCleanup(CpuContext);
				//SendAckPacket(DEBUG_COMMAND_OK);
				CommandCommunication(Vector, CpuContext, BreakCause, BreakReceived);
				break;

			case DEBUG_DATA_BREAK_CAUSE_MEMORY_READY:
				//
				// Memory is ready
				//
				//SendCommandAndWaitForAckOK(DEBUG_COMMAND_MEMORY_READY, READ_PACKET_TIMEOUT, &BreakReceived, NULL);
				CommandCommunication(Vector, CpuContext, BreakCause, BreakReceived);
				break;

			case DEBUG_DATA_BREAK_CAUSE_IMAGE_LOAD:
			case DEBUG_DATA_BREAK_CAUSE_IMAGE_UNLOAD:
				//
				// Set AL to DEBUG_AGENT_IMAGE_CONTINUE
				//
				/*Al = ArchReadRegisterBuffer(CpuContext, SOFT_DEBUGGER_REGISTER_AX, &Data8);
				*Al = DEBUG_AGENT_IMAGE_CONTINUE;
				*/
			{
				//
				// If HOST is not connected for image load/unload, return
				//


				CommandCommunication(Vector, CpuContext, BreakCause, BreakReceived);
				break;
			}

			//
			// Continue to run the following common code
			//

			case DEBUG_DATA_BREAK_CAUSE_HW_BREAKPOINT:
			case DEBUG_DATA_BREAK_CAUSE_SW_BREAKPOINT:
			case DEBUG_DATA_BREAK_CAUSE_EXCEPTION:
			default:
			{
				if (Vector == DEBUG_INT3_VECTOR) {

					//
					// go back address located "0xCC"
					//
					CpuContext->Eip--;
					SavedEip = CpuContext->Eip;
					CommandCommunication(Vector, CpuContext, BreakCause, BreakReceived);
					/*UefiMemoryStackTrace(CpuContext->Eip, CpuContext->Rsp);
					while (TRUE)
					{
						stall(10);
					}*/
					if ((SavedEip == CpuContext->Eip) &&
						(*(UINT8*)(UINTN)CpuContext->Eip == DEBUG_SW_BREAKPOINT_SYMBOL))
					{
						//
						// If this is not a software breakpoint set by HOST,
						// restore EIP
						//
						CpuContext->Eip++;
					}
					else if (CpuContext->Eip != SavedEip)
					{
						KdpDprintf(L"rip %p %p \r\n", CpuContext->Eip, SavedEip);

						CpuContext->Eip = SavedEip;
					}
				}
				else {
					CommandCommunication(Vector, CpuContext, BreakCause, BreakReceived);
				}

				break;
			}
			}
			break;
		}
		case DEBUG_TIMER_VECTOR:

		{
			SendApicEoi();
			if (!VmbusServiceProtocolLoaded)
			{
				//KdpDprintf(L"HvVmbusTimer\r\n");
				HvVmbusTimer();
			}
			break;
		}
		default: {
			if (Vector < 20 && BreakCause == DEBUG_DATA_BREAK_CAUSE_EXCEPTION)
			{
				CommandCommunication(Vector, CpuContext, BreakCause, BreakReceived);
			}
			break;
		}
		}
	}

	if (CpuContextSave != CpuContext)
	{
		KdpDprintf(L"ctx %p %p %p %p %p\r\n", CpuContext, CpuContextSave, SavedEip, CpuContext->Eip, CpuContextSave->Eip);
		KdpDprintf(L"handled rip %p %p \r\n", CpuContext->Eip, SavedEip);

	}


	if (CpuContext->Sig1 != 0x8728367956867960 && CpuContext->Sig0 != 0x5686796087283679)
	{
		KdpDprintf(L"sig %p %p \r\n", CpuContext->Sig0, CpuContext->Sig1);
	}
	return CpuContext;
}


/**
  Initialize the state of the SoftwareEnable bit in the Local APIC
  Spurious Interrupt Vector register.

  @param  Enable  If TRUE, then set SoftwareEnable to 1
				  If FALSE, then set SoftwareEnable to 0.

**/
VOID
EFIAPI
InitializeLocalApicSoftwareEnable(
	IN BOOLEAN  Enable
)
{
	LOCAL_APIC_SVR  Svr;

	//
	// Set local APIC software-enabled bit.
	//
	Svr.Uint32 = ReadLocalApicReg(XAPIC_SPURIOUS_VECTOR_OFFSET);
	if (Enable) {
		if (Svr.Bits.SoftwareEnable == 0) {
			Svr.Bits.SoftwareEnable = 1;
			WriteLocalApicReg(XAPIC_SPURIOUS_VECTOR_OFFSET, Svr.Uint32);
		}
	}
	else {
		if (Svr.Bits.SoftwareEnable == 1) {
			Svr.Bits.SoftwareEnable = 0;
			WriteLocalApicReg(XAPIC_SPURIOUS_VECTOR_OFFSET, Svr.Uint32);
		}
	}
}


/**
  Get the state of the local APIC timer.

  This function will ASSERT if the local APIC is not software enabled.

  @param DivideValue   Return the divide value for the DCR. It is one of 1,2,4,8,16,32,64,128.
  @param PeriodicMode  Return the timer mode. If TRUE, timer mode is peridoic. Othewise, timer mode is one-shot.
  @param Vector        Return the timer interrupt vector number.
**/
VOID
EFIAPI
GetApicTimerState(
	OUT UINTN* DivideValue  OPTIONAL,
	OUT BOOLEAN* PeriodicMode  OPTIONAL,
	OUT UINT8* Vector  OPTIONAL
)
{
	UINT32                Divisor;
	LOCAL_APIC_DCR        Dcr;
	LOCAL_APIC_LVT_TIMER  LvtTimer;

	//
	// Check the APIC Software Enable/Disable bit (bit 8) in Spurious-Interrupt
	// Vector Register.
	// This bit will be 1, if local APIC is software enabled.
	//
	ASSERT((ReadLocalApicReg(XAPIC_SPURIOUS_VECTOR_OFFSET) & BIT8) != 0);

	if (DivideValue != NULL) {
		Dcr.Uint32 = ReadLocalApicReg(XAPIC_TIMER_DIVIDE_CONFIGURATION_OFFSET);
		Divisor = Dcr.Bits.DivideValue1 | (Dcr.Bits.DivideValue2 << 2);
		Divisor = (Divisor + 1) & 0x7;
		*DivideValue = ((UINTN)1) << Divisor;
	}

	if ((PeriodicMode != NULL) || (Vector != NULL)) {
		LvtTimer.Uint32 = ReadLocalApicReg(XAPIC_LVT_TIMER_OFFSET);
		if (PeriodicMode != NULL) {
			if (LvtTimer.Bits.TimerMode == 1) {
				*PeriodicMode = TRUE;
			}
			else {
				*PeriodicMode = FALSE;
			}
		}

		if (Vector != NULL) {
			*Vector = (UINT8)LvtTimer.Bits.Vector;
		}
	}
}

/**
  Enable the local APIC timer interrupt.
**/
VOID
EFIAPI
EnableApicTimerInterrupt(
	VOID
)
{
	LOCAL_APIC_LVT_TIMER  LvtTimer;

	LvtTimer.Uint32 = ReadLocalApicReg(XAPIC_LVT_TIMER_OFFSET);
	LvtTimer.Bits.Mask = 0;
	WriteLocalApicReg(XAPIC_LVT_TIMER_OFFSET, LvtTimer.Uint32);
}

/**
  Disable the local APIC timer interrupt.
**/
VOID
EFIAPI
DisableApicTimerInterrupt(
	VOID
)
{
	LOCAL_APIC_LVT_TIMER  LvtTimer;

	LvtTimer.Uint32 = ReadLocalApicReg(XAPIC_LVT_TIMER_OFFSET);
	LvtTimer.Bits.Mask = 1;
	WriteLocalApicReg(XAPIC_LVT_TIMER_OFFSET, LvtTimer.Uint32);
}

/**
  Get the local APIC timer interrupt state.

  @retval TRUE  The local APIC timer interrupt is enabled.
  @retval FALSE The local APIC timer interrupt is disabled.
**/
BOOLEAN
EFIAPI
GetApicTimerInterruptState(
	VOID
)
{
	LOCAL_APIC_LVT_TIMER  LvtTimer;

	LvtTimer.Uint32 = ReadLocalApicReg(XAPIC_LVT_TIMER_OFFSET);
	return (BOOLEAN)(LvtTimer.Bits.Mask == 0);
}

UINT32
EFIAPI
GetApicTimerCurrentCount(
	VOID
)
{
	return ReadLocalApicReg(XAPIC_TIMER_CURRENT_COUNT_OFFSET);
}


/**
  Initialize the local APIC timer.

  The local APIC timer is initialized and enabled.

  @param DivideValue   The divide value for the DCR. It is one of 1,2,4,8,16,32,64,128.
					   If it is 0, then use the current divide value in the DCR.
  @param InitCount     The initial count value.
  @param PeriodicMode  If TRUE, timer mode is peridoic. Othewise, timer mode is one-shot.
  @param Vector        The timer interrupt vector number.
**/
VOID
EFIAPI
InitializeApicTimer(
	IN UINTN    DivideValue,
	IN UINT32   InitCount,
	IN BOOLEAN  PeriodicMode,
	IN UINT8    Vector
)
{
	LOCAL_APIC_DCR        Dcr;
	LOCAL_APIC_LVT_TIMER  LvtTimer;
	UINT32                Divisor;

	//
	// Ensure local APIC is in software-enabled state.
	//
	InitializeLocalApicSoftwareEnable(TRUE);

	//
	// Program init-count register.
	//
	WriteLocalApicReg(XAPIC_TIMER_INIT_COUNT_OFFSET, InitCount);

	if (DivideValue != 0) {
		/*ASSERT(DivideValue <= 128);
		ASSERT(DivideValue == GetPowerOfTwo32((UINT32)DivideValue));*/
		Divisor = (UINT32)((HighBitSet32((UINT32)DivideValue) - 1) & 0x7);

		Dcr.Uint32 = ReadLocalApicReg(XAPIC_TIMER_DIVIDE_CONFIGURATION_OFFSET);
		Dcr.Bits.DivideValue1 = (Divisor & 0x3);
		Dcr.Bits.DivideValue2 = (Divisor >> 2);
		WriteLocalApicReg(XAPIC_TIMER_DIVIDE_CONFIGURATION_OFFSET, Dcr.Uint32);
	}

	//
	// Enable APIC timer interrupt with specified timer mode.
	//
	LvtTimer.Uint32 = ReadLocalApicReg(XAPIC_LVT_TIMER_OFFSET);
	if (PeriodicMode) {
		LvtTimer.Bits.TimerMode = 1;
	}
	else {
		LvtTimer.Bits.TimerMode = 0;
	}

	LvtTimer.Bits.Mask = 0;
	LvtTimer.Bits.Vector = Vector;
	WriteLocalApicReg(XAPIC_LVT_TIMER_OFFSET, LvtTimer.Uint32);
	return;
}
#define _PCD_VALUE_PcdFSBClock  200000000U

/**
  Initialize CPU local APIC timer.

  @param[out] TimerFrequency  Local APIC timer frequency returned.
  @param[in]  DumpFlag        If TRUE, dump Local APIC timer's parameter.

  @return   32-bit Local APIC timer init count.
**/
UINT32
InitializeDebugTimer(
	OUT UINT32* TimerFrequency,
	IN  BOOLEAN  DumpFlag
)
{
	UINTN   ApicTimerDivisor;
	UINT32  InitialCount;
	UINT32  ApicTimerFrequency;

	InitializeLocalApicSoftwareEnable(TRUE);
	GetApicTimerState(&ApicTimerDivisor, NULL, NULL);
	ApicTimerFrequency = _PCD_VALUE_PcdFSBClock / (UINT32)ApicTimerDivisor;
	//
	// Cpu Local Apic timer interrupt frequency, it is set to 0.1s
	//
	InitialCount = (UINT32)DivU64x32(
		MultU64x64(
			ApicTimerFrequency,
			DEBUG_TIMER_INTERVAL
		),
		1000000u
	);

	InitializeApicTimer(ApicTimerDivisor, InitialCount, TRUE, DEBUG_TIMER_VECTOR);
	//
	// Disable Debug Timer interrupt to avoid it is delivered before Debug Port
	// is initialized
	//
	DisableApicTimerInterrupt();



	if (TimerFrequency != NULL) {
		*TimerFrequency = ApicTimerFrequency;
	}

	return InitialCount;
}

BOOLEAN
EFIAPI
SaveAndSetDebugTimerInterrupt(
	IN BOOLEAN  EnableStatus
)
{
	BOOLEAN  OldDebugTimerInterruptState;

	OldDebugTimerInterruptState = GetApicTimerInterruptState();

	if (OldDebugTimerInterruptState != EnableStatus) {
		if (EnableStatus) {
			EnableApicTimerInterrupt();
		}
		else {
			DisableApicTimerInterrupt();
		}

		//
		// Validate the Debug Timer interrupt state
		// This will make additional delay after Local Apic Timer interrupt state is changed.
		// Thus, CPU could handle the potential pending interrupt of Local Apic timer.
		//
		while (GetApicTimerInterruptState() != EnableStatus) {
			stall(1);
		}
	}

	return OldDebugTimerInterruptState;
}


VOID NTAPI
InitializeDebugIdtWindbg(
	VOID
)
{
	IA32_IDT_GATE_DESCRIPTOR* IdtEntry;
	UINTN                     InterruptHandler;
	IA32_DESCRIPTOR           IdtDescriptor;
	UINTN                     Index;
	UINT16                    CodeSegment;
	UINT32                    RegEdx;

	__sidt(&IdtDescriptor);

	//
	// Use current CS as the segment selector of interrupt gate in IDT
	//
	CodeSegment = AsmReadCs();

	IdtEntry = (IA32_IDT_GATE_DESCRIPTOR*)IdtDescriptor.Base;

	for (Index = 0; Index < 20; Index++) {


		InterruptHandler = (UINTN)&Exception0Handle + Index * ExceptionStubHeaderSize;
		IdtEntry[Index].Bits.OffsetLow = (UINT16)(UINTN)InterruptHandler;
		IdtEntry[Index].Bits.OffsetHigh = (UINT16)((UINTN)InterruptHandler >> 16);
		IdtEntry[Index].Bits.Selector = CodeSegment;
		IdtEntry[Index].Bits.GateType = IA32_IDT_GATE_TYPE_INTERRUPT_32;
	}

	InterruptHandler = (UINTN)&TimerInterruptHandle;
	IdtEntry[DEBUG_TIMER_VECTOR].Bits.OffsetLow = (UINT16)(UINTN)InterruptHandler;
	IdtEntry[DEBUG_TIMER_VECTOR].Bits.OffsetHigh = (UINT16)((UINTN)InterruptHandler >> 16);
	IdtEntry[DEBUG_TIMER_VECTOR].Bits.Selector = CodeSegment;
	IdtEntry[DEBUG_TIMER_VECTOR].Bits.GateType = IA32_IDT_GATE_TYPE_INTERRUPT_32;



	//



	//VmbusSintIdtWindbgEntry = (UINT64)GetExceptionHandlerInIdtEntry(SintVectorRestore);
	//Print(L"SintVector %p %p %p\r\n", SintVectorRestore, HYPERVISOR_CALLBACK_VECTOR, VmbusSintIdtWindbgEntry);

	//*(UINT16*)VmbusSintIdtWindbgEntry = iretqopcode;


		/*IA32_IDT_GATE_DESCRIPTOR* idtEntryArr = &IdtEntry[SintVector];
		DumpIdtWindbgEntry(idtEntryArr);*/

		//todo check
	if (SintVectorModify)
	{

		InterruptHandler = (UINTN)&hdlmsgint;
		IdtEntry[HYPERVISOR_CALLBACK_VECTOR].Bits.OffsetLow = (UINT16)(UINTN)InterruptHandler;
		IdtEntry[HYPERVISOR_CALLBACK_VECTOR].Bits.OffsetHigh = (UINT16)((UINTN)InterruptHandler >> 16);
		IdtEntry[HYPERVISOR_CALLBACK_VECTOR].Bits.Selector = CodeSegment;
		IdtEntry[HYPERVISOR_CALLBACK_VECTOR].Bits.GateType = IA32_IDT_GATE_TYPE_INTERRUPT_32;
	}


	/*
	IdtEntry[HYPERVISOR_CALLBACK_VECTOR].Bits.OffsetLow = (UINT16)(UINTN)InterruptHandler;
	IdtEntry[HYPERVISOR_CALLBACK_VECTOR].Bits.OffsetHigh = (UINT16)((UINTN)InterruptHandler >> 16);
	IdtEntry[HYPERVISOR_CALLBACK_VECTOR].Bits.Selector = CodeSegment;
	IdtEntry[HYPERVISOR_CALLBACK_VECTOR].Bits.GateType = IA32_IDT_GATE_TYPE_INTERRUPT_32;*/

	/*

	Print(L"SintVector %p \r\n", SintVector);
	IA32_IDT_GATE_DESCRIPTOR* idtEntryArr = &IdtEntry[SintVector];
	DumpIdtWindbgEntry(idtEntryArr);

	IdtEntry[SintVector].Bits.OffsetLow = (UINT16)(UINTN)InterruptHandler;
	IdtEntry[SintVector].Bits.OffsetHigh = (UINT16)((UINTN)InterruptHandler >> 16);
	IdtEntry[SintVector].Bits.Selector = CodeSegment;
	IdtEntry[SintVector].Bits.GateType = IA32_IDT_GATE_TYPE_INTERRUPT_32;*/




	/*//
	// If the CPU supports Debug Extensions(CPUID:01 EDX:BIT2), then
	// Set DE flag in CR4 to enable IO breakpoint
	//
	AsmCpuid(1, NULL, NULL, NULL, &RegEdx);
	if ((RegEdx & BIT2) != 0) {
		AsmWriteCr4(AsmReadCr4() | BIT3);
	}*/
	return;
}

void EFIAPI FixGdtrMap()
{

	ULONG            GdtBufferSize;
	IA32_DESCRIPTOR  Gdtr;

	UINT64 GdtBuffer;
	_sgdt((IA32_DESCRIPTOR*)&Gdtr);
	GdtBufferSize = sizeof(IA32_SEGMENT_DESCRIPTOR) - 1 + Gdtr.Limit + 1;
	GdtBuffer = (UINT64)AllocateZeroPool(0x1000);
	KdpMoveMemory((VOID*)GdtBuffer, (VOID*)Gdtr.Base, Gdtr.Limit + 1);
	gsymmap[0].BaseOfAddr = Gdtr.Base;
	gsymmap[0].MapOfAddr = GdtBuffer;
	gsymmap[0].SizeOfAddr = 0x1000;
	if (TRUE)
	{
		KdpDprintf(L"FixGdtrMap BaseOfDll %p MapOfDll %p Length %08x ok\r\n", gsymmap[0].BaseOfAddr, gsymmap[0].MapOfAddr, gsymmap[0].SizeOfAddr);
		dumpbuf((VOID*)Gdtr.Base, (int)(Gdtr.Limit & 0xf0));

	}

	AddCachedMemoryRegion(Gdtr.Base, GdtBuffer, 0x1000);
	return;

}


/**
  Find and report module image info to HOST.

  @param[in] AlignSize      Image aligned size.

**/
VOID
FindAndReportModuleImageInfoWindbg(IN UINT64 Pe32Data, UINT64 modbase, PUEFI_SYMBOLS_INFO pSyntheticSymbolInfo
)
{
	if (Pe32Data != 0) {

		KdVersionBlock.KernBase = modbase;
		KdVersionBlock.DebuggerDataList = modbase;
		KdVersionBlock.PsLoadedModuleList = modbase;


		WCHAR* pdbpath = GetModuleName((UINT8*)Pe32Data, &pSyntheticSymbolInfo->SymbolInfo.CheckSum, &pSyntheticSymbolInfo->SymbolInfo.SizeOfImage);
		if (pdbpath)
		{
			KdpDprintf(L"%s\r\n", pdbpath);
			/*if (ForceConsoleOutput)
			{
				DEBUG((DEBUG_INFO, "%s\r\n", pdbpath));

			}*/
			hvwcscpy(pSyntheticSymbolInfo->SymbolPathBuffer, pdbpath);
		}
		pSyntheticSymbolInfo->SymbolInfo.BaseOfDll = (PVOID)modbase;
		pSyntheticSymbolInfo->SymbolInfo.ProcessId = GetProcessorIndex();



	}
	return;
}


BOOLEAN EFIAPI KdInitSystem(IN EFI_HANDLE   ImageHandle,
	IN EFI_SYSTEM_TABLE* SystemTable, UINT64 modbase)
{



	FixGdtrMap();
	/*pPengdingManipulatePacket = (PKD_PACKETEXTRA)AllocateZeroPool(sizeof(KD_PACKETEXTRA));
	InitializeListHeadUefi(&pPengdingManipulatePacket->List);
	*/
	PUEFI_SYMBOLS_INFO pSyntheticSymbolInfo = &mSyntheticSymbolInfo[0];

	KdpContext.KdpDefaultRetries = 20;
	KdpContext.KdpControlCPending = FALSE;
	KdpContext.KdpControlReturn = FALSE;

	KdpSendControlPacket(PACKET_TYPE_KD_RESET, 0);

	KdpDprintf(L"KdInitSystem\r\n");
	/*

	PEFI_WINDBGPROTOCOL myProtocol = &gWindbgProtocol;
	myProtocol->Revsion = 1;
	myProtocol->OutputMsg = OutputMsgFromClient;

	EFI_STATUS	Status = gBS->InstallProtocolInterface(
		&ImageHandle,
		&gEfiWindbgProtocolGUID,
		EFI_NATIVE_INTERFACE,
		&gWindbgProtocol
	);
	if (ForceConsoleOutput)
	{
		if (!EFI_ERROR(Status))
		{
			KdpDprintf(L"KdInitSystem Success\r\n");

		}
		else {
			KdpDprintf(L"KdInitSystem Failed\r\n");
		}
	}
	*/

	/*FindAndReportModuleImageInfoWindbg(modfile, modbase, pSyntheticSymbolInfo);


	if (ReportSynthetic == 0)
	{

		KdpSymbolReportSynthetic(pSyntheticSymbolInfo);

		//stall(10);
		//CurrentPacketId ^= 1;
		KdpDprintf(L"KdInitSystem Success , Windbg Session Established\r\n");
		VmbusKdInitSystemLoaded = TRUE;
		return TRUE;
	}*/



	return  TRUE;
};


/**
  Worker function to set up Debug Agent environment.

  This function will set up IDT table and initialize the IDT entries and
  initialize CPU LOCAL APIC timer.
  It also tries to connect HOST if Debug Agent was not initialized before.

  @param[in] Mailbox        Pointer to Mailbox.

**/
VOID NTAPI
SetupDebugAgentEnvironmentWindbg(IN EFI_HANDLE        ImageHandle, IN EFI_SYSTEM_TABLE* SystemTable, IN UINT64 modbase)
{
	IA32_DESCRIPTOR  Idtr;
	UINT16           IdtEntryCount;
	UINT64           DebugPortHandle;
	UINT32           DebugTimerFrequency;





	//
	// Initialize the IDT table entries to support source level debug.
	//
	//SintVectorRestore = HvVmbusSintVector();

	HvSYNICVtl0New();
	InitializeDebugIdtWindbg();





	//
	// Initialize Debug Timer hardware and save its initial count and frequency
	//
	InitializeDebugTimer(&DebugTimerFrequency, TRUE);

	//启用timer
	SaveAndSetDebugTimerInterrupt(TRUE);

	EnableInterrupts();

	//__debugbreak();
	// Enable interrupt to receive Debug Timer interrupt
	//
	//return;
	//
	if (gVmbusWindbgProtocol == VmbusChannel)
	{
		HvVmbusServiceDxeInitialize();
	}




	KdInitSystem(ImageHandle, SystemTable, modbase);

	//__debugbreak();

	/*if (reportshellefi)
	{
		if (!mSyntheticSymbolInfo[1].SymbolInfo.BaseOfDll)
		{
			PUEFI_SYMBOLS_INFO pSyntheticSymbolInfo1 = &mSyntheticSymbolInfo[1];
			FindAndReportModuleImageInfoShell(SIZE_4KB, pSyntheticSymbolInfo1);
			KdpSymbolReportSynthetic(pSyntheticSymbolInfo1);
		}
	}*/
	//__debugbreak();

	/*Print(L"stall\r\n");
	while (TRUE)
	{
		stall(10);
	}*/
	return;
}



UINT64 GetUnwindInfo(UINT8* m_pBuffer);


VOID EFIAPI InitializeDebugAgentWindbg(IN EFI_HANDLE        ImageHandle,
	IN EFI_SYSTEM_TABLE* SystemTable,
	IN UINT32                InitFlag,
	UINT64 modbase,
	UINT32 modsize)
{
	gBS = SystemTable->BootServices;
	KdpZeroMemory(mSyntheticSymbolInfo, sizeof(mSyntheticSymbolInfo));
	mSyntheticSymbolInfo[0].SymbolInfo.BaseOfDll = modbase;
	mSyntheticSymbolInfo[0].SymbolInfo.SizeOfImage = modsize;
	gmodbase = modbase;
	InitGlobalHv();
	//SetupDebugAgentEnvironmentWindbg(ImageHandle, SystemTable, modbase,modfile);
	GetUnwindInfo(gmodbase);
	//abort();
	return;
}




UINT64 __cdecl __CxxFrameHandler4()
{
	return 0;
}

	/*EHExceptionRecord* pExcept,         // Information for this exception
	EHRegistrationNode RN,               // Dynamic information for this frame
	CONTEXT* pContext,        // Context info
	DispatcherContext* pDC              // More dynamic info for this frame
	)


}*/