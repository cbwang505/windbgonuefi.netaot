using System.Runtime.InteropServices;

namespace System.Runtime
{
    [StructLayout(LayoutKind.Explicit, Size = 304)]
    internal struct REGDISPLAY
    {
        [FieldOffset(120)]
        internal UIntPtr SP;
    }

}
