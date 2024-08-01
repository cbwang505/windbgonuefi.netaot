using System;
using System.Runtime.InteropServices;

namespace Internal.Runtime
{
#pragma warning disable 0169
    [StructLayout(LayoutKind.Sequential)]
    internal struct ReadyToRunHeader
    {
        public uint Signature;      // ReadyToRunHeaderConstants.Signature
        public ushort MajorVersion;

        public ushort MinorVersion;

        public uint Flags;

        public ushort NumberOfSections;

        public byte EntrySize;

        public byte EntryType;

        // Array of sections follows.
    };
#pragma warning restore 0169
}
