using System;
using System.Runtime.InteropServices;

namespace Internal.Runtime.NativeFormat
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct NativePrimitiveDecoder
    {
        public unsafe static byte ReadUInt8(ref byte* stream)
        {
            byte result = *stream;
            stream++;
            return result;
        }

        public unsafe static ushort ReadUInt16(ref byte* stream)
        {
            ushort result = *(ushort*)stream;
            stream += 2;
            return result;
        }

        public unsafe static uint ReadUInt32(ref byte* stream)
        {
            uint result = *(uint*)stream;
            stream += 4;
            return result;
        }

        public unsafe static ulong ReadUInt64(ref byte* stream)
        {
            ulong result = *(ulong*)stream;
            stream += 8;
            return result;
        }
        public unsafe static byte ReadUInt8(ref IntPtr stream)
        {
            byte* buffer = (byte*)stream;
            byte result = *buffer;
            stream+=1;
            return result;
        }

        public unsafe static ushort ReadUInt16(ref IntPtr stream)
        {
            ushort* buffer = (ushort*)stream;
            ushort result = *(ushort*)buffer;
            stream += 2;
            return result;
        }

        public unsafe static uint ReadUInt32(ref IntPtr stream)
        {
            uint* buffer = (uint*)stream;
            uint result = *(uint*)buffer;
            stream += 4;
            return result;
        }

        public unsafe static ulong ReadUInt64(ref IntPtr stream)
        {
            ulong* buffer = (ulong*)stream;
            ulong result = *(ulong*)buffer;
            stream += 8;
            return result;
        }
        public unsafe static byte ReadUInt8( IntPtr stream)
        {
            byte* buffer = (byte*)stream;
            byte result = *buffer;
           
            return result;
        }

        public unsafe static ushort ReadUInt16( IntPtr stream)
        {
            ushort* buffer = (ushort*)stream;
            ushort result = *(ushort*)buffer;
            
            return result;
        }

        public unsafe static uint ReadUInt32( IntPtr stream)
        {
            uint* buffer = (uint*)stream;
            uint result = *(uint*)buffer;
            
            return result;
        }

        public unsafe static ulong ReadUInt64(IntPtr stream)
        {
            ulong* buffer = (ulong*)stream;
            ulong result = *(ulong*)buffer;
            return result;
        }

        public unsafe static float ReadFloat(ref byte* stream)
        {
            uint value = ReadUInt32(ref stream);
            return *(float*)(&value);
        }

        public unsafe static double ReadDouble(ref byte* stream)
        {
            ulong value = ReadUInt64(ref stream);
            return *(double*)(&value);
        }

        public static uint GetUnsignedEncodingSize(uint value)
        {
            if (value < 128)
            {
                return 1u;
            }
            if (value < 16384)
            {
                return 2u;
            }
            if (value < 2097152)
            {
                return 3u;
            }
            if (value < 268435456)
            {
                return 4u;
            }
            return 5u;
        }

        public unsafe static uint DecodeUnsigned(ref byte* stream)
        {
            uint value = 0u;
            uint val = *stream;
            if ((val & 1) == 0)
            {
                value = val >> 1;
                stream++;
            }
            else if ((val & 2) == 0)
            {
                value = (val >> 2) | (uint)(stream[1] << 6);
                stream += 2;
            }
            else if ((val & 4) == 0)
            {
                value = (val >> 3) | (uint)(stream[1] << 5) | (uint)(stream[2] << 13);
                stream += 3;
            }
            else if ((val & 8) == 0)
            {
                value = (val >> 4) | (uint)(stream[1] << 4) | (uint)(stream[2] << 12) | (uint)(stream[3] << 20);
                stream += 4;
            }
            else
            {
                if ((val & 0x10u) != 0)
                {
                    return 0u;
                }
                stream++;
                value = ReadUInt32(ref stream);
            }
            return value;
        }

        public unsafe static int DecodeSigned(ref byte* stream)
        {
            int value = 0;
            int val = *stream;
            if ((val & 1) == 0)
            {
                value = (sbyte)val >> 1;
                stream++;
            }
            else if ((val & 2) == 0)
            {
                value = (val >> 2) | (stream[1] << 6);
                stream += 2;
            }
            else if ((val & 4) == 0)
            {
                value = (val >> 3) | (stream[1] << 5) | (stream[2] << 13);
                stream += 3;
            }
            else if ((val & 8) == 0)
            {
                value = (val >> 4) | (stream[1] << 4) | (stream[2] << 12) | (stream[3] << 20);
                stream += 4;
            }
            else
            {
                if (((uint)val & 0x10u) != 0)
                {
                    return 0;
                }
                stream++;
                value = (int)ReadUInt32(ref stream);
            }
            return value;
        }

        public unsafe static ulong DecodeUnsignedLong(ref byte* stream)
        {
            ulong value = 0uL;
            byte val = *stream;
            if ((val & 0x1F) != 31)
            {
                return DecodeUnsigned(ref stream);
            }
            if ((val & 0x20) == 0)
            {
                stream++;
                return ReadUInt64(ref stream);
            }
            return 0uL;
        }

        public unsafe static long DecodeSignedLong(ref byte* stream)
        {
            long value = 0L;
            byte val = *stream;
            if ((val & 0x1F) != 31)
            {
                return DecodeSigned(ref stream);
            }
            if ((val & 0x20) == 0)
            {
                stream++;
                return (long)ReadUInt64(ref stream);
            }
            return 0L;
        }
    }
}
