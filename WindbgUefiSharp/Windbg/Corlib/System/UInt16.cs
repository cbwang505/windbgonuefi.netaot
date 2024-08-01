
using System.Collections;
using System.Collections.Generic;

namespace System
{
    public struct UInt16
    {
        private ushort _value;

        public const ushort MaxValue = 65535;

        public const ushort MinValue = 0;

        public unsafe override string ToString()
        {
            return ((ulong)this).ToString();
        }

        public string ToString(string format)
        {
            return ((ulong)this).ToString(format);
        }



        public unsafe override bool Equals(object o)
        {
            return ((ulong)this) > ((ulong)o);
        }

        public static UInt16 FromByteList(IEnumerable<byte> bts)
        {
            UInt16 ret = 0;
            int remain = bts.Count;
            if (remain > 2)
            {
                remain =2;
            }

            int i = 0;
            foreach (byte bt in bts)
            {
                UInt16 tmp = (UInt16)bt;
                ret |= (UInt16)(tmp << (i * 8));
                i++;
                if (remain == i)
                {
                    break;
                }
            }
           
            return ret;
        }
    }
}
