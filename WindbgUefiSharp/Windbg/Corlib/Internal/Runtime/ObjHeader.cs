using System;
using System.Runtime.InteropServices;

namespace Internal.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ObjHeader 
    {
        // Contents of the object header
        private IntPtr _objHeaderContents;
    }

    //// Wrapper around EEType pointers that may be indirected through the IAT if their low bit is set.
    //[StructLayout(LayoutKind.Sequential)]
    //internal unsafe struct EETypeRef {
    //	private byte* _value;

    //	public EEType* Value {
    //		get {
    //			if (((int)_value & IndirectionConstants.IndirectionCellPointer) == 0)
    //				return (EEType*)_value;
    //			return *(EEType**)(_value - IndirectionConstants.IndirectionCellPointer);
    //		}
    //	}
    //}

    //// Wrapper around pointers
    //[StructLayout(LayoutKind.Sequential)]
    //internal readonly struct Pointer {
    //	private readonly IntPtr _value;

    //	public IntPtr Value {
    //		get {
    //			return _value;
    //		}
    //	}
    //}

    //// Wrapper around pointers
    //[StructLayout(LayoutKind.Sequential)]
    //internal unsafe readonly struct Pointer<T> where T : unmanaged {
    //	private readonly T* _value;

    //	public T* Value {
    //		get {
    //			return _value;
    //		}
    //	}
    //}

    //// Wrapper around pointers that might be indirected through IAT
    //[StructLayout(LayoutKind.Sequential)]
    //internal unsafe readonly struct IatAwarePointer<T> where T : unmanaged {
    //	private readonly T* _value;

    //	public T* Value {
    //		get {
    //			if (((int)_value & IndirectionConstants.IndirectionCellPointer) == 0)
    //				return _value;
    //			return *(T**)((byte*)_value - IndirectionConstants.IndirectionCellPointer);
    //		}
    //	}
    //}

    //// Wrapper around relative pointers
    //[StructLayout(LayoutKind.Sequential)]
    //internal readonly struct RelativePointer {
    //	private readonly int _value;

    //	public unsafe IntPtr Value {
    //		get {
    //			return (IntPtr)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + _value);
    //		}
    //	}
    //}

    //// Wrapper around relative pointers
    //[StructLayout(LayoutKind.Sequential)]
    //internal unsafe readonly struct RelativePointer<T> where T : unmanaged {
    //	private readonly int _value;

    //	public T* Value {
    //		get {
    //			return (T*)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + _value);
    //		}
    //	}
    //}

    //// Wrapper around relative pointers that might be indirected through IAT
    //[StructLayout(LayoutKind.Sequential)]
    //internal unsafe readonly struct IatAwareRelativePointer<T> where T : unmanaged {
    //	private readonly int _value;

    //	public T* Value {
    //		get {
    //			if ((_value & IndirectionConstants.IndirectionCellPointer) == 0) {
    //				return (T*)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + _value);
    //			}
    //			else {
    //				return *(T**)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + (_value & ~IndirectionConstants.IndirectionCellPointer));
    //			}
    //		}
    //	}
    //}

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