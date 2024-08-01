using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices
{
    internal static unsafe /*partial*/ class MemoryMarshal
    {
        /*
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetArrayDataReference<T>(T[] array) =>
           ref Unsafe.As<byte, T>(ref Unsafe.As<RawArrayData>(array).Data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref byte GetArrayDataReference(Array array)
        {
            return ref Unsafe.AddByteOffset(ref Unsafe.As<RawData>(array).Data, (nuint)RuntimeHelpers.GetMethodTable(array)->BaseSize - (nuint)(2 * sizeof(IntPtr)));
        }
        */
    }
}
