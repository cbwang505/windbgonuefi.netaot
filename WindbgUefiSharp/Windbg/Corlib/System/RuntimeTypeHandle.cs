
using System.Runtime.CompilerServices;

namespace System
{
    public struct RuntimeTypeHandle 
    {
        private EETypePtr _pEEType;

        public RuntimeTypeHandle(EETypePtr ptr)
        {
            _pEEType = _pEEType;
        }

        [Intrinsic]
        internal unsafe static IntPtr GetValueInternal(RuntimeTypeHandle handle)
        {
            return (IntPtr)handle._pEEType.ToPointer();
        }
    }
}
