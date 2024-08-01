#ifndef __NTOSKRNL_INCLUDE_INTERNAL_AMD64_KE_H
#define __NTOSKRNL_INCLUDE_INTERNAL_AMD64_KE_H

#ifdef __cplusplus
extern "C" {
#endif

#define X86_EFLAGS_TF           0x00000100 /* Trap flag */
#define X86_EFLAGS_IF           0x00000200 /* Interrupt Enable flag */
#define X86_EFLAGS_IOPL         0x00003000 /* I/O Privilege Level bits */
#define X86_EFLAGS_NT           0x00004000 /* Nested Task flag */
#define X86_EFLAGS_RF           0x00010000 /* Resume flag */
#define X86_EFLAGS_VM           0x00020000 /* Virtual Mode */
#define X86_EFLAGS_ID           0x00200000 /* CPUID detection flag */

#define X86_CR0_PE              0x00000001 /* enable Protected Mode */
#define X86_CR0_NE              0x00000020 /* enable native FPU error reporting */
#define X86_CR0_TS              0x00000008 /* enable exception on FPU instruction for task switch */
#define X86_CR0_EM              0x00000004 /* enable FPU emulation (disable FPU) */
#define X86_CR0_MP              0x00000002 /* enable FPU monitoring */
#define X86_CR0_WP              0x00010000 /* enable Write Protect (copy on write) */
#define X86_CR0_PG              0x80000000 /* enable Paging */

#define X86_CR4_PAE             0x00000020 /* enable physical address extensions */
#define X86_CR4_PGE             0x00000080 /* enable global pages */
#define X86_CR4_OSFXSR          0x00000200 /* enable FXSAVE/FXRSTOR instructions */
#define X86_CR4_OSXMMEXCPT      0x00000400 /* enable #XF exception */

/* EDX flags */
#define X86_FEATURE_FPU         0x00000001 /* x87 FPU is present */
#define X86_FEATURE_VME         0x00000002 /* Virtual 8086 Extensions are present */
#define X86_FEATURE_DBG         0x00000004 /* Debugging extensions are present */
#define X86_FEATURE_PSE         0x00000008 /* Page Size Extension is present */
#define X86_FEATURE_TSC         0x00000010 /* time stamp counters are present */
#define X86_FEATURE_PAE         0x00000040 /* physical address extension is present */
#define X86_FEATURE_CX8         0x00000100 /* CMPXCHG8B instruction present */
#define X86_FEATURE_SYSCALL     0x00000800 /* SYSCALL/SYSRET support present */
#define X86_FEATURE_MTTR        0x00001000 /* Memory type range registers are present */
#define X86_FEATURE_PGE         0x00002000 /* Page Global Enable */
#define X86_FEATURE_CMOV        0x00008000 /* "Conditional move" instruction supported */
#define X86_FEATURE_PAT         0x00010000 /* Page Attribute Table is supported */
#define X86_FEATURE_DS          0x00200000 /* Debug Store is present */
#define X86_FEATURE_MMX         0x00800000 /* MMX extension present */
#define X86_FEATURE_FXSR        0x01000000 /* FXSAVE/FXRSTOR instructions present */
#define X86_FEATURE_SSE         0x02000000 /* SSE extension present */
#define X86_FEATURE_SSE2        0x04000000 /* SSE2 extension present */
#define X86_FEATURE_HT          0x10000000 /* Hyper-Threading present */

/* ECX flags */
#define X86_FEATURE_SSE3        0x00000001 /* SSE3 is supported */
#define X86_FEATURE_MONITOR     0x00000008 /* SSE3 Monitor instructions supported */
#define X86_FEATURE_VMX         0x00000020 /* Virtual Machine eXtensions are available */
#define X86_FEATURE_SSSE3       0x00000200 /* Supplemental SSE3 are available */
#define X86_FEATURE_FMA3        0x00001000 /* Fused multiple-add supported */
#define X86_FEATURE_CX16        0x00002000 /* CMPXCHG16B instruction are available */
#define X86_FEATURE_PCID        0x00020000 /* Process Context IDentifiers are supported */
#define X86_FEATURE_SSE41       0x00080000 /* SSE 4.1 is supported */
#define X86_FEATURE_SSE42       0x00100000 /* SSE 4.2 is supported */
#define X86_FEATURE_POPCNT      0x00800000 /* POPCNT instruction is available */
#define X86_FEATURE_XSAVE       0x04000000 /* XSAVE family are available */

/* EDX extended flags */
#define X86_FEATURE_NX          0x00100000 /* NX support present */

#define X86_EXT_FEATURE_SSE3    0x00000001 /* SSE3 extension present */
#define X86_EXT_FEATURE_3DNOW   0x40000000 /* 3DNOW! extension present */

#define FRAME_EDITED        0xFFF8

#define X86_MSR_GSBASE          0xC0000101
#define X86_MSR_KERNEL_GSBASE   0xC0000102
#define X86_MSR_EFER            0xC0000080
#define X86_MSR_STAR            0xC0000081
#define X86_MSR_LSTAR           0xC0000082
#define X86_MSR_CSTAR           0xC0000083
#define X86_MSR_SFMASK          0xC0000084

#define EFER_SCE    0x0001
#define EFER_LME    0x0100
#define EFER_LMA    0x0400
#define EFER_NXE    0x0800
#define EFER_SVME   0x1000
#define EFER_FFXSR  0x4000

#define AMD64_TSS 9

#define APIC_EOI_REGISTER 0xFFFFFFFFFFFE00B0ULL


typedef struct _KIDT_INIT
{
    UCHAR InterruptId;
    UCHAR Dpl;
    UCHAR IstIndex;
    PVOID ServiceRoutine;
} KIDT_INIT, *PKIDT_INIT;

#include <pshpack1.h>
typedef struct _KI_INTERRUPT_DISPATCH_ENTRY
{
    UCHAR _Op_nop;
    UCHAR _Op_push;
    UCHAR _Vector;
    UCHAR _Op_jmp;
    ULONG RelativeAddress;
} KI_INTERRUPT_DISPATCH_ENTRY, *PKI_INTERRUPT_DISPATCH_ENTRY;
#include <poppack.h>

extern ULONG KeI386NpxPresent;
extern ULONG KeI386XMMIPresent;
extern ULONG KeI386FxsrPresent;
extern ULONG KeI386CpuType;
extern ULONG KeI386CpuStep;

//
// INT3 is 1 byte long
//
#define KD_BREAKPOINT_TYPE        UCHAR
#define KD_BREAKPOINT_SIZE        sizeof(UCHAR)
#define KD_BREAKPOINT_VALUE       0xCC




//
// One-liners for getting and setting special purpose registers in portable code
//
FORCEINLINE
ULONG_PTR
KeGetContextPc(PCONTEXT Context)
{
    return Context->Rip;
}

FORCEINLINE
VOID
KeSetContextPc(PCONTEXT Context, ULONG_PTR ProgramCounter)
{
    Context->Rip = ProgramCounter;
}

FORCEINLINE
ULONG_PTR
KeGetContextReturnRegister(PCONTEXT Context)
{
    return Context->Rax;
}

FORCEINLINE
VOID
KeSetContextReturnRegister(PCONTEXT Context, ULONG_PTR ReturnValue)
{
    Context->Rax = ReturnValue;
}

FORCEINLINE
ULONG_PTR
KeGetContextStackRegister(PCONTEXT Context)
{
    return Context->Rsp;
}

FORCEINLINE
ULONG_PTR
KeGetContextFrameRegister(PCONTEXT Context)
{
    return Context->Rbp;
}

FORCEINLINE
VOID
KeSetContextFrameRegister(PCONTEXT Context, ULONG_PTR Frame)
{
    Context->Rbp = Frame;
}

FORCEINLINE
ULONG_PTR
KeGetTrapFramePc(PKTRAP_FRAME TrapFrame)
{
    return TrapFrame->Rip;
}

FORCEINLINE
PKTRAP_FRAME
KiGetLinkedTrapFrame(PKTRAP_FRAME TrapFrame)
{
    return (PKTRAP_FRAME)TrapFrame->TrapFrame;
}

FORCEINLINE
ULONG_PTR
KeGetTrapFrameStackRegister(PKTRAP_FRAME TrapFrame)
{
    return TrapFrame->Rsp;
}

FORCEINLINE
ULONG_PTR
KeGetTrapFrameFrameRegister(PKTRAP_FRAME TrapFrame)
{
    return TrapFrame->Rbp;
}

#define KdpGetParameterThree(Context)  ((Context)->R8)
#define KdpGetParameterFour(Context)   ((Context)->R9)

//
// Macro to get trap and exception frame from a thread stack
//
#define KeGetTrapFrame(Thread) \
    (PKTRAP_FRAME)((ULONG_PTR)((Thread)->InitialStack) - \
                   sizeof(KTRAP_FRAME))

//
// Macro to get context switches from the PRCB
// All architectures but x86 have it in the PRCB's KeContextSwitches
//
#define KeGetContextSwitches(Prcb)  \
    (Prcb->KeContextSwitches)

//
// Macro to get the second level cache size field name which differs between
// CISC and RISC architectures, as the former has unified I/D cache
//
#define KiGetSecondLevelDCacheSize() ((PKIPCR)KeGetPcr())->SecondLevelCacheSize

#define KeGetExceptionFrame(Thread) \
    (PKEXCEPTION_FRAME)((ULONG_PTR)KeGetTrapFrame(Thread) - \
                        sizeof(KEXCEPTION_FRAME))

//
// Returns the Interrupt State from a Trap Frame.
// ON = TRUE, OFF = FALSE
//
#define KeGetTrapFrameInterruptState(TrapFrame) \
        BooleanFlagOn((TrapFrame)->EFlags, EFLAGS_INTERRUPT_MASK)

/* Diable interrupts and return whether they were enabled before */
FORCEINLINE
BOOLEAN
KeDisableInterrupts(VOID)
{
    ULONG_PTR Flags;

    /* Get EFLAGS and check if the interrupt bit is set */
    Flags = __readeflags();

    /* Disable interrupts */
    _disable();
    return (Flags & EFLAGS_INTERRUPT_MASK) ? TRUE : FALSE;
}

/* Restore previous interrupt state */
FORCEINLINE
VOID
KeRestoreInterrupts(BOOLEAN WereEnabled)
{
    if (WereEnabled) _enable();
}

//
// Invalidates the TLB entry for a specified address
//
FORCEINLINE
VOID
KeInvalidateTlbEntry(IN PVOID Address)
{
    /* Invalidate the TLB entry for this address */
    __invlpg(Address);
}

FORCEINLINE
VOID
KeFlushProcessTb(VOID)
{
    /* Flush the TLB by resetting CR3 */
    __writecr3(__readcr3());
}

FORCEINLINE
VOID
KeSweepICache(IN PVOID BaseAddress,
              IN SIZE_T FlushSize)
{
    //
    // Always sweep the whole cache
    //
    /*UNREFERENCED_PARAMETER(BaseAddress);
    UNREFERENCED_PARAMETER(FlushSize);*/
    __wbinvd();
}


FORCEINLINE
VOID
KiSendEOI(VOID)
{
    /* Write 0 to the apic EOI register */
    *((volatile ULONG*)APIC_EOI_REGISTER) = 0;
}


struct _KPCR;

#endif




VOID
KiSetTrapContext(
    _Out_ PKTRAP_FRAME TrapFrame,
    _In_ PCONTEXT Context,
    _In_ KPROCESSOR_MODE RequestorMode);


#ifdef __cplusplus
} // extern "C"
#endif

/* EOF */
