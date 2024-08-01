using System.Runtime.CompilerServices;

namespace Internal.Runtime
{
    internal readonly struct RelativePointer<T> where T : unmanaged
    {
        private readonly int _value;

        public unsafe T* Value => (T*)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + _value);
    }
}
