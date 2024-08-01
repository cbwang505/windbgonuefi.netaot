using Internal.Runtime;

namespace System.Runtime
{
    internal unsafe struct DispatchCellInfo
    {
        public DispatchCellType CellType;

        public MethodTable* InterfaceType;

        public ushort InterfaceSlot;

        public byte HasCache;

        public uint MetadataToken;

        public uint VTableOffset;
    };

    internal unsafe struct InterfaceDispatchCell
    {
        public IntPtr m_pStub;
        public IntPtr m_pCache;
    };

}
