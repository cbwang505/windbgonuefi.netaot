using System;

namespace Internal.Runtime
{
    [Flags]
    internal enum EETypeRareFlags
    {
        RequiresAlign8Flag = 1,
        HasCctorFlag = 0x20,
        HasDynamicallyAllocatedDispatchMapFlag = 0x80,
        IsHFAFlag = 0x100,
        HasSealedVTableEntriesFlag = 0x200,
        IsDynamicTypeWithGcStatics = 0x400,
        IsDynamicTypeWithNonGcStatics = 0x800,
        IsDynamicTypeWithThreadStatics = 0x1000,
        HasDynamicModuleFlag = 0x2000,
        IsAbstractClassFlag = 0x4000,
        IsByRefLikeFlag = 0x8000
    }
}
