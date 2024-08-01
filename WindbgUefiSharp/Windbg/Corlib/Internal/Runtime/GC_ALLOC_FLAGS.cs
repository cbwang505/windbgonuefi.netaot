namespace Internal.Runtime
{
    internal enum GC_ALLOC_FLAGS
    {
        GC_ALLOC_NO_FLAGS = 0,
        GC_ALLOC_ZEROING_OPTIONAL = 0x10,
        GC_ALLOC_PINNED_OBJECT_HEAP = 0x40
    }
}
