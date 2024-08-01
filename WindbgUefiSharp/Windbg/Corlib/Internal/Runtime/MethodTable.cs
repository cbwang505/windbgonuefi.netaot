using System.Runtime.CompilerServices;
using Internal.Runtime.NativeFormat;
using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Internal.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MethodTable
    {
        private const int POINTER_SIZE = 8;
        private const int PADDING = 1; // _numComponents is padded by one Int32 to make the first element pointer-aligned
        internal const int SZARRAY_BASE_SIZE = POINTER_SIZE + POINTER_SIZE + (1 + PADDING) * 4;

        [StructLayout(LayoutKind.Explicit)]
        private unsafe struct RelatedTypeUnion
        {
            // Kinds.CanonicalEEType
            [FieldOffset(0)]
            public MethodTable* _pBaseType;
            [FieldOffset(0)]
            public MethodTable** _ppBaseTypeViaIAT;

            // Kinds.ClonedEEType
            [FieldOffset(0)]
            public MethodTable* _pCanonicalType;
            [FieldOffset(0)]
            public MethodTable** _ppCanonicalTypeViaIAT;

            // Kinds.ArrayEEType
            [FieldOffset(0)]
            public MethodTable* _pRelatedParameterType;
            [FieldOffset(0)]
            public MethodTable** _ppRelatedParameterTypeViaIAT;
        }

        public static class OptionalFieldsReader
        {
            internal unsafe static uint GetInlineField(byte* pFields, EETypeOptionalFieldTag eTag, uint uiDefaultValue)
            {

                if (pFields == null)
                    return uiDefaultValue;

                bool isLastField = false;

                while (!isLastField)
                {
                    byte fieldHeader = NativePrimitiveDecoder.ReadUInt8(ref pFields);
                    isLastField = (fieldHeader & 0x80) != 0;
                    EETypeOptionalFieldTag eCurrentTag = (EETypeOptionalFieldTag)(fieldHeader & 0x7Fu);//0x7f
                    uint uiCurrentValue = NativePrimitiveDecoder.DecodeUnsigned(ref pFields);

                    // If we found a tag match return the current value.
                    if (eCurrentTag == eTag)
                        return uiCurrentValue;
                }

                // Reached end of stream without getting a match. Field is not present so return default value.
                return uiDefaultValue;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe readonly struct GenericComposition
        {
            public readonly ushort Arity;

            private readonly EETypeRef _genericArgument1;

            public EETypeRef* GenericArguments => (EETypeRef*)Unsafe.AsPointer(ref Unsafe.AsRef(in _genericArgument1));

            // Generic variance directly follows the last generic argument
            public GenericVariance* GenericVariance => (GenericVariance*)(GenericArguments + Arity);
        }

        private ushort _usComponentSize;
        private ushort _usFlags;
        private uint _uBaseSize;
        private RelatedTypeUnion _relatedType;
        private ushort _usNumVtableSlots;
        private ushort _usNumInterfaces;
        private uint _uHashCode;

        // These masks and paddings have been chosen so that the ValueTypePadding field can always fit in a byte of data.
        // if the alignment is 8 bytes or less. If the alignment is higher then there may be a need for more bits to hold
        // the rest of the padding data.
        // If paddings of greater than 7 bytes are necessary, then the high bits of the field represent that padding
        private const uint ValueTypePaddingLowMask = 0x7;
        private const uint ValueTypePaddingHighMask = 0xFFFFFF00;
        private const uint ValueTypePaddingMax = 0x07FFFFFF;
        private const int ValueTypePaddingHighShift = 8;
        private const uint ValueTypePaddingAlignmentMask = 0xF8;
        private const int ValueTypePaddingAlignmentShift = 3;

        //This pulls from Internal/Runtime/TypeLoader/ExternalReferencesTable.cs
        /// <summary>
        /// Gets a value indicating whether the statically generated data structures use relative pointers.
        /// </summary>
       // [Intrinsic]
        internal static bool SupportsRelativePointers()
        {
            return true;
        } //get_

        internal static bool SupportsWritableData => SupportsRelativePointers();

        internal ushort ComponentSize => _usComponentSize;

        internal ushort GenericArgumentCount => _usComponentSize;

        internal ushort Flags => _usFlags;

        internal uint BaseSize => _uBaseSize;

        internal ushort NumVtableSlots => _usNumVtableSlots;

        internal ushort NumInterfaces => _usNumInterfaces;

        internal uint HashCode => _uHashCode;

        private EETypeKind Kind
        {
            get
            {
                return (EETypeKind)(_usFlags & (ushort)EETypeFlags.EETypeKindMask);
            }
        }
        //unsure if 0x0100 is same as 0x100
        internal bool HasOptionalFields => ((_usFlags & (ushort)EETypeFlags.OptionalFieldsFlag) != 0);

        //// Mark or determine that a type is generic and one or more of it's type parameters is co- or
        //// contra-variant. This only applies to interface and delegate types.
        internal bool HasGenericVariance => ((_usFlags & (ushort)EETypeFlags.GenericVarianceFlag) != 0);

        internal bool IsFinalizable => ((_usFlags & (ushort)EETypeFlags.HasFinalizerFlag) != 0);

        internal bool IsNullable => ElementType == EETypeElementType.Nullable;

        internal bool IsCloned => Kind == EETypeKind.ClonedEEType;

        internal bool IsCanonical => Kind == EETypeKind.CanonicalEEType;

        // String is currently the only non-array type with a non-zero component size.
        internal bool IsString // => ComponentSize == sizeof(char) && !IsArray && !IsGenericTypeDefinition;
        {
            get
            {
                if (ComponentSize == sizeof(char) && !IsArray)
                    return !IsGenericTypeDefinition;

                return false;
            }
        }

        internal bool IsArray
        {
            get
            {
                EETypeElementType elementType = ElementType;
                if (elementType != EETypeElementType.Array)
                    return elementType == EETypeElementType.SzArray;

                return true;
                //return elementType == EETypeElementType.Array || elementType == EETypeElementType.SzArray;
            }
        }

        internal int ArrayRank
        {
            get
            {
                int boundsSize = (int)ParameterizedTypeShape - SZARRAY_BASE_SIZE;
                if (boundsSize > 0)
                {
                    // Multidim array case: Base size includes space for two Int32s
                    // (upper and lower bound) per each dimension of the array.
                    return boundsSize / (2 * sizeof(int));
                }
                return 1;
            }
        }

        internal static class WellKnownEETypes
        {
            // Returns true if the passed in EEType is the EEType for System.Object
            // This is recognized by the fact that System.Object and interfaces are the only ones without a base type
            internal static unsafe bool IsSystemObject(MethodTable* pEEType)
            {
                if (pEEType->IsArray)
                    return false;
                return (pEEType->NonArrayBaseType == null) && !pEEType->IsInterface;
            }

            // Returns true if the passed in EEType is the EEType for System.Array.
            // The binder sets a special CorElementType for this well known type
            internal static unsafe bool IsSystemArray(MethodTable* pEEType)
            {
                return (pEEType->ElementType == EETypeElementType.SystemArray);
            }
        }

        internal bool IsSzArray => ElementType == EETypeElementType.SzArray;

        internal bool IsGeneric => ((_usFlags & (ushort)EETypeFlags.IsGenericFlag) != 0);

        internal bool IsGenericTypeDefinition => Kind == EETypeKind.GenericTypeDefEEType;

        internal unsafe MethodTable* GenericDefinition
        {
            get
            {
                //Debug.Assert(IsGeneric);
                if (IsDynamicType || !SupportsRelativePointers())
                    return GetField<IatAwarePointer<MethodTable>>(EETypeField.ETF_GenericDefinition).Value;

                return GetField<IatAwareRelativePointer<MethodTable>>(EETypeField.ETF_GenericDefinition).Value;
            }
        }

        internal unsafe uint GenericArity
        {
            get
            {
                //Debug.Assert(IsGeneric);
                if (IsDynamicType || !SupportsRelativePointers())
                    return GetField<Pointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->Arity;

                return GetField<RelativePointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->Arity;
            }
        }

        internal unsafe EETypeRef* GenericArguments
        {
            get
            {
                //Debug.Assert(IsGeneric);
                if (IsDynamicType || !SupportsRelativePointers())
                    return GetField<Pointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->GenericArguments;

                return GetField<RelativePointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->GenericArguments;
            }
        }

        internal unsafe GenericVariance* GenericVariance
        {
            get
            {
                //Debug.Assert(IsGeneric);

                if (!HasGenericVariance)
                    return null;

                if (IsDynamicType || !SupportsRelativePointers())
                    return GetField<Pointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->GenericVariance;

                return GetField<RelativePointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->GenericVariance;
            }
        }

        internal bool IsPointerType => ElementType == EETypeElementType.Pointer;

        internal bool IsByRefType => ElementType == EETypeElementType.ByRef;

        internal bool IsInterface => ElementType == EETypeElementType.Interface;

        internal bool IsAbstract
        {
            get
            {
                if (!IsInterface)
                    return (RareFlags & EETypeRareFlags.IsAbstractClassFlag) != 0;

                return true;
                //return IsInterface || (RareFlags & EETypeRareFlags.IsAbstractClassFlag) != 0;
            }
        }

        internal bool IsByRefLike => (RareFlags & EETypeRareFlags.IsByRefLikeFlag) != 0;

        internal bool IsDynamicType => (_usFlags & (ushort)EETypeFlags.IsDynamicTypeFlag) != 0;

        internal bool HasDynamicallyAllocatedDispatchMap => (RareFlags & EETypeRareFlags.HasDynamicallyAllocatedDispatchMapFlag) != 0;

        internal bool IsParameterizedType => Kind == EETypeKind.ParameterizedEEType;

        // The parameterized type shape defines the particular form of parameterized type that
        // is being represented.
        // Currently, the meaning is a shape of 0 indicates that this is a Pointer,
        // shape of 1 indicates a ByRef, and >=SZARRAY_BASE_SIZE indicates that this is an array.
        // Two types are not equivalent if their shapes do not exactly match.
        internal uint ParameterizedTypeShape => _uBaseSize;

        internal bool IsRelatedTypeViaIAT => (_usFlags & (ushort)EETypeFlags.RelatedTypeViaIATFlag) != 0;

        internal bool RequiresAlign8 => (RareFlags & EETypeRareFlags.RequiresAlign8Flag) != 0;

        internal bool IsIDynamicInterfaceCastable => (_usFlags & 0x40) != 0;

        internal bool IsValueType => ElementType < EETypeElementType.Class;

        internal bool IsPrimitive => ElementType < EETypeElementType.ValueType;

        internal bool HasGCPointers => ((_usFlags & (ushort)EETypeFlags.HasPointersFlag) != 0);

        internal bool IsHFA => (RareFlags & EETypeRareFlags.IsHFAFlag) != 0;

        internal unsafe uint ValueTypeFieldPadding
        {
            get
            {
                byte* optionalFields = OptionalFieldsPtr;
                if (optionalFields == null)
                {
                    return 0u;
                }
                uint ValueTypeFieldPaddingData = OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.ValueTypeFieldPadding, 0u);
                uint padding = ValueTypeFieldPaddingData & 7u;
                return padding | ((ValueTypeFieldPaddingData & 0xFFFFFF00u) >> 5);
            }
        }

        //Debug.Assert(IsValueType);
        internal unsafe uint ValueTypeSize => BaseSize - (uint)(sizeof(ObjHeader) + sizeof(MethodTable*) + (int)ValueTypeFieldPadding);

        // This api is designed to return correct results for EETypes which can be derived from
        // And results indistinguishable from correct for DefTypes which cannot be derived from (sealed classes)
        // (For sealed classes, this should always return BaseSize-((uint)sizeof(ObjHeader));
        // Debug.Assert(!IsInterface && !IsParameterizedType);

        // get_BaseSize returns the GC size including space for the sync block index field, the EEType* and
        // padding for GC heap alignment. Must subtract all of these to get the size used for the fields of
        // the type (where the fields of the type includes the EEType*)
        internal unsafe uint FieldByteCountNonGCAligned => BaseSize - (uint)(sizeof(ObjHeader) + (int)ValueTypeFieldPadding);

        //internal bool IsICastable {
        //	get {
        //		return ((_usFlags & (ushort)EETypeFlags.ICastableFlag) != 0);
        //	}
        //}

        ///// <summary>
        ///// Gets the pointer to the method that implements ICastable.IsInstanceOfInterface.
        ///// </summary>
        //internal IntPtr ICastableIsInstanceOfInterfaceMethod {
        //	get {
        //		Debug.Assert(IsICastable);

        //		byte* optionalFields = OptionalFieldsPtr;
        //		if (optionalFields != null) {
        //			const ushort NoSlot = 0xFFFF;
        //			ushort uiSlot = (ushort)OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.ICastableIsInstSlot, NoSlot);
        //			if (uiSlot != NoSlot) {
        //				if (uiSlot < NumVtableSlots)
        //					return GetVTableStartAddress()[uiSlot];
        //				else
        //					return GetSealedVirtualSlot((ushort)(uiSlot - NumVtableSlots));
        //			}
        //		}

        //		EEType* baseType = BaseType;
        //		if (baseType != null)
        //			return baseType->ICastableIsInstanceOfInterfaceMethod;

        //		Debug.Assert(false);
        //		return IntPtr.Zero;
        //	}
        //}

        ///// <summary>
        ///// Gets the pointer to the method that implements ICastable.GetImplType.
        ///// </summary>
        //internal IntPtr ICastableGetImplTypeMethod {
        //	get {
        //		Debug.Assert(IsICastable);

        //		byte* optionalFields = OptionalFieldsPtr;
        //		if (optionalFields != null) {
        //			const ushort NoSlot = 0xFFFF;
        //			ushort uiSlot = (ushort)OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.ICastableGetImplTypeSlot, NoSlot);
        //			if (uiSlot != NoSlot) {
        //				if (uiSlot < NumVtableSlots)
        //					return GetVTableStartAddress()[uiSlot];
        //				else
        //					return GetSealedVirtualSlot((ushort)(uiSlot - NumVtableSlots));
        //			}
        //		}

        //		EEType* baseType = BaseType;
        //		if (baseType != null)
        //			return baseType->ICastableGetImplTypeMethod;

        //		Debug.Assert(false);
        //		return IntPtr.Zero;
        //	}
        //}

        internal unsafe EEInterfaceInfo* InterfaceMap
        {
            get
            {
                fixed (MethodTable* start = &this)
                {
                    // interface info table starts after the vtable and has _usNumInterfaces entries
                    return (EEInterfaceInfo*)((byte*)(start + 1) + sizeof(void*) * _usNumVtableSlots);
                }
            }
        }

        internal unsafe uint DispatchMapIndex
        {
            get
            {
                byte* optionalFields = OptionalFieldsPtr;

                if (optionalFields == null)
                    return 0;

                return OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.DispatchMap, 0xffffffff);
            }
        }
        internal unsafe bool HasDispatchMap
        {
            get
            {
                if (NumInterfaces == 0)
                    return false;

                byte* optionalFields = OptionalFieldsPtr;

                if (optionalFields == null)
                    return false;

                uint idxDispatchMap = OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.DispatchMap, 0xffffffff);

                if (idxDispatchMap == uint.MaxValue)
                {
                    if (HasDynamicallyAllocatedDispatchMap)
                        return true;

                    else if (IsDynamicType)
                        return DynamicTemplateType->HasDispatchMap;

                    // return RawBaseType->HasDispatchMap;
                    return false;
                }
                return true;
            }
        }

        //TODO: DispatchMap & TypeManager do not exist
        internal unsafe DispatchMap* DispatchMap
        {
            get
            {
                if (NumInterfaces == 0)
                    return null;

                byte* optionalFields = OptionalFieldsPtr;

                if (optionalFields == null)
                    return null;



                uint idxDispatchMap = OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.DispatchMap, uint.MaxValue);

                if (idxDispatchMap == uint.MaxValue && (IsDynamicType ))
                {
                    if (HasDynamicallyAllocatedDispatchMap)
                    {
                        fixed (MethodTable* pThis = &this)
                            return *(DispatchMap**)((byte*)pThis + GetFieldOffset(EETypeField.ETF_DynamicDispatchMap));
                    }

                    return DynamicTemplateType->DispatchMap;
                }

                /*
                if (idxDispatchMap == uint.MaxValue)
                {
                    Runtime.DispatchMap* ret = RawBaseType->DispatchMap;
                    return ret ;
                }*/
                //这不能用
                /*if (SupportsRelativePointers())
                {
                    return (DispatchMap*)(void*)FollowRelativePointer((int*)((byte*)(void*)TypeManager.DispatchMap + (long)idxDispatchMap * 4L));
                }*/

                return ((DispatchMap**)TypeManager.DispatchMap)[idxDispatchMap];
            }
        }

        // Get the address of the finalizer method for finalizable types.
        internal IntPtr FinalizerCode
        {
            get
            {
                //Debug.Assert(IsFinalizable);

                if (IsDynamicType || !SupportsRelativePointers())
                    return GetField<Pointer>(EETypeField.ETF_Finalizer).Value;

                return GetField<RelativePointer>(EETypeField.ETF_Finalizer).Value;
            }
        }

        internal unsafe MethodTable* BaseType
        {
            get
            {
                if (IsCloned)
                    return CanonicalEEType->BaseType;

                if (IsParameterizedType)
                {
                    if (IsArray)
                        return GetArrayEEType();

                    return null;
                }

                //Debug.Assert(IsCanonical);

                if (IsRelatedTypeViaIAT)
                    return *_relatedType._ppBaseTypeViaIAT;

                return _relatedType._pBaseType;
            }
        }

        internal unsafe MethodTable* NonArrayBaseType
        {
            get
            {
                if (IsCloned)
                {
                    // Assuming that since this is not an Array, the CanonicalEEType is also not an array
                    return CanonicalEEType->NonArrayBaseType;
                }

                if (IsRelatedTypeViaIAT)
                {
                    return *_relatedType._ppBaseTypeViaIAT;
                }

                return _relatedType._pBaseType;
            }
        }

        internal unsafe MethodTable* NonClonedNonArrayBaseType
        {
            get
            {
                if (IsRelatedTypeViaIAT)
                {
                    return *_relatedType._ppBaseTypeViaIAT;
                }

                return _relatedType._pBaseType;
            }
        }

        // Debug.Assert(!IsParameterizedType, "array type not supported in NonArrayBaseType");
        // Debug.Assert(!IsCloned, "cloned type not supported in NonClonedNonArrayBaseType");
        // Debug.Assert(IsCanonical, "we expect canonical types here");
        // Debug.Assert(!IsRelatedTypeViaIAT, "Non IAT");
        internal unsafe MethodTable* RawBaseType => _relatedType._pBaseType;

        internal unsafe MethodTable* CanonicalEEType
        {
            get
            {
                // cloned EETypes must always refer to types in other modules
                if (IsRelatedTypeViaIAT)
                    return *_relatedType._ppCanonicalTypeViaIAT;

                return _relatedType._pCanonicalType;
            }
        }

        // Debug.Assert(IsNullable);
        // Debug.Assert(GenericArity == 1);
        internal unsafe MethodTable* NullableType => GenericArguments->Value;

        ///// <summary>
        ///// Gets the offset of the value embedded in a Nullable&lt;T&gt;.
        ///// </summary>
        internal unsafe byte NullableValueOffset
        {
            get
            {
                //Debug.Assert(IsNullable);

                // Grab optional fields. If there aren't any then the offset was the default of 1 (immediately after the
                // Nullable's boolean flag).
                byte* optionalFields = OptionalFieldsPtr;
                if (optionalFields == null)
                    return 1;

                // The offset is never zero (Nullable has a boolean there indicating whether the value is valid). So the
                // offset is encoded - 1 to save space. The zero below is the default value if the field wasn't encoded at
                // all.
                return (byte)(OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.NullableValueOffset, 0u) + 1);
            }
        }

        internal unsafe MethodTable* RelatedParameterType
        {
            get
            {
                if (IsRelatedTypeViaIAT)
                    return *_relatedType._ppRelatedParameterTypeViaIAT;

                return _relatedType._pRelatedParameterType;
            }
        }

        internal unsafe byte* OptionalFieldsPtr
        {
            get
            {
                if (!HasOptionalFields)
                    return null;

                if (IsDynamicType || !SupportsRelativePointers())
                    return GetField<Pointer<byte>>(EETypeField.ETF_OptionalFieldsPtr).Value;

                return GetField<RelativePointer<byte>>(EETypeField.ETF_OptionalFieldsPtr).Value;
            }
        }

        internal unsafe MethodTable* DynamicTemplateType
        {
            get
            {
                uint cbOffset = GetFieldOffset(EETypeField.ETF_DynamicTemplateType);

                fixed (MethodTable* pThis = &this)
                {
                    return *(MethodTable**)((byte*)pThis + cbOffset);
                }
            }
        }

        internal unsafe IntPtr DynamicGcStaticsData
        {
            get
            {
                //Debug.Assert((RareFlags & EETypeRareFlags.IsDynamicTypeWithGcStatics) != 0);
                uint cbOffset = GetFieldOffset(EETypeField.ETF_DynamicGcStatics);
                fixed (MethodTable* pThis = &this)
                {
                    return *(IntPtr*)((byte*)pThis + cbOffset);
                }
            }
        }

        internal unsafe IntPtr DynamicNonGcStaticsData
        {
            get
            {
                //Debug.Assert((RareFlags & EETypeRareFlags.IsDynamicTypeWithNonGcStatics) != 0);
                uint cbOffset = GetFieldOffset(EETypeField.ETF_DynamicNonGcStatics);
                fixed (MethodTable* pThis = &this)
                {
                    return *(IntPtr*)((byte*)pThis + cbOffset);
                }
            }
        }

        internal unsafe IntPtr DynamicThreadStaticsIndex
        {
            get
            {
                uint cbOffset = GetFieldOffset(EETypeField.ETF_DynamicThreadStaticOffset);
                fixed (MethodTable* pThis = &this)
                {
                    return *(IntPtr*)((byte*)pThis + cbOffset);
                }
            }
        }

        internal unsafe DynamicModule* DynamicModule
        {
            get
            {
                if ((RareFlags & EETypeRareFlags.HasDynamicModuleFlag) != 0)
                {
                    uint cbOffset = GetFieldOffset(EETypeField.ETF_DynamicModule);
                    fixed (MethodTable* pThis = &this)
                    {
                        return *(DynamicModule**)((byte*)pThis + cbOffset);
                    }
                }

                return null;
            }
        }

        internal unsafe TypeManagerHandle TypeManager
        {
            get
            {
                IntPtr typeManagerIndirection = ((!IsDynamicType && SupportsRelativePointers()) ? GetField<RelativePointer>(EETypeField.ETF_TypeManagerIndirection).Value : GetField<Pointer>(EETypeField.ETF_TypeManagerIndirection).Value);
                return *(TypeManagerHandle*)(void*)typeManagerIndirection;
            }
        }

        internal IntPtr WritableData
        {
            get
            {
                uint offset = GetFieldOffset(EETypeField.ETF_WritableData);
                if (!IsDynamicType)
                {
                    return GetField<RelativePointer>(offset).Value;
                }
                return GetField<Pointer>(offset).Value;
            }
        }

        internal unsafe EETypeRareFlags RareFlags
        {
            get
            {
                if (!HasOptionalFields)
                {
                    return (EETypeRareFlags)0;
                }
                return (EETypeRareFlags)OptionalFieldsReader.GetInlineField(OptionalFieldsPtr, EETypeOptionalFieldTag.RareFlags, 0u);
                // If there are no optional fields then none of the rare flags have been set.
                // Get the flags from the optional fields. The default is zero if that particular field was not included.
                //return HasOptionalFields ? (EETypeRareFlags)OptionalFieldsReader.GetInlineField(OptionalFieldsPtr, EETypeOptionalFieldTag.RareFlags, 0u) : (EETypeRareFlags)0;
            }
        }

        internal unsafe int FieldAlignmentRequirement
        {
            get
            {
                byte* optionalFields = OptionalFieldsPtr;

                // If there are no optional fields then the alignment must have been the default, IntPtr.Size. 
                // (This happens for all reference types, and for valuetypes with default alignment and no padding)
                if (optionalFields == null)
                    return IntPtr.Size;

                // Get the value from the optional fields. The default is zero if that particular field was not included.
                // The low bits of this field is the ValueType field padding, the rest of the value is the alignment if present
                uint alignmentValue = (OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.ValueTypeFieldPadding, 0u) & ValueTypePaddingAlignmentMask) >> ValueTypePaddingAlignmentShift;

                // Alignment is stored as 1 + the log base 2 of the alignment, except a 0 indicates standard pointer alignment.
                if (alignmentValue == 0)
                    return IntPtr.Size;

                return 1 << ((int)alignmentValue - 1);
            }
        }

        internal /*readonly*/ EETypeElementType ElementType
            => (EETypeElementType)((_usFlags & (ushort)EETypeFlags.ElementTypeMask) >> (ushort)EETypeFlags.ElementTypeShift);

        public bool HasCctor => (RareFlags & EETypeRareFlags.HasCctorFlag) != 0;

        internal unsafe MethodTable* GetArrayEEType()
            => EETypePtr.EETypePtrOf<Array>().ToPointer();

        internal Exception GetClasslibException(ExceptionIDs id)
        {
            //Unimplemented atm
            //return RuntimeExceptionHelpers.GetRuntimeException(id);
            throw new PlatformNotSupportedException("GetClasslibException,ExceptionIDs:=>" + id.ToString("x"));
            return null;
        }

        //Unimplemented at the time
        //internal unsafe IntPtr GetClasslibFunction(ClassLibFunctionId id)
        //   => (IntPtr)InternalCalls.RhpGetClasslibFunctionFromEEType((MethodTable*)Unsafe.AsPointer(ref this), id);

        internal unsafe void SetToCloneOf(MethodTable* pOrigType)
        {
            _usFlags |= 1;
            _relatedType._pCanonicalType = pOrigType;
        }

        internal unsafe MethodTable* GetAssociatedModuleAddress()
        {
            fixed (MethodTable* pThis = &this)
            {
                if (!IsDynamicType)
                    return pThis;

                if (IsParameterizedType)
                    return pThis->RelatedParameterType->GetAssociatedModuleAddress();

                if (!IsGeneric)
                    return null;

                return pThis->GenericDefinition;
            }
        }

        internal bool SimpleCasting() => (_usFlags & 0x87) == 0;

        internal unsafe static bool BothSimpleCasting(MethodTable* pThis, MethodTable* pOther)
            => ((pThis->_usFlags | pOther->_usFlags) & 0x87) == 0;


        internal unsafe IntPtr GetClasslibFunction(ClassLibFunctionId id)
        {
            // return (IntPtr)InternalCalls.RhpGetClasslibFunctionFromEEType((MethodTable*)Unsafe.AsPointer(ref this), id);
            throw new PlatformNotSupportedException("GetClasslibFunction");
            return IntPtr.Zero;
        }


        internal unsafe bool IsEquivalentTo(MethodTable* pOtherEEType)
        {
            fixed (MethodTable* pThis = &this)
            {
                if (pThis == pOtherEEType)
                {
                    return true;
                }
                MethodTable* pThisEEType = pThis;
                if (pThisEEType->IsCloned)
                {
                    pThisEEType = pThisEEType->CanonicalEEType;
                }
                if (pOtherEEType->IsCloned)
                {
                    pOtherEEType = pOtherEEType->CanonicalEEType;
                }
                if (pThisEEType == pOtherEEType)
                {
                    return true;
                }
                if (pThisEEType->IsParameterizedType && pOtherEEType->IsParameterizedType)
                {
                    if (pThisEEType->RelatedParameterType->IsEquivalentTo(pOtherEEType->RelatedParameterType))
                    {
                        return pThisEEType->ParameterizedTypeShape == pOtherEEType->ParameterizedTypeShape;
                    }
                    return false;
                }
            }
            return false;
        }

        [Intrinsic]
        internal unsafe static extern MethodTable* Of<T>();

        internal unsafe IntPtr* GetVTableStartAddress()
        {
            byte* pResult;
            fixed (MethodTable* pThis = &this)
            {
                pResult = (byte*)pThis;
            }
            return (IntPtr*)(pResult + sizeof(MethodTable));
        }

        private unsafe static IntPtr FollowRelativePointer(int* pDist)
        {
            int dist = *pDist;
            return (IntPtr)((byte*)pDist + dist);
        }

        internal unsafe IntPtr GetSealedVirtualSlot(ushort slotNumber)
        {
            //Debug.Assert((RareFlags & EETypeRareFlags.HasSealedVTableEntriesFlag) != 0);
            fixed (MethodTable* pThis = &this)
            {
                if (IsDynamicType || !SupportsRelativePointers())
                {
                    uint cbSealedVirtualSlotsTypeOffset = GetFieldOffset(EETypeField.ETF_SealedVirtualSlots);
                    IntPtr* pSealedVirtualsSlotTable = *(IntPtr**)((byte*)pThis + cbSealedVirtualSlotsTypeOffset);
                    return pSealedVirtualsSlotTable[(int)slotNumber];
                }
                uint cbSealedVirtualSlotsTypeOffset2 = GetFieldOffset(EETypeField.ETF_SealedVirtualSlots);
                int* pSealedVirtualsSlotTable2 = (int*)(void*)FollowRelativePointer((int*)((byte*)pThis + cbSealedVirtualSlotsTypeOffset2));
                return FollowRelativePointer(pSealedVirtualsSlotTable2 + (int)slotNumber);
            }
        }

        public unsafe uint GetFieldOffset(EETypeField eField)
        {
            // First part of EEType consists of the fixed portion followed by the vtable.
            uint cbOffset = (uint)(sizeof(MethodTable) + (IntPtr.Size * _usNumVtableSlots));

            // Then we have the interface map.
            if (eField == EETypeField.ETF_InterfaceMap)
            {
                //Debug.Assert(NumInterfaces > 0);
                return cbOffset;
            }
            cbOffset += (uint)(sizeof(EEInterfaceInfo) * NumInterfaces);

            uint relativeOrFullPointerOffset = (IsDynamicType || !SupportsRelativePointers() ? (uint)IntPtr.Size : 4u);

            // Followed by the type manager indirection cell.
            if (eField == EETypeField.ETF_TypeManagerIndirection)
                return cbOffset;

            cbOffset += relativeOrFullPointerOffset;


            // Followed by writable data.
            if (eField == EETypeField.ETF_WritableData)
            {
                return cbOffset;
            }
            cbOffset += relativeOrFullPointerOffset;

            // Followed by the pointer to the finalizer method.
            if (eField == EETypeField.ETF_Finalizer)
            {
                //Debug.Assert(IsFinalizable);
                return cbOffset;
            }
            if (IsFinalizable)
                cbOffset += relativeOrFullPointerOffset;

            // Followed by the pointer to the optional fields.
            if (eField == EETypeField.ETF_OptionalFieldsPtr)
            {
                //Debug.Assert(HasOptionalFields);
                return cbOffset;
            }
            if (HasOptionalFields)
                cbOffset += relativeOrFullPointerOffset;

            // Followed by the pointer to the sealed virtual slots
            if (eField == EETypeField.ETF_SealedVirtualSlots)
                return cbOffset;

            EETypeRareFlags rareFlags = RareFlags;

            // in the case of sealed vtable entries on static types, we have a UInt sized relative pointer
            if ((rareFlags & EETypeRareFlags.HasSealedVTableEntriesFlag) != 0)
                cbOffset += relativeOrFullPointerOffset;

            if (eField == EETypeField.ETF_DynamicDispatchMap)
            {
                //Debug.Assert(IsDynamicType);
                return cbOffset;
            }
            if ((rareFlags & EETypeRareFlags.HasDynamicallyAllocatedDispatchMapFlag) != 0)
                cbOffset += (uint)IntPtr.Size;

            if (eField == EETypeField.ETF_GenericDefinition)
            {
                //Debug.Assert(IsGeneric);
                return cbOffset;
            }
            if (IsGeneric)
            {
                cbOffset += relativeOrFullPointerOffset;
            }

            if (eField == EETypeField.ETF_GenericComposition)
            {
                //Debug.Assert(IsGeneric);
                return cbOffset;
            }
            if (IsGeneric)
            {
                cbOffset += relativeOrFullPointerOffset;
            }

            if (eField == EETypeField.ETF_DynamicModule)
            {
                return cbOffset;
            }

            if ((rareFlags & EETypeRareFlags.HasDynamicModuleFlag) != 0)
                cbOffset += (uint)IntPtr.Size;

            if (eField == EETypeField.ETF_DynamicTemplateType)
            {
                //Debug.Assert(IsDynamicType);
                return cbOffset;
            }
            if (IsDynamicType)
                cbOffset += (uint)IntPtr.Size;

            if (eField == EETypeField.ETF_DynamicGcStatics)
            {
                //Debug.Assert((rareFlags & EETypeRareFlags.IsDynamicTypeWithGcStatics) != 0);
                return cbOffset;
            }
            if ((rareFlags & EETypeRareFlags.IsDynamicTypeWithGcStatics) != 0)
                cbOffset += (uint)IntPtr.Size;

            if (eField == EETypeField.ETF_DynamicNonGcStatics)
            {
                //Debug.Assert((rareFlags & EETypeRareFlags.IsDynamicTypeWithNonGcStatics) != 0);
                return cbOffset;
            }
            if ((rareFlags & EETypeRareFlags.IsDynamicTypeWithNonGcStatics) != 0)
                cbOffset += (uint)IntPtr.Size;

            if (eField == EETypeField.ETF_DynamicThreadStaticOffset)
            {
                //Debug.Assert((rareFlags & EETypeRareFlags.IsDynamicTypeWithThreadStatics) != 0);
                return cbOffset;
            }

            if ((rareFlags & EETypeRareFlags.IsDynamicTypeWithThreadStatics) != 0)
                cbOffset += 4;

            //Debug.Assert(false, "Unknown EEType field type");
            return 0;
        }

        public unsafe ref T GetField<T>(EETypeField eField)
        {
            return ref Unsafe.As<byte, T>(ref ((byte*)Unsafe.AsPointer(ref this))[GetFieldOffset(eField)]);
            //return ref Unsafe.AddByteOffset(ref Unsafe.As<EEType, T>(ref *pThis), (IntPtr)GetFieldOffset(eField));
        }

        public unsafe ref T GetField<T>(uint offset)
        {
            return ref Unsafe.As<byte, T>(ref ((byte*)Unsafe.AsPointer(ref this))[offset]);
        }
    }

    //	public IntPtr GetRuntimeException {
    //		get {
    //			unsafe {
    //				if (_cbSize >= sizeof(IntPtr) * 3) {
    //					return _getRuntimeException;
    //				}
    //				else {
    //					return IntPtr.Zero;
    //				}
    //			}
    //		}
    //	}
    //}
}