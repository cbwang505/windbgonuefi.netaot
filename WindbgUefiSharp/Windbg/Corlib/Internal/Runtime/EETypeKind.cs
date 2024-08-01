namespace Internal.Runtime
{
    internal enum EETypeKind : ushort
    {
        /// <summary>
        /// Represents a standard ECMA type
        /// </summary>
        CanonicalEEType = 0x0000,

        /// <summary>
        /// Represents a type cloned from another EEType
        /// </summary>
        ClonedEEType = 0x0001,

        /// <summary>
        /// Represents a parameterized type. For example a single dimensional array or pointer type
        /// </summary>
        ParameterizedEEType = 0x0002,

        /// <summary>
        /// Represents an uninstantiated generic type definition
        /// </summary>
        GenericTypeDefEEType = 0x0003,
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