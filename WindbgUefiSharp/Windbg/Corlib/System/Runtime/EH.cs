using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System
{
    internal static class EH
    {
        private enum RhEHClauseKind
        {
            RH_EH_CLAUSE_TYPED,
            RH_EH_CLAUSE_FAULT,
            RH_EH_CLAUSE_FILTER,
            RH_EH_CLAUSE_UNUSED
        }

        [StructLayout(LayoutKind.Explicit, Size = 32)]
        private struct EHEnum
        {
            [FieldOffset(0)]
            private IntPtr _dummy;
        }

        [StructLayout(LayoutKind.Explicit, Size = 1232)]
        private struct OSCONTEXT
        {
        }

        private enum RhEHFrameType
        {
            RH_EH_FIRST_FRAME = 1,
            RH_EH_FIRST_RETHROW_FRAME
        }

        private enum HwExceptionCode : uint
        {
            STATUS_REDHAWK_NULL_REFERENCE = 0u,
            STATUS_REDHAWK_UNMANAGED_HELPER_NULL_REFERENCE = 66u,
            STATUS_REDHAWK_THREAD_ABORT = 67u,
            STATUS_DATATYPE_MISALIGNMENT = 2147483650u,
            STATUS_ACCESS_VIOLATION = 3221225477u,
            STATUS_INTEGER_DIVIDE_BY_ZERO = 3221225620u,
            STATUS_INTEGER_OVERFLOW = 3221225621u
        }

        [StructLayout(LayoutKind.Explicit, Size = 256)]
        public struct PAL_LIMITED_CONTEXT
        {
            [FieldOffset(0)]
            internal IntPtr IP;
        }

        [Flags]
        internal enum ExKind : byte
        {
            None = 0,
            Throw = 1,
            HardwareFault = 2,
            KindMask = 3,
            RethrowFlag = 4,
            SupersededFlag = 8,
            InstructionFaultFlag = 0x10
        }

        [StructLayout(LayoutKind.Explicit)]
        public ref struct ExInfo
        {
            [FieldOffset(0)]
            internal unsafe void* _pPrevExInfo;

            [FieldOffset(8)]
            internal unsafe PAL_LIMITED_CONTEXT* _pExContext;

            [FieldOffset(16)]
            private object _exception;

            [FieldOffset(24)]
            internal ExKind _kind;

            [FieldOffset(25)]
            internal byte _passNumber;

            [FieldOffset(28)]
            internal uint _idxCurClause;

            [FieldOffset(32)]
            internal StackFrameIterator _frameIter;

            [FieldOffset(592)]
            internal volatile UIntPtr _notifyDebuggerSP;

            internal object ThrownException => _exception;

            internal void Init(object exceptionObj, bool instructionFault = false)
            {
                _exception = exceptionObj;
                if (instructionFault)
                {
                    _kind |= ExKind.InstructionFaultFlag;
                }
                _notifyDebuggerSP = UIntPtr.Zero;
            }

            internal void Init(object exceptionObj, ref ExInfo rethrownExInfo)
            {
                _exception = exceptionObj;
                _kind = rethrownExInfo._kind | ExKind.RethrowFlag;
                _notifyDebuggerSP = UIntPtr.Zero;
            }
        }

        private const uint MaxTryRegionIdx = uint.MaxValue;

        internal unsafe static UIntPtr MaxSP => (UIntPtr)(void*)(-1);

        //internal static void FallbackFailFast(RhFailFastReason reason, object unhandledException)

        //internal unsafe static void FailFastViaClasslib(RhFailFastReason reason, object unhandledException, IntPtr classlibAddress)

        internal unsafe static void* PointerAlign(void* ptr, int alignmentInBytes)
        {
            int alignMask = alignmentInBytes - 1;
            return (void*)(((ulong)ptr + (ulong)alignMask) & (ulong)(~alignMask));
        }

        //private unsafe static void OnFirstChanceExceptionViaClassLib(object exception)

        //private unsafe static void OnUnhandledExceptionViaClassLib(object exception)

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //internal unsafe static void UnhandledExceptionFailFastViaClasslib(RhFailFastReason reason, object unhandledException, IntPtr classlibAddress, ref ExInfo exInfo)

        //private unsafe static void AppendExceptionStackFrameViaClasslib(object exception, IntPtr ip, ref bool isFirstRethrowFrame, ref bool isFirstFrame)

        //internal unsafe static Exception GetClasslibException(ExceptionIDs id, IntPtr address)

        //internal unsafe static Exception GetClasslibExceptionFromEEType(ExceptionIDs id, MethodTable* pEEType)

        //[RuntimeExport("RhExceptionHandling_ThrowClasslibOverflowException")]
        //public static void ThrowClasslibOverflowException(IntPtr address)

        //[RuntimeExport("RhExceptionHandling_ThrowClasslibDivideByZeroException")]
        //public static void ThrowClasslibDivideByZeroException(IntPtr address)

        //[RuntimeExport("RhExceptionHandling_FailedAllocation")]
        //public unsafe static void FailedAllocation(EETypePtr pEEType, bool fIsOverflow)
        //RhThrowHwEx


#if EMBEDDEDGC
        [RuntimeImport("*", "RhThrowHwEx")]
        public static extern void RhThrowHwEx(uint exceptionCode, ref ExInfo exInfo);

        [RuntimeImport("*", "RhThrowEx")]
        public static extern void RhThrowEx(object exceptionObj, ref ExInfo exInfo);

        [RuntimeImport("*", "RhRethrow")]
        public static extern void RhRethrow(ref ExInfo activeExInfo, ref ExInfo exInfo);
#else
        [RuntimeExport("RhpThrowHwEx")]
        public unsafe static void RhThrowHwEx(uint exceptionCode, ref ExInfo exInfo)
        {
            intr3();
        }

        //RhThrowEx
        [RuntimeExport("RhpThrowEx")]
        public unsafe static void RhThrowEx(object exceptionObj, ref ExInfo exInfo)
        {
            /*
             if you come in here you can most likely interrupt this and continue after something is serial port (ed)
             */
            intr3();
        }

        //RhRethrow
        [RuntimeExport("RhpRethrow")]
        public static void RhRethrow(ref ExInfo activeExInfo, ref ExInfo exInfo)
        {
            intr3();
        }


       
#endif
        public static void intr3()
        {

        }
        //private unsafe static void DispatchEx([ScopedRef] ref StackFrameIterator frameIter, ref ExInfo exInfo, uint startIdx)

        //[Conditional("DEBUG")]
        //private unsafe static void DebugScanCallFrame(int passNumber, byte* ip, UIntPtr sp)

        //[Conditional("DEBUG")]
        //private static void DebugVerifyHandlingFrame(UIntPtr handlingFrameSP)

        //private static void UpdateStackTrace(object exceptionObj, UIntPtr curFramePtr, IntPtr ip, ref bool isFirstRethrowFrame, ref UIntPtr prevFramePtr, ref bool isFirstFrame)

        //private unsafe static bool FindFirstPassHandler(object exception, uint idxStart, ref StackFrameIterator frameIter, out uint tryRegionIdx, out byte* pHandler)

        //private unsafe static bool ShouldTypedClauseCatchThisException(object exception, MethodTable* pClauseType)

        //private static void InvokeSecondPass(ref ExInfo exInfo, uint idxStart)

        //private unsafe static void InvokeSecondPass(ref ExInfo exInfo, uint idxStart, uint idxLimit)

        //[UnmanagedCallersOnly(EntryPoint = "RhpFailFastForPInvokeExceptionPreemp", CallConvs = new Type[] { typeof(CallConvCdecl) })]
        //public unsafe static void RhpFailFastForPInvokeExceptionPreemp(IntPtr PInvokeCallsiteReturnAddr, void* pExceptionRecord, void* pContextRecord)

        //[RuntimeExport("RhpFailFastForPInvokeExceptionCoop")]
        //public unsafe static void RhpFailFastForPInvokeExceptionCoop(IntPtr classlibBreadcrumb, void* pExceptionRecord, void* pContextRecord)


    }
}
