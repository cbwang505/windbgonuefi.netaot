
using System.Collections;
using System.Collections.Generic;

namespace System
{
    public struct UInt32
    {
        private uint _value;

        public const uint MaxValue = 0xffffffff;

        public const uint MinValue = 0u;

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
            return ((ulong)this) == ((ulong)o);
        }


        public static UInt32 FromByteList(IEnumerable<byte> bts)
        {
            UInt32 ret= 0;
            int remain = bts.Count;
            if (remain > 4)
            {
                remain = 4;
            }

            int i = 0;
            foreach (byte bt in bts)
            {
                UInt32 tmp = (UInt32)bt;
                ret |= (tmp << (i * 8));
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
