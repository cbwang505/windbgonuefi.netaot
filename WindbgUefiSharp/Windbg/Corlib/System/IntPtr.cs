using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System
{
    public struct IntPtr
    {
        private unsafe void* _value;

        [Intrinsic]
        public static readonly IntPtr Zero;

        public static int Size
        {
            [Intrinsic]
            get => 8;
        }

        [Intrinsic]
        public unsafe IntPtr(void* value) => _value = value;

        [Intrinsic]
        public unsafe IntPtr(int value) => _value = (void*)value;

        [Intrinsic]
        public unsafe IntPtr(uint value) => _value = (void*)value;

        [Intrinsic]
        public unsafe IntPtr(long value) => _value = (void*)value;

        [Intrinsic]
        public unsafe IntPtr(ulong value) => _value = (void*)value;


        /*
        [Intrinsic]
        public unsafe IntPtr(UInt64 value) => _value = (ulong)value;*/


        [Intrinsic]
        public unsafe long ToInt64() => (long)_value;
        public unsafe ulong ToUInt64()
        {
            return (ulong)_value;
        }

        [Intrinsic]
        public static explicit operator IntPtr(int value) => new IntPtr(value);

        [Intrinsic]
        public static explicit operator IntPtr(uint value) => new IntPtr(value);

        [Intrinsic]
        public static explicit operator IntPtr(long value) => new IntPtr(value);

        [Intrinsic]
        public static explicit operator IntPtr(ulong value) => new IntPtr(value);

        /*[Intrinsic]
        public static explicit operator IntPtr(UInt64 value) => new IntPtr((ulong)value);*/

        [Intrinsic]
        public unsafe static explicit operator IntPtr(void* value) => new IntPtr(value);

        [Intrinsic]
        public unsafe static explicit operator void*(IntPtr value) => value._value;

        [Intrinsic]
        public unsafe static explicit operator int(IntPtr value) => (int)value._value;
        //{
        //var l = (long)value._value;

        //  return checked((int)l);
        //}

        [Intrinsic]
        public unsafe static explicit operator long(IntPtr value) => (long)value._value;

        [Intrinsic]
        public unsafe static explicit operator ulong(IntPtr value) => (ulong)value._value;

        [Intrinsic]
        public unsafe static explicit operator IntPtr(UIntPtr ptr) => new IntPtr() { _value = (void*)ptr };

        [Intrinsic]
        public unsafe static bool operator == (IntPtr a,IntPtr b) 
        {
            return a._value == b._value;
        }

        [Intrinsic]
        public unsafe static bool operator !=(IntPtr a, IntPtr b)
        {
            return !(a._value == b._value);
        }
        [Intrinsic]
        public unsafe static bool operator >(IntPtr a, IntPtr b)
        {
            return (a._value >b._value);
        }
        [Intrinsic]
        public unsafe static bool operator >=(IntPtr a, IntPtr b)
        {
            return (a._value >= b._value);
        }

        [Intrinsic]
        public unsafe static bool operator <(IntPtr a, IntPtr b)
        {
            return (a._value <b._value);
        }  [Intrinsic]
        public unsafe static bool operator <=(IntPtr a, IntPtr b)
        {
            return (a._value <=b._value);
        }

        [Intrinsic]
        public unsafe static IntPtr operator +(IntPtr a, uint b)
            => new IntPtr((byte*)a._value + b);

        [Intrinsic]
        public unsafe static IntPtr operator +(IntPtr a, int b)
            => new IntPtr((byte*)a._value + b);
        [Intrinsic]
        public unsafe static IntPtr operator -(IntPtr a, int b)
            => new IntPtr((byte*)a._value - b);
        [Intrinsic]
        public unsafe static IntPtr operator -(IntPtr a, IntPtr b)
            => new IntPtr((ulong)a._value - (ulong)b._value);
        [Intrinsic]
        public unsafe static IntPtr operator +(IntPtr a, IntPtr b)
            => new IntPtr((ulong)a._value + (ulong)b._value);

        [Intrinsic]
        public unsafe static IntPtr operator >>(IntPtr a, int b)
            => new IntPtr((ulong)a._value >> b);


        [Intrinsic]
        public unsafe static IntPtr operator <<(IntPtr a, int b)
            => new IntPtr((ulong)a._value << b);
        [Intrinsic]
        public unsafe static IntPtr operator &(IntPtr a, int b)
            => new IntPtr((ulong)a._value & b);
        //[Intrinsic]?
        public unsafe bool Equals(IntPtr ptr) => _value == ptr._value;

        //[Intrinsic]?
        public override string ToString()
        {
            return ((UIntPtr)this).ToString();
        }

        public  string ToString(string format) {
            return ((UIntPtr)this).ToString(format);
        }

        public ByteList ToByteList()
        {
            ByteList ret=new ByteList(Size);
            long val = ToInt64();
            for (int i = 0; i < Size; i++)
            {
                byte tmp = (byte)((val >> (i * 8)));

                ret.Add(tmp);
            }
            return ret;
        }
    }
}