using System;

namespace System.Runtime.CompilerServices
{
    //Edited to follow layout of ILCompiler's Internal/IL/Stubs/UnsafeIntrinsics.cs [Namespace] in Common/TypeSystem/IL/Stubs [Path]
    [CLSCompliant(false)]
    [ReflectionBlocked]
    public static unsafe class Unsafe
    {
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void* AsPointer<T>(ref T value);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern int SizeOf<T>();

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern T As<T>(object value) where T : class;

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref TTo As<TFrom, TTo>(ref TFrom source);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, int elementOffset)
            => ref AddByteOffset(ref source, (IntPtr)(elementOffset * (nint)SizeOf<T>()));
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, IntPtr elementOffset)
            => ref AddByteOffset(ref source, (IntPtr)((nint)elementOffset * (nint)SizeOf<T>()));
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Add<T>(void* source, int elementOffset)
            => (byte *)source + (elementOffset * (nint)SizeOf<T>());
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddByteOffset<T>(ref T source, nuint byteOffset)
            => ref AddByteOffset(ref source, (IntPtr)(void*)byteOffset);
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern bool AreSame<T>(ref T left, ref T right);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern bool IsAddressGreaterThan<T>(ref T left, ref T right);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern bool IsAddressLessThan<T>(ref T left, ref T right);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
        {
            for (uint i = 0; i < byteCount; i++)
                AddByteOffset(ref startAddress, i) = value;
        }

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(void* source)
            => As<byte, T>(ref *(byte*)source);
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(ref byte source)
            => As<byte, T>(ref source);
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(void* destination, T value)
            => As<byte, T>(ref *(byte*)destination) = value;
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(ref byte destination, T value)
            => As<byte, T>(ref destination) = value;
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref T AddByteOffset<T>(ref T source, IntPtr byteOffset);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(void* source)
            => As<byte, T>(ref *(byte*)source);
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(ref byte source)
            => As<byte, T>(ref source);
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(void* destination, T value)
            => As<byte, T>(ref *(byte*)destination) = value;
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(ref byte destination, T value)
            => As<byte, T>(ref destination) = value;
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(void* source)
            => ref As<byte, T>(ref *(byte*)source);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref T AsRef<T>(in T source);

        //[Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(IntPtr pointer)
            => ref AsRef<T>((void*)pointer);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern IntPtr ByteOffset<T>(ref T origin, ref T target);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T NullRef<T>()
            => ref AsRef<T>(null);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullRef<T>(ref T source)
            => AsPointer(ref source) == null;
        
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void SkipInit<T>(out T value);
    }
}