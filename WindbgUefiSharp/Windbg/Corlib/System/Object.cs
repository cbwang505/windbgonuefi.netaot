using EfiSharp;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime;
using Internal.Runtime.CompilerHelpers;

namespace System
{
    public unsafe class Object
    {
        // The layout of object is a contract with the compiler.
        public unsafe MethodTable* m_pEEType;

        [StructLayout(LayoutKind.Sequential)]
        private class RawData
        {
            public byte Data;
        }

        public ref byte GetRawData()
        {
            return ref Unsafe.As<RawData>(this).Data;
        }
        public IntPtr GetRawDataPtr()
        {
            ref byte byteptr= ref Unsafe.As<RawData>(this).Data;
            IntPtr ret =(IntPtr)Unsafe.AsPointer(ref Unsafe.AsRef(in byteptr));
            return ret;
        }


        public ByteList GetRawDataBytes()
        {

            IntPtr buffptr = GetRawDataPtr();
            int len =(int)GetRawDataSize();
            ByteList ret=new ByteList(len);
            ret.From(buffptr, len);
            return ret;
        }

        public int SetRawData(IntPtr src, int count)
        {

            int len = (int)GetRawDataSize();

            if (count > len)
            {
                count = len;
            }
            IntPtr buffptr = GetRawDataPtr();
            StartupCodeHelpers.MemCpy((byte*)buffptr, (byte*)src, count);

            return count;
        }

        public int SetRawData(IntPtr src)
        {
            int count = (int)GetRawDataSize();
            SetRawData(src, count);
            return count;
        }

        public int SetRawData(ByteList src, int count)
        {

            int len = (int)GetRawDataSize();

            if (count > len)
            {
                count = len;
            }
            IntPtr buffptr = GetRawDataPtr();
            src.Move(buffptr, count);

            return count;
        }
        public int SetRawData(ByteList src,int offset, int count)
        {

            int len = (int)GetRawDataSize();

            if (count > len)
            {
                count = len;
            }
            IntPtr buffptr = GetRawDataPtr();
            src.Move(buffptr, offset,0, count);

            return count;
        }

        public int SetRawData(ByteList src)
        {
            int count = (int)GetRawDataSize();
            SetRawData(src, count);
            return count;
        }

        public unsafe MethodTable* GetMethodTable()
        {
            return m_pEEType;
        }

        public uint GetRawDataSize()
        {
            return (uint)((int)GetMethodTable()->BaseSize - sizeof(ObjHeader) - sizeof(MethodTable*));
        }

        public Object() { }
        ~Object() { }

        public virtual bool Equals(object o)
            => false;

        public virtual int GetHashCode()
            => 0;

        public virtual string ToString()
            => "System.Object";

        public virtual void Dispose()
        {
            var obj = this;
            StartupCodeHelpers.free(Unsafe.As<object, IntPtr>(ref obj));
        }

        public static implicit operator bool(object obj)=> obj != null;

        public static implicit operator IntPtr(object obj) => Unsafe.As<object, IntPtr>(ref obj);

        
    }
}
