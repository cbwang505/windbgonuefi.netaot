using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System
{
    public class Type
    {
        private readonly RuntimeTypeHandle _typeHandle;

        public RuntimeTypeHandle TypeHandle => _typeHandle;

        private Type(RuntimeTypeHandle typeHandle)
        {
            _typeHandle = typeHandle;
        }

        public static Type GetTypeFromHandle(RuntimeTypeHandle rth)
        {
            return new Type(rth);
        }

        [Intrinsic]
        public static bool operator ==(Type left, Type right)
        {
            return RuntimeTypeHandle.GetValueInternal(left._typeHandle) == RuntimeTypeHandle.GetValueInternal(right._typeHandle);
        }

        [Intrinsic]
        public static bool operator !=(Type left, Type right)
        {
            return !(left == right);
        }

        public override bool Equals(object o)
        {
            if (o is Type)
            {
                return this == (Type)o;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
