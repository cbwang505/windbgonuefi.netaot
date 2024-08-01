using System.Runtime.CompilerServices;

namespace System
{
    public struct UIntPtr
    {
        private unsafe void* _value;

        [Intrinsic]
        public static readonly UIntPtr Zero;

        [Intrinsic]
        public unsafe UIntPtr(void* value) => _value = value;

        [Intrinsic]
        public unsafe UIntPtr(int value) => _value = (void*)value;

        [Intrinsic]
        public unsafe UIntPtr(uint value) => _value = (void*)value;

        [Intrinsic]
        public unsafe UIntPtr(long value) => _value = (void*)value;

        [Intrinsic]
        public unsafe UIntPtr(ulong value) => _value = (void*)value;

        //[Intrinsic]?
        public unsafe bool Equals(UIntPtr ptr)
            => _value == ptr._value;

        [Intrinsic]
        public static explicit operator UIntPtr(int value) => new UIntPtr(value);

        [Intrinsic]
        public static explicit operator UIntPtr(uint value) => new UIntPtr(value);

        [Intrinsic]
        public static explicit operator UIntPtr(long value) => new UIntPtr(value);

        [Intrinsic]
        public static explicit operator UIntPtr(ulong value) => new UIntPtr(value);

        [Intrinsic]
        public unsafe static explicit operator UIntPtr(void* value) => new UIntPtr(value);

        [Intrinsic]
        public unsafe static explicit operator void*(UIntPtr value) => value._value;

        [Intrinsic]
        public unsafe static explicit operator int(UIntPtr value) => (int)value._value;

        [Intrinsic]
        public unsafe static explicit operator long(UIntPtr value) => (long)value._value;

        [Intrinsic]
        public unsafe static explicit operator ulong(UIntPtr value) => (ulong)value._value;

        [Intrinsic]
        public unsafe static explicit operator UIntPtr(IntPtr ptr) => new UIntPtr() { _value = (void*)ptr };

        [Intrinsic]
        public unsafe static bool operator ==(UIntPtr a, UIntPtr b) => a._value == b._value;

        [Intrinsic]
        public unsafe static bool operator !=(UIntPtr a, UIntPtr b) =>!(a._value == b._value);
        
        [Intrinsic]
        public unsafe static UIntPtr operator +(UIntPtr a, uint b)
            => new UIntPtr((byte*)a._value + b);

        [Intrinsic]
        public unsafe static UIntPtr operator +(UIntPtr a, ulong b)
            => new UIntPtr((byte*)a._value + b);

        //[Intrinsic]?
        public unsafe override string ToString() => ((ulong)_value).ToString();

        public unsafe  string ToString(string format) => ((ulong)_value).ToString(format);
    }
}
