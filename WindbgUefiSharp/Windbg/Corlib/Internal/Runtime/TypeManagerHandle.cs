using System;
using System.Runtime.InteropServices;

namespace Internal.Runtime
{
    public struct TypeManagerHandle
    {
        private struct TypeManager
        {
            public IntPtr OsHandle;

            public IntPtr ReadyToRunHeader;

            public IntPtr DispatchMap;
        }

        private unsafe TypeManager* _handleValue;

        public unsafe bool IsNull => _handleValue == null;

        public unsafe IntPtr OsModuleBase => _handleValue->OsHandle;

        public unsafe IntPtr DispatchMap => _handleValue->DispatchMap;

        public unsafe TypeManagerHandle(IntPtr handleValue)
        {
            _handleValue = (TypeManager*)(void*)handleValue;
        }

        public unsafe IntPtr GetIntPtrUNSAFE()
        {
            return (IntPtr)_handleValue;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct TypeManagerHandleRaw
    {
        public IntPtr OsModuleBase;

        public IntPtr ReadyToRunHeader;

        public IntPtr DispatchMap;
    }
}