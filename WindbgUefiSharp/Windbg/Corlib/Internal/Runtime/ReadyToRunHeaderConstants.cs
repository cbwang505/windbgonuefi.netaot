using System.Runtime.InteropServices;

namespace Internal.Runtime
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct ReadyToRunHeaderConstants
    {
        public const uint Signature = 0x00525452; // 'RTR' //5395538u

        public const ushort CurrentMajorVersion = 8;
        public const ushort CurrentMinorVersion = 1;
    }
}
