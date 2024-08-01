namespace Internal.Runtime
{
    internal enum EETypeElementType
    {
        // Primitive
        Unknown = 0x00,
        Void = 0x01,
        Boolean = 0x02,
        Char = 0x03,
        SByte = 0x04,
        Byte = 0x05,
        Int16 = 0x06,
        UInt16 = 0x07,
        Int32 = 0x08,
        UInt32 = 0x09,
        Int64 = 0x0A,
        UInt64 = 0x0B,
        IntPtr = 0x0C,
        UIntPtr = 0x0D,
        Single = 0x0E,
        Double = 0x0F,

        ValueType = 0x10,
        // Enum = 0x11, // EETypes store enums as their underlying type
        Nullable = 0x12,
        // Unused 0x13,

        Class = 0x14,
        Interface = 0x15,

        SystemArray = 0x16, // System.Array type

        Array = 0x17,
        SzArray = 0x18,
        ByRef = 0x19,
        Pointer = 0x1A,
    }

    //[StructLayout(LayoutKind.Sequential)]
    //internal struct DynamicModule {
    //	// Size field used to indicate the number of bytes of this structure that are defined in Runtime Known ways
    //	// This is used to drive versioning of this field
    //	private int _cbSize;

    //	// Pointer to interface dispatch resolver that works off of a type/slot pair
    //	// This is a function pointer with the following signature IntPtr()(IntPtr targetType, IntPtr interfaceType, ushort slot)
    //	private IntPtr _dynamicTypeSlotDispatchResolve;

    //	// Starting address for the the binary module corresponding to this dynamic module.
    //	private IntPtr _getRuntimeException;

    //	public IntPtr DynamicTypeSlotDispatchResolve {
    //		get {
    //			unsafe {
    //				if (_cbSize >= sizeof(IntPtr) * 2) {
    //					return _dynamicTypeSlotDispatchResolve;
    //				}
    //				else {
    //					return IntPtr.Zero;
    //				}
    //			}
    //		}
    //	}

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