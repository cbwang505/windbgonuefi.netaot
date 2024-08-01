using EfiSharp;
using System.Runtime.CompilerServices;
using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Internal.Runtime.CompilerHelpers
{
    public unsafe class StartupCodeHelpers
    {
#if EMBEDDEDGC
        [RuntimeImport("*", "__imp_GetCurrentThreadId")]
        public static extern int __imp_GetCurrentThreadId();
#else

        [RuntimeExport("__imp_GetCurrentThreadId")]
        public static int __imp_GetCurrentThreadId() => 0;
#endif
        [RuntimeExport("__CheckForDebuggerJustMyCode")]
        public static int __CheckForDebuggerJustMyCode() => 0;

        [RuntimeExport("__fail_fast")]
        static void FailFast() { while (true) ; }

        [RuntimeExport("memset")]
        public static unsafe void MemSet(byte* ptr, byte c, int count)
        {


            if (ptr == 0)
            {
                return;
            }

            /*for (byte* p = ptr; p < ptr + count; p++)
            {
                *p = (byte)c;
            }*/

             UefiApplication.SystemTable->BootServices->SetMem(ptr, count, c);

            return;
        }

        public static unsafe void memset(byte* ptr, int c, int count)
        {
            StartupCodeHelpers.MemSet(ptr, (byte)c, count);
            return;
        }
        [RuntimeExport("memcpy")]
        public static unsafe void MemCpy(byte* dest, byte* src, nuint count)
        {
            // for (ulong i = 0; i < count; i++) dest[i] = src[i];

            UefiApplication.SystemTable->BootServices->CopyMem(dest, src, count);
        }
       
        public static unsafe void memcpy(byte* dest, byte* src, ulong count)
        {
            StartupCodeHelpers.MemCpy(dest, src, (nuint)count);

        }
        [RuntimeExport("memmove")]
        public static unsafe void memmove(byte* dest, byte* src, ulong count)
        {
            StartupCodeHelpers.MemCpy(dest, src, (nuint)count);

        }
        public static unsafe void MemcpyPtr(IntPtr dest, IntPtr src, ulong count)
        {
            StartupCodeHelpers.MemCpy((byte*)dest, (byte*)src, (nuint)count);

        }
        [RuntimeExport("free")]
        public static unsafe ulong free(IntPtr ptr)
        {

            UefiApplication.SystemTable->BootServices->FreePool(ptr);
            return 0;
        }


        [RuntimeExport("RhpReversePInvoke2")]
        static void RhpReversePInvoke2(IntPtr frame) { }

        [RuntimeExport("RhpReversePInvokeReturn2")]
        static void RhpReversePInvokeReturn2(IntPtr frame) { }
#if EMBEDDEDGC
        [RuntimeImport("*", "RhpReversePInvoke")]
        public static extern void RhpReversePInvoke(IntPtr frame);

        [RuntimeImport("*", "RhpReversePInvokeReturn")]
        public static extern void RhpReversePInvokeReturn(IntPtr frame);

        [RuntimeImport("*", "RhpFallbackFailFast")]
        public static extern void RhpFallbackFailFast();
#else
        [RuntimeExport("RhpReversePInvoke")]
        static void RhpReversePInvoke(IntPtr frame) { }
        [RuntimeExport("RhpReversePInvokeReturn")]
        static void RhpReversePInvokeReturn(IntPtr frame) { }

        [RuntimeExport("RhpFallbackFailFast")]
        static void RhpFallbackFailFast() { while (true) ; }
#endif


        [RuntimeExport("RhpPInvoke")]
        static void RhpPinvoke(IntPtr frame) { }

        [RuntimeExport("RhpPInvokeReturn")]
        static void RhpPinvokeReturn(IntPtr frame) { }

        [RuntimeExport("RhpNewFast")]
        static unsafe object RhpNewFast(MethodTable* pEEType)
        {
            var size = pEEType->BaseSize;

            // Round to next power of 8
            if (size % 8 > 0)
                size = ((size / 8) + 1) * 8;

            var data = malloc(size);
            var obj = Unsafe.As<IntPtr, object>(ref data);
            MemSet((byte*)data, 0, (int)size);
            *(IntPtr*)data = (IntPtr)pEEType;

            return obj;
        }



        [RuntimeExport("__security_cookie")]
        static void __security_cookie()
        {
            Console.WriteLine("call __security_cookie");
            return;
        }
        [RuntimeExport("__security_check_cookie")]
        static void __security_check_cookie(IntPtr _StackCookie)
        {
            Console.WriteLine("call __security_check_cookie _StackCookie:=>" + _StackCookie.ToString());
            return;
        }

        [RuntimeExport("malloc")]
        public static unsafe nint malloc(ulong size)
        {
            IntPtr data;
            UefiApplication.SystemTable->BootServices->AllocatePool(EFI_MEMORY_TYPE.EfiLoaderData, size, out data);
            if (data == IntPtr.Zero)
            {
                Console.WriteLine("'malloc failed len =>" + size.ToString("x"));
            }
            nint obj = Unsafe.As<IntPtr, nint>(ref data);
            return obj;
        }

        [RuntimeExport("RhpNewArray")]
        internal static unsafe object RhpNewArray(MethodTable* pEEType, int length)
        {
            var size = pEEType->BaseSize + (ulong)length * pEEType->ComponentSize;

            // Round to next power of 8
            if (size % 8 > 0)
                size = ((size / 8) + 1) * 8;

            var data = malloc(size);
            var obj = Unsafe.As<IntPtr, object>(ref data);
            MemSet((byte*)data, 0, (int)size);
            *(IntPtr*)data = (IntPtr)pEEType;

            var b = (byte*)data;
            b += sizeof(IntPtr);
            MemCpy(b, (byte*)(&length), sizeof(int));

            return obj;
        }
#if EMBEDDEDGC
       
        [RuntimeImport("*", "RhpAssignRef")]
        public static extern void RhpAssignRef(ref object address, object obj);

        [RuntimeImport("*", "RhpByRefAssignRef")]
        public static extern unsafe void RhpByRefAssignRef(void** address, void* obj);

        [RuntimeImport("*", "RhpCheckedAssignRef")]
        public static extern unsafe  void RhpCheckedAssignRef(void** address, void* obj);
#else
        [RuntimeExport("RhpAssignRef")]
        static unsafe void RhpAssignRef(void** address, void* obj)
        {
            *address = obj;
        }
        [RuntimeExport("RhpByRefAssignRef")]
        static unsafe void RhpByRefAssignRef(void** address, void* obj)
        {
            *address = obj;
        }

        [RuntimeExport("RhpCheckedAssignRef")]
        static unsafe void RhpCheckedAssignRef(void** address, void* obj)
        {
            *address = obj;
        }
#endif


        [RuntimeExport("RhpStelemRef")]
        static unsafe void RhpStelemRef(Array array, int index, object obj)
        {
            fixed (int* n = &array._numComponents)
            {
                var ptr = (byte*)n;
                ptr += sizeof(void*);   // Array length is padded to 8 bytes on 64-bit
                ptr += index * array.m_pEEType->ComponentSize;  // Component size should always be 8, seeing as it's a pointer...
                var pp = (IntPtr*)ptr;
                *pp = Unsafe.As<object, IntPtr>(ref obj);
            }
        }

        public static void Move(Array array, IntPtr dest, int index, int count)
        {
            fixed (int* n = &array._numComponents)
            {
                var ptr = (byte*)n;
                ptr += sizeof(void*);   // Array length is padded to 8 bytes on 64-bit
                ptr += index * array.m_pEEType->ComponentSize;  // Component size should always be 8, seeing as it's a pointer...
                MemCpy((byte*)dest, ptr, count);
            }
        } 
        public static void From(IntPtr src, Array array, int index, int count)
        {
            fixed (int* n = &array._numComponents)
            {
                var ptr = (byte*)n;
                ptr += sizeof(void*);   // Array length is padded to 8 bytes on 64-bit
                ptr += index * array.m_pEEType->ComponentSize;  // Component size should always be 8, seeing as it's a pointer...
                MemCpy((byte*)ptr, (byte*)src,count);
            }
        }
        [RuntimeExport("RhBox")]
        public static unsafe object RhBox(MethodTable* pEEType, ref byte data)
        {
            ref byte dataAdjustedForNullable = ref data;

            // Can box non-ByRefLike value types only (which also implies no finalizers).
           // Debug.Assert(pEEType->IsValueType && !pEEType->IsByRefLike && !pEEType->IsFinalizable);

            // If we're boxing a Nullable<T> then either box the underlying T or return null (if the
            // nullable's value is empty).
            if (pEEType->IsNullable)
            {
                // The boolean which indicates whether the value is null comes first in the Nullable struct.
                if (data == 0)
                    return null;

                // Switch type we're going to box to the Nullable<T> target type and advance the data pointer
                // to the value embedded within the nullable.
                dataAdjustedForNullable = ref Unsafe.Add(ref data, pEEType->NullableValueOffset);
                pEEType = pEEType->NullableType;
            }

            object result;
#if FEATURE_64BIT_ALIGNMENT
            if (pEEType->RequiresAlign8)
            {
                result = InternalCalls.RhpNewFastMisalign(pEEType);
            }
            else
#endif // FEATURE_64BIT_ALIGNMENT
            {
                result = StartupCodeHelpers.RhpNewFast(pEEType);
            }

            // Copy the unboxed value type data into the new object.
            // Perform any write barriers necessary for embedded reference fields.
           // if (pEEType->HasGCPointers)
            if(false)
            {
                //StartupCodeHelpers.RhBulkMoveWithWriteBarrier(ref result.GetRawData(), ref dataAdjustedForNullable, pEEType->ValueTypeSize);
            }
            else
            {
                fixed (byte* pFields = &result.GetRawData())
                fixed (byte* pData = &dataAdjustedForNullable)
                    StartupCodeHelpers.memmove(pFields, pData, pEEType->ValueTypeSize);
            }

            return result;
        }

        private static bool IsPrimitiveElementType(EETypeElementType ElementType)
        {
            switch (ElementType)
            {
                case EETypeElementType.Byte:
                case EETypeElementType.SByte:
                case EETypeElementType.Int16:
                case EETypeElementType.UInt16:
                case EETypeElementType.Int32:
                case EETypeElementType.UInt32:
                case EETypeElementType.Int64:
                case EETypeElementType.UInt64:
                case EETypeElementType.IntPtr:
                case EETypeElementType.UIntPtr:
                    return true;
            }
            return false;
        }
        private static unsafe bool UnboxAnyTypeCompare(MethodTable* pEEType, MethodTable* ptrUnboxToEEType)
        {
            if (TypeCast.AreTypesEquivalent(pEEType, ptrUnboxToEEType))
                return true;
           
            if (pEEType->ElementType == ptrUnboxToEEType->ElementType)
            {
               
                // Enum's and primitive types should pass the UnboxAny exception cases
                // if they have an exactly matching cor element type.
                switch (ptrUnboxToEEType->ElementType)
                {
                    case EETypeElementType.Byte:
                    case EETypeElementType.SByte:
                    case EETypeElementType.Int16:
                    case EETypeElementType.UInt16:
                    case EETypeElementType.Int32:
                    case EETypeElementType.UInt32:
                    case EETypeElementType.Int64:
                    case EETypeElementType.UInt64:
                    case EETypeElementType.IntPtr:
                    case EETypeElementType.UIntPtr:
                        return true;
                }
            }

            if (IsPrimitiveElementType(pEEType->ElementType) && IsPrimitiveElementType(ptrUnboxToEEType->ElementType))
            {
                return true;
            }
            return false;
        }
        [RuntimeExport("RhUnbox2")]
        public static unsafe ref byte RhUnbox2(MethodTable* pUnboxToEEType, object obj)
        {
            if ((obj == null) || !UnboxAnyTypeCompare(obj.GetMethodTable(), pUnboxToEEType))
            //if (obj == null)
            {
                ExceptionIDs exID = obj == null ? ExceptionIDs.NullReference : ExceptionIDs.InvalidCast;
                throw pUnboxToEEType->GetClasslibException(exID);
            }
            return ref obj.GetRawData();
        }


        [RuntimeExport("RhTypeCast_IsInstanceOfClass")]
        public static unsafe object RhTypeCast_IsInstanceOfClass(MethodTable* pTargetType, object obj)
        {
            if (obj == null)
                return null;

            if (pTargetType == obj.m_pEEType)
                return obj;

            var bt = obj.m_pEEType->RawBaseType;

            while (true)
            {
                if (bt == null)
                    return null;

                if (pTargetType == bt)
                    return obj;

                bt = bt->RawBaseType;
            }
        }

        public static void InitializeModules(IntPtr Modules)
        {
            for (int i = 0; ; i++)
            {
                if (((IntPtr*)Modules)[i].Equals(IntPtr.Zero))
                    break;

                var header = (ReadyToRunHeader*)((IntPtr*)Modules)[i];
                var sections = (ModuleInfoRow*)(header + 1);

                if (header->Signature != ReadyToRunHeaderConstants.Signature)
                {
                    FailFast();
                }

                for (int k = 0; k < header->NumberOfSections; k++)
                {
                    if (sections[k].SectionId == ReadyToRunSectionType.GCStaticRegion)
                        InitializeStatics(sections[k].Start, sections[k].End);

                    if (sections[k].SectionId == ReadyToRunSectionType.EagerCctor)
                        RunEagerClassConstructors(sections[k].Start, sections[k].End);
                }
            }

            DateTime.s_daysToMonth365 = new int[]{
                0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
            DateTime.s_daysToMonth366 = new int[]{
                0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };
        }

        public static void InitializeModulesStaticsAndClassConstructors(IntPtr Modules)
        {
            ReadyToRunHeader* pReadyToRunHeader = (ReadyToRunHeader*)Modules;
            if (pReadyToRunHeader->Signature != ReadyToRunHeaderConstants.Signature)
            {
                Console.WriteLine("pReadyToRunHeader->Signature == ReadyToRunHeaderConstants::Signature");
                return ;
            }

            // Only the current major version is supported currently

            if (pReadyToRunHeader->MajorVersion != ReadyToRunHeaderConstants.CurrentMajorVersion)
            {
                Console.WriteLine("pReadyToRunHeader->MajorVersion == ReadyToRunHeaderConstants::CurrentMajorVersion");
                return ;
            }

            ReadyToRunHeader* header = (ReadyToRunHeader*)(Modules);
            ModuleInfoRow* sections = (ModuleInfoRow*)(header + 1);
            if (header->Signature != ReadyToRunHeaderConstants.Signature)
            {
                Console.WriteLine("header->Signature != ReadyToRunHeaderConstants.Signature");
                return;
            }

            for (int k = 0; k < header->NumberOfSections; k++)
            {
                if (sections[k].SectionId == ReadyToRunSectionType.GCStaticRegion)
                    InitializeStatics(sections[k].Start, sections[k].End);

                if (sections[k].SectionId == ReadyToRunSectionType.EagerCctor)
                    RunEagerClassConstructors(sections[k].Start, sections[k].End);
            }


            DateTime.s_daysToMonth365 = new int[]{
                0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
            DateTime.s_daysToMonth366 = new int[]{
                0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };
        }

        public static IntPtr GetModuleSection(IntPtr Modules, ReadyToRunSectionType sectionId)
        {
            ReadyToRunHeader* pReadyToRunHeader = (ReadyToRunHeader*)Modules;
            IntPtr ret = IntPtr.Zero;
            // Sanity check the signature magic

            if (pReadyToRunHeader->Signature != ReadyToRunHeaderConstants.Signature)
            {
                Console.WriteLine("pReadyToRunHeader->Signature == ReadyToRunHeaderConstants::Signature");
                return ret;
            }

            // Only the current major version is supported currently

            if (pReadyToRunHeader->MajorVersion != ReadyToRunHeaderConstants.CurrentMajorVersion)
            {
                Console.WriteLine("pReadyToRunHeader->MajorVersion == ReadyToRunHeaderConstants::CurrentMajorVersion");
                return ret;
            }

            ReadyToRunHeader* header = (ReadyToRunHeader*)(Modules);
            ModuleInfoRow* sections = (ModuleInfoRow*)(header + 1);

            if (header->Signature != ReadyToRunHeaderConstants.Signature)
            {
                Console.WriteLine("header->Signature != ReadyToRunHeaderConstants.Signature");
                return ret;
            }

            for (int k = 0; k < header->NumberOfSections; k++)
            {
                if (sections[k].SectionId == sectionId)
                {

                    return sections[k].Start;
                }
            }
            return ret;
        }
        public static IntPtr GetModuleSectionWithLength(IntPtr Modules, ReadyToRunSectionType sectionId,ref IntPtr len)
        {
            ReadyToRunHeader* pReadyToRunHeader = (ReadyToRunHeader*)Modules;
            IntPtr ret = IntPtr.Zero;
            // Sanity check the signature magic

            if (pReadyToRunHeader->Signature != ReadyToRunHeaderConstants.Signature)
            {
                Console.WriteLine("pReadyToRunHeader->Signature == ReadyToRunHeaderConstants::Signature");
                return ret;
            }

            // Only the current major version is supported currently

            if (pReadyToRunHeader->MajorVersion != ReadyToRunHeaderConstants.CurrentMajorVersion)
            {
                Console.WriteLine("pReadyToRunHeader->MajorVersion == ReadyToRunHeaderConstants::CurrentMajorVersion");
                return ret;
            }

            ReadyToRunHeader* header = (ReadyToRunHeader*)(Modules);
            ModuleInfoRow* sections = (ModuleInfoRow*)(header + 1);

            if (header->Signature != ReadyToRunHeaderConstants.Signature)
            {
                Console.WriteLine("header->Signature != ReadyToRunHeaderConstants.Signature");
                return ret;
            }

            for (int k = 0; k < header->NumberOfSections; k++)
            {
                if (sections[k].SectionId == sectionId)
                {
                    len = sections[k].End - sections[k].Start;
                    return sections[k].Start;
                }
            }
            return ret;
        }
        private static unsafe void RunEagerClassConstructors(IntPtr cctorTableStart, IntPtr cctorTableEnd)
        {
            for (IntPtr* tab = (IntPtr*)cctorTableStart; tab < (IntPtr*)cctorTableEnd; tab++)
            {
                ((delegate*<void>)(*tab))();
            }
        }

        static unsafe void InitializeStatics(IntPtr rgnStart, IntPtr rgnEnd)
        {
            for (IntPtr* block = (IntPtr*)rgnStart; block < (IntPtr*)rgnEnd; block++)
            {
                var pBlock = (IntPtr*)*block;
                var blockAddr = (long)(*pBlock);

                if ((blockAddr & GCStaticRegionConstants.Uninitialized) == GCStaticRegionConstants.Uninitialized)
                {
                    var obj = RhpNewFast((MethodTable*)(blockAddr & ~GCStaticRegionConstants.Mask));

                    if ((blockAddr & GCStaticRegionConstants.HasPreInitializedData) == GCStaticRegionConstants.HasPreInitializedData)
                    {
                        IntPtr pPreInitDataAddr = *(pBlock + 1);
                        fixed (byte* p = &obj.GetRawData())
                        {
                            MemCpy(p, (byte*)pPreInitDataAddr, obj.GetRawDataSize());
                        }
                    }

                    var handle = malloc((ulong)sizeof(IntPtr));
                    *(IntPtr*)handle = Unsafe.As<object, IntPtr>(ref obj);
                    *pBlock = handle;
                }
            }
        }


       
    }
    }
