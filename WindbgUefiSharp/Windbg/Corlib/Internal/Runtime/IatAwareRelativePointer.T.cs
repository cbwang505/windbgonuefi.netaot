using System.Runtime.CompilerServices;

namespace Internal.Runtime
{
    internal readonly struct IatAwareRelativePointer<T> where T : unmanaged
    {
        private readonly int _value;

        public unsafe T* Value
        {
            get
            {
                if ((_value & 1) == 0)
                {
                    return (T*)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + _value);
                }
                return *(T**)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + (_value & -2));
            }
        }
    }
}
