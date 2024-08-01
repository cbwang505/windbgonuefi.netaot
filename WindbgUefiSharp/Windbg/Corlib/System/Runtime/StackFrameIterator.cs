using System;
using System.Runtime.InteropServices;

namespace System.Runtime
{
    [StructLayout(LayoutKind.Explicit, Size = 560)]
    internal struct StackFrameIterator
    {
        [FieldOffset(16)]
        private UIntPtr _framePointer;

        [FieldOffset(24)]
        private IntPtr _controlPC;

        [FieldOffset(32)]
        private REGDISPLAY _regDisplay;

        [FieldOffset(552)]
        private IntPtr _originalControlPC;

        internal unsafe byte* ControlPC => (byte*)(void*)_controlPC;

        internal unsafe byte* OriginalControlPC => (byte*)(void*)_originalControlPC;

        internal unsafe void* RegisterSet
        {
            get
            {
                fixed (REGDISPLAY* result = &_regDisplay)
                {
                    return result;
                }
            }
        }

        internal UIntPtr SP => _regDisplay.SP;

        internal UIntPtr FramePointer => _framePointer;

        //internal unsafe bool Init(EH.PAL_LIMITED_CONTEXT* pStackwalkCtx, bool instructionFault = false)

        //internal unsafe bool Next()

        //internal unsafe bool Next(uint* uExCollideClauseIdx)

        //internal unsafe bool Next(uint* uExCollideClauseIdx, bool* fUnwoundReversePInvoke)
    }
}
