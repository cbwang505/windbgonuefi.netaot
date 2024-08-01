
using System;
using System.Runtime.CompilerServices;

namespace Internal.Runtime
{
    internal readonly struct RelativePointer
    {
        private readonly int _value;

        public unsafe IntPtr Value => (IntPtr)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + _value);
    }
}
