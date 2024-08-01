using System;

namespace Internal.Runtime
{
    [Flags]
    internal enum ModuleInfoFlags : int
    {
        HasEndPointer = 0x1,
    }
}
