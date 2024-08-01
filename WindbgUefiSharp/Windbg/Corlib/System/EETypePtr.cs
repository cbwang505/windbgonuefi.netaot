using Internal.Runtime;
using System.Runtime.CompilerServices;

namespace System
{
    public unsafe struct EETypePtr
    {
        //In better theory this should be private, i'll get to that later affected classes:
        // ArrayHelpers.cs, Array.cs, String.cs
        internal MethodTable* _value;

        public bool IsSzArray 
        {
            get 
            {
                return _value->IsSzArray;
            }
        }

        public EETypePtr ArrayElementType 
        {
            get 
            {
                return new EETypePtr(_value->RelatedParameterType);
            }
        }

        internal int ArrayRank
        {
            get
            {
                return _value->ArrayRank;
            }
        }

        public IntPtr RawValue 
        {
            get 
            {
                return (IntPtr)_value;
            }
        }

        public EETypePtr(IntPtr value)
        {
            _value = (MethodTable*)value;
        }

        public EETypePtr(MethodTable* value)
        {
            _value = value;
        }

        public bool Equals(EETypePtr ptr)
        {
            return _value == ptr._value;
        }

        public unsafe MethodTable* ToPointer()
        {
            return (MethodTable*)(void*)_value;
        }

        [Intrinsic]
        internal static EETypePtr EETypePtrOf<T>()
        {
            // Compilers are required to provide a low level implementation of this method.
            // This can be achieved by optimizing away the reflection part of this implementation
            // by optimizing typeof(!!0).TypeHandle into "ldtoken !!0", or by
            // completely replacing the body of this method.
            return default;
        }
    }
}