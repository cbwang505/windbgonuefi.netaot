using System.Runtime.CompilerServices;

namespace System
{
    public abstract class Enum : ValueType
    {
        [Intrinsic]
        public bool HasFlag(Enum flag)
        {
            return false;
        }


        public unsafe override string ToString()
        {
            Enum cp = this;
            IntPtr ret = Unsafe.As<Enum, IntPtr>(ref cp);
            ulong val = *(ulong*)(ret + sizeof(IntPtr));
            return val.ToString();
        }

        public unsafe string ToString(string format)
        {
            fixed (byte* cp = &this.GetRawData())
            {
                ulong val = *(ulong*)(cp);

                return val.ToString(format);
            }
        }
    }
}
