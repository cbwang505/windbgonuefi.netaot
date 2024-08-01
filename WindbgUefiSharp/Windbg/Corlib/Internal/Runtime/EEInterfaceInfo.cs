using System.Runtime.InteropServices;

namespace Internal.Runtime
{
    internal struct EEInterfaceInfo
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct InterfaceTypeUnion
        {
            [FieldOffset(0)]
            public unsafe MethodTable* _pInterfaceEEType;

            [FieldOffset(0)]
            public unsafe MethodTable** _ppInterfaceEETypeViaIAT;
        }

        private InterfaceTypeUnion _interfaceType;

        internal unsafe MethodTable* InterfaceType
        {
            get
            {
                if (((uint)(int)_interfaceType._pInterfaceEEType & (true ? 1u : 0u)) != 0)
                {
                    MethodTable** ppInterfaceEETypeViaIAT = (MethodTable**)((ulong)_interfaceType._ppInterfaceEETypeViaIAT - 1uL);
                    return *ppInterfaceEETypeViaIAT;
                }
                return _interfaceType._pInterfaceEEType;
            }
        }
        internal unsafe MethodTable* InterfaceEntryType
        {
            get
            {
                
                return _interfaceType._pInterfaceEEType;
            }
        }
    }
}
/*

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct EEInterfaceInfo {
        [StructLayout(LayoutKind.Explicit)]
        private unsafe struct InterfaceTypeUnion {
            [FieldOffset(0)]
            public EEType* _pInterfaceEEType;
            [FieldOffset(0)]
            public EEType** _ppInterfaceEETypeViaIAT;
        }

        private InterfaceTypeUnion _interfaceType;

        internal EEType* InterfaceType {
            get {
                if ((unchecked((uint)_interfaceType._pInterfaceEEType) & IndirectionConstants.IndirectionCellPointer) != 0) {
#if TARGET_64BIT
                    EEType** ppInterfaceEETypeViaIAT = (EEType**)(((ulong)_interfaceType._ppInterfaceEETypeViaIAT) - IndirectionConstants.IndirectionCellPointer);
#else
                    EEType** ppInterfaceEETypeViaIAT = (EEType**)(((uint)_interfaceType._ppInterfaceEETypeViaIAT) - IndirectionConstants.IndirectionCellPointer);
#endif
                    return *ppInterfaceEETypeViaIAT;
                }

                return _interfaceType._pInterfaceEEType;
            }
#if TYPE_LOADER_IMPLEMENTATION
            set
            {
                _interfaceType._pInterfaceEEType = value;
            }
#endif
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct DispatchMap {
        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct DispatchMapEntry {
            internal ushort _usInterfaceIndex;
            internal ushort _usInterfaceMethodSlot;
            internal ushort _usImplMethodSlot;
        }

        private uint _entryCount;
        private DispatchMapEntry _dispatchMap;  at least one entry if any interfaces defined

        public bool IsEmpty {
            get {
                return _entryCount == 0;
            }
        }

        public uint NumEntries {
            get {
                return _entryCount;
            }
#if TYPE_LOADER_IMPLEMENTATION
            set
            {
                _entryCount = value;
            }
#endif
        }

        public int Size {
            get {
                return sizeof(uint) + sizeof(DispatchMapEntry) * (int)_entryCount;
            }
        }

        public DispatchMapEntry* this[int index] {
            get {
                fixed (DispatchMap* pThis = &this)
                    return (DispatchMapEntry*)((byte*)pThis + sizeof(uint) + (sizeof(DispatchMapEntry) * index));
            }
        }
    }
*/