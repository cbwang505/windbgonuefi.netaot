using Internal.Runtime;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/* I don't expect this to just *work* but everything's worth a try.. it does from from IlCompiler after all */
namespace System.Runtime
{
    internal static class InternalCalls
    {
        /*[MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpGetClasslibFunctionFromEEType")]
        internal unsafe static extern void* RhpGetClasslibFunctionFromEEType(MethodTable* pEEType, ClassLibFunctionId id);*/
    }
    /*
    internal static class InternalCalls
    {
        /*
        [RuntimeExport("RhCollect")]
        internal static void RhCollect(int generation, InternalGCCollectionMode mode)
        {
            RhpCollect(generation, mode);
        }

        [DllImport("*")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        private static extern void RhpCollect(int generation, InternalGCCollectionMode mode);

        *

        [RuntimeExport("RhGetGcTotalMemory")]
        internal static long RhGetGcTotalMemory()
        {
            return RhpGetGcTotalMemory();
        }

        [DllImport("*")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        private static extern long RhpGetGcTotalMemory();

        //[RuntimeExport("RhStartNoGCRegion")]
        //internal static int RhStartNoGCRegion(long totalSize, bool hasLohSize, long lohSize, bool disallowFullBlockingGC)
        //{
        //    return RhpStartNoGCRegion(totalSize, hasLohSize ? Interop.BOOL.TRUE : Interop.BOOL.FALSE, lohSize, disallowFullBlockingGC ? Interop.BOOL.TRUE : Interop.BOOL.FALSE);
        //}

        [RuntimeExport("RhEndNoGCRegion")]
        internal static int RhEndNoGCRegion()
        {
            return RhpEndNoGCRegion();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpGetNextFinalizableObject")]
        internal static extern object RhpGetNextFinalizableObject();

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //[RuntimeImport("*", "RhpHandleAlloc")]
        //internal static extern IntPtr RhpHandleAlloc(object value, GCHandleType type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhHandleGet")]
        internal static extern object RhHandleGet(IntPtr handle);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhHandleSet")]
        internal static extern IntPtr RhHandleSet(IntPtr handle, object value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpNewFast")]
        internal unsafe static extern object RhpNewFast(MethodTable* pEEType);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpNewFinalizable")]
        internal unsafe static extern object RhpNewFinalizable(MethodTable* pEEType);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpNewArray")]
        internal unsafe static extern object RhpNewArray(MethodTable* pEEType, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpCopyObjectContents")]
        internal static extern void RhpCopyObjectContents(object objDest, object objSrc);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpAssignRef")]
        internal static extern void RhpAssignRef(ref object address, object obj);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpInitMultibyte")]
        internal static extern ref byte RhpInitMultibyte(ref byte dmem, int c, nuint size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "memmove")]
        internal unsafe static extern void* memmove(byte* dmem, byte* smem, nuint size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhBulkMoveWithWriteBarrier")]
        internal static extern void RhBulkMoveWithWriteBarrier(ref byte dmem, ref byte smem, nuint size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpEHEnumInitFromStackFrameIterator")]
        internal unsafe static extern bool RhpEHEnumInitFromStackFrameIterator(ref StackFrameIterator pFrameIter, byte** pMethodStartAddress, void* pEHEnum);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpEHEnumNext")]
        internal unsafe static extern bool RhpEHEnumNext(void* pEHEnum, void* pEHClause);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpGetDispatchCellInfo")]
        internal static extern void RhpGetDispatchCellInfo(IntPtr pCell, out DispatchCellInfo newCellInfo);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpSearchDispatchCellCache")]
        internal unsafe static extern IntPtr RhpSearchDispatchCellCache(IntPtr pCell, MethodTable* pInstanceType);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpUpdateDispatchCellCache")]
        internal unsafe static extern IntPtr RhpUpdateDispatchCellCache(IntPtr pCell, IntPtr pTargetCode, MethodTable* pInstanceType, ref DispatchCellInfo newCellInfo);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpGetClasslibFunctionFromCodeAddress")]
        internal unsafe static extern void* RhpGetClasslibFunctionFromCodeAddress(IntPtr address, ClassLibFunctionId id);

       

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpSfiInit")]
        internal unsafe static extern bool RhpSfiInit(ref StackFrameIterator pThis, void* pStackwalkCtx, bool instructionFault);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpSfiNext")]
        internal unsafe static extern bool RhpSfiNext(ref StackFrameIterator pThis, uint* uExCollideClauseIdx, bool* fUnwoundReversePInvoke);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpCallCatchFunclet")]
        internal unsafe static extern IntPtr RhpCallCatchFunclet(object exceptionObj, byte* pHandlerIP, void* pvRegDisplay, ref EH.ExInfo exInfo);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpCallFinallyFunclet")]
        internal unsafe static extern void RhpCallFinallyFunclet(byte* pHandlerIP, void* pvRegDisplay);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpCallFilterFunclet")]
        internal unsafe static extern bool RhpCallFilterFunclet(object exceptionObj, byte* pFilterIP, void* pvRegDisplay);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpFallbackFailFast")]
        internal static extern void RhpFallbackFailFast();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpSetThreadDoNotTriggerGC")]
        internal static extern void RhpSetThreadDoNotTriggerGC();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [Conditional("DEBUG")]
        [RuntimeImport("*", "RhpValidateExInfoStack")]
        internal static extern void RhpValidateExInfoStack();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpCopyContextFromExInfo")]
        internal unsafe static extern void RhpCopyContextFromExInfo(void* pOSContext, int cbOSContext, EH.PAL_LIMITED_CONTEXT* pPalContext);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpGetNumThunkBlocksPerMapping")]
        internal static extern int RhpGetNumThunkBlocksPerMapping();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpGetNumThunksPerBlock")]
        internal static extern int RhpGetNumThunksPerBlock();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpGetThunkSize")]
        internal static extern int RhpGetThunkSize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpGetThunkDataBlockAddress")]
        internal static extern IntPtr RhpGetThunkDataBlockAddress(IntPtr thunkStubAddress);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpGetThunkStubsBlockAddress")]
        internal static extern IntPtr RhpGetThunkStubsBlockAddress(IntPtr thunkDataAddress);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpGetThunkBlockSize")]
        internal static extern int RhpGetThunkBlockSize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpGetThreadAbortException")]
        internal static extern Exception RhpGetThreadAbortException();

        [DllImport("*")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        internal static extern uint RhpWaitForFinalizerRequest();

        [DllImport("*")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        internal static extern void RhpSignalFinalizationComplete();

        [DllImport("*")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        internal static extern void RhpAcquireCastCacheLock();

        [DllImport("*")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        internal static extern void RhpReleaseCastCacheLock();

        [DllImport("*")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        internal static extern ulong RhpGetTickCount64();

        [DllImport("*")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        internal static extern void RhpAcquireThunkPoolLock();

        [DllImport("*")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        internal static extern void RhpReleaseThunkPoolLock();

        [DllImport("*")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        internal static extern IntPtr RhAllocateThunksMapping();

        //[DllImport("*")]
        //[UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        //internal static extern int RhpStartNoGCRegion(long totalSize, Interop.BOOL hasLohSize, long lohSize, Interop.BOOL disallowFullBlockingGC);

        [DllImport("*")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        internal static extern int RhpEndNoGCRegion();
    }

    */
}
