using System;

namespace Internal.Runtime
{
    internal readonly struct Pointer
    {
        private readonly IntPtr _value;

        public IntPtr Value => _value;
    }
}
