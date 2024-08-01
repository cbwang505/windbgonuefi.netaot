using System;

namespace Internal.Runtime
{
    [Flags]
    internal enum EETypeFlags : ushort
    {
        /// <summary>
        /// There are four kinds of EETypes, defined in <c>Kinds</c>.
        /// </summary>
        EETypeKindMask = 0x0003,

        /// <summary>
        /// This flag is set when m_RelatedType is in a different module.  In that case, _pRelatedType
        /// actually points to an IAT slot in this module, which then points to the desired EEType in the
        /// other module.  In other words, there is an extra indirection through m_RelatedType to get to 
        /// the related type in the other module.  When this flag is set, it is expected that you use the 
        /// "_ppXxxxViaIAT" member of the RelatedTypeUnion for the particular related type you're 
        /// accessing.
        /// </summary>
        RelatedTypeViaIATFlag = 0x0004,

        /// <summary>
        /// This type was dynamically allocated at runtime.
        /// </summary>
        IsDynamicTypeFlag = 0x0008,

        /// <summary>
        /// This EEType represents a type which requires finalization.
        /// </summary>
        HasFinalizerFlag = 0x0010,

        /// <summary>
        /// This type contain GC pointers.
        /// </summary>
        HasPointersFlag = 0x0020,

        /// <summary>
        /// Type implements ICastable to allow dynamic resolution of interface casts.
        /// </summary>
        ICastableFlag = 0x0040,

        /// <summary>
        /// This type is generic and one or more of its type parameters is co- or contra-variant. This
        /// only applies to interface and delegate types.
        /// </summary>
        GenericVarianceFlag = 0x0080,

        /// <summary>
        /// This type has optional fields present.
        /// </summary>
        OptionalFieldsFlag = 0x0100,

        // Unused = 0x0200,

        /// <summary>
        /// This type is generic.
        /// </summary>
        IsGenericFlag = 0x0400,

        /// <summary>
        /// We are storing a EETypeElementType in the upper bits for unboxing enums.
        /// </summary>
        ElementTypeMask = 0xf800,
        ElementTypeShift = 11,

        /// <summary>
        /// Single mark to check TypeKind and two flags. When non-zero, casting is more complicated.
        /// </summary>
        ComplexCastingMask = EETypeKindMask | RelatedTypeViaIATFlag | GenericVarianceFlag
    };
}