using System;
using System.Runtime;

namespace Internal.Runtime
{
    internal struct DynamicModule
    {
        private int _cbSize;

        private unsafe delegate*<MethodTable*, MethodTable*, ushort, IntPtr> _dynamicTypeSlotDispatchResolve;

        private unsafe delegate*<ExceptionIDs, Exception> _getRuntimeException;

        public unsafe delegate*<MethodTable*, MethodTable*, ushort, IntPtr> DynamicTypeSlotDispatchResolve
        {
            get
            {
                if (_cbSize >= sizeof(IntPtr) * 2)
                {
                    return _dynamicTypeSlotDispatchResolve;
                }
                return null;
            }
        }

        public unsafe delegate*<ExceptionIDs, Exception> GetRuntimeException
        {
            get
            {
                if (_cbSize >= sizeof(IntPtr) * 3)
                {
                    return _getRuntimeException;
                }
                return null;
            }
        }
    }
}
