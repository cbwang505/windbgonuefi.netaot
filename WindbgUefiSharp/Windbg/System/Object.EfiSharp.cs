﻿using Internal.Runtime.CompilerServices;

namespace System
{
    public unsafe partial class Object
    {
        //From https://github.com/Michael-Kelley/RoseOS/blob/ecd805014a/CoreLib/System/Object.cs#L21
        public void Free()
        {
            object obj = this;
            IntPtr pObj = System.Runtime.CompilerServices.Unsafe.As<object, IntPtr>(ref obj);
#if WINDOWS
            Interop.Kernel32.LocalFree(pObj);
#elif EFI
            EfiSharp.UefiApplication.SystemTable->BootServices->FreePool(pObj);
#endif
        }
    }
}
