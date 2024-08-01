using System;
using System.Runtime;

namespace Internal.Runtime
{
    internal static class IDynamicCastableSupport
    {
        [RuntimeExport("IDynamicCastableIsInterfaceImplemented")]
        internal unsafe static bool IDynamicCastableIsInterfaceImplemented(object instance, MethodTable* interfaceType, bool throwIfNotImplemented)
        {
            return false;
        }

        [RuntimeExport("IDynamicCastableGetInterfaceImplementation")]
        internal unsafe static IntPtr IDynamicCastableGetInterfaceImplementation(object instance, MethodTable* interfaceType, ushort slot)
        {
            //RuntimeImports.RhpFallbackFailFast();
            return (IntPtr)0;
        }
    }
}
