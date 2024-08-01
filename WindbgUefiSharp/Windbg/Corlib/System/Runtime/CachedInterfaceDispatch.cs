using Internal.Runtime;
using System.Runtime.CompilerServices;

namespace System.Runtime
{
    internal static class CachedInterfaceDispatch
    {
        //
        // Keep these in sync with the managed copy in src\Common\src\Internal\Runtime\InterfaceCachePointerType.cs
        //
        enum DispatchCellInfoFlags
        {
            // The low 2 bits of the m_pCache pointer are treated specially so that we can avoid the need for
            // extra fields on this type.
            // OR if the m_pCache value is less than 0x1000 then this it is a vtable offset and should be used as such
            IDC_CachePointerIsInterfaceRelativePointer = 0x3,
            IDC_CachePointerIsIndirectedInterfaceRelativePointer = 0x2,
            IDC_CachePointerIsInterfacePointerOrMetadataToken = 0x1, // Metadata token is a 30 bit number in this case.
            // Tokens are required to have at least one of their upper 20 bits set
            // But they are not required by this part of the system to follow any specific
            // token format
            IDC_CachePointerPointsAtCache = 0x0,
            IDC_CachePointerMask = 0x3,
            IDC_CachePointerMaskShift = 0x2,
            IDC_MaxVTableOffsetPlusOne = 0x1000,
        };



        [RuntimeExport("RhpGetDispatchCellInfo")]
        private unsafe static void RhpGetDispatchCellInfo(InterfaceDispatchCell* pCell, out DispatchCellInfo newCellInfo)
        {


            newCellInfo = GetDispatchCellInfo(pCell);

          
           
            return;
        }

        private unsafe static DispatchCellInfo GetDispatchCellInfo(InterfaceDispatchCell* pCell)
        {
            // Capture m_pCache into a local for safe access (this is a volatile read of a value that may be
            // modified on another thread while this function is executing.)
            ulong cachem_pStub = (ulong)pCell->m_pStub;
            ulong cachePointerValue = (ulong)pCell->m_pCache;
            DispatchCellInfo cellInfo = new DispatchCellInfo();
           
            if ((cachePointerValue < (ulong)DispatchCellInfoFlags.IDC_MaxVTableOffsetPlusOne) && ((cachePointerValue & (ulong)DispatchCellInfoFlags.IDC_CachePointerMask) == (ulong)DispatchCellInfoFlags.IDC_CachePointerPointsAtCache))
            {
                cellInfo.VTableOffset = (uint)cachePointerValue;
                cellInfo.CellType = DispatchCellType.VTableOffset;
                cellInfo.HasCache = 1;
                return cellInfo;
            }

            /*// If there is a real cache pointer, grab the data from there.
            if ((cachePointerValue & DispatchCellInfoFlags.IDC_CachePointerMask) == DispatchCellInfoFlags.IDC_CachePointerPointsAtCache)
            {
                return ((InterfaceDispatchCacheHeader*)cachePointerValue)->GetDispatchCellInfo();
            }*/

            // Otherwise, walk to cell with Flags and Slot field

            // The slot number/flags for a dispatch cell is encoded once per run of DispatchCells
            // The run is terminated by having an dispatch cell with a null stub pointer.
            InterfaceDispatchCell* currentCell = pCell;
            while (((IntPtr)currentCell->m_pStub) != IntPtr.Zero)
            {
                currentCell = currentCell + 1;
            }
            IntPtr cachePointerValueFlags = currentCell->m_pCache;

            DispatchCellType cellType = (DispatchCellType)(cachePointerValueFlags >> 16);
            cellInfo.CellType = cellType;
          
            if (cellType == DispatchCellType.InterfaceAndSlot)
            {
                cellInfo.InterfaceSlot = (ushort)cachePointerValueFlags;

                switch ((DispatchCellInfoFlags)((ulong)cachePointerValue & (ulong)DispatchCellInfoFlags.IDC_CachePointerMask))
                {
                    case DispatchCellInfoFlags.IDC_CachePointerIsInterfacePointerOrMetadataToken:
                        cellInfo.InterfaceType = (MethodTable*)(cachePointerValue & ~((ulong)DispatchCellInfoFlags.IDC_CachePointerMask));
                        break;

                    case DispatchCellInfoFlags.IDC_CachePointerIsInterfaceRelativePointer:
                    case DispatchCellInfoFlags.IDC_CachePointerIsIndirectedInterfaceRelativePointer:
                        {
                            ulong interfacePointerValue = (ulong)((IntPtr)pCell + IntPtr.Size + (int)cachePointerValue);
                            interfacePointerValue &= ~(ulong)DispatchCellInfoFlags.IDC_CachePointerMask;
                            if ((cachePointerValue & (ulong)DispatchCellInfoFlags.IDC_CachePointerMask) == (ulong)DispatchCellInfoFlags.IDC_CachePointerIsInterfaceRelativePointer)
                            {
                                cellInfo.InterfaceType = (MethodTable*)interfacePointerValue;
                            }
                            else
                            {
                                cellInfo.InterfaceType = *(MethodTable**)interfacePointerValue;
                            }
                        }
                        break;
                }
            }
            else
            {
                cellInfo.MetadataToken = (UInt32)(cachePointerValue >> (int)DispatchCellInfoFlags.IDC_CachePointerMaskShift);
            }

            return cellInfo;
        }


        [RuntimeExport("RhpCidResolve")]
        private unsafe static IntPtr RhpCidResolve(IntPtr rcxthis, InterfaceDispatchCell* pCell)
        {
        
            // IntPtr locationOfThisPointer = callerTransitionBlockParam + TransitionBlock.GetThisOffset();
            IntPtr locationOfThisPointer = (IntPtr)rcxthis;
            object pObject = Unsafe.As<IntPtr, object>(ref locationOfThisPointer);
            return RhpCidResolve_Worker(pObject, pCell);
        }

        private unsafe static IntPtr RhpCidResolve_Worker(object pObject, InterfaceDispatchCell* pCell)
        {
            RhpGetDispatchCellInfo(pCell, out var cellInfo);
            IntPtr pTargetCode = RhResolveDispatchWorker(pObject, (void*)pCell, ref cellInfo);
           
            

            if (pTargetCode != IntPtr.Zero)
            {
                /*if (!pObject.GetMethodTable()->IsIDynamicInterfaceCastable)
                {
                    return InternalCalls.RhpUpdateDispatchCellCache(pCell, pTargetCode, pObject.GetMethodTable(), ref cellInfo);
                }*/
                return pTargetCode;
            }

            IntPtr pCellPtr = (IntPtr)pCell;
            Console.WriteLine("RhpCidResolve_Worker failed pTargetCode:=>" + ((IntPtr)pTargetCode).ToString("x")+ ",pCellPtr:=>"+ pCellPtr.ToString("x"));

            System.Diagnostics.Debug.Halt(true);
            //EH.FallbackFailFast(RhFailFastReason.InternalError, null);
            return IntPtr.Zero;
        }

        /*[RuntimeExport("RhpResolveInterfaceMethod")]
        private unsafe static IntPtr RhpResolveInterfaceMethod(object pObject, InterfaceDispatchCell* pCell)
        {
            if (pObject == null)
            {
                return IntPtr.Zero;
            }
            MethodTable* pInstanceType = pObject.GetMethodTable();
            IntPtr pTargetCode = InternalCalls.RhpSearchDispatchCellCache(pCell, pInstanceType);
            if (pTargetCode == IntPtr.Zero)
            {
                pTargetCode = RhpCidResolve_Worker(pObject, pCell);
            }
            return pTargetCode;
        }*/

        private unsafe static IntPtr RhResolveDispatchWorker(object pObject, void* cell, ref DispatchCellInfo cellInfo)
        {
            MethodTable* pInstanceType = pObject.GetMethodTable();
            if (cellInfo.CellType == DispatchCellType.InterfaceAndSlot)
            {
                MethodTable* pResolvingInstanceType = pInstanceType;
              

                IntPtr pTargetCode = DispatchResolve.FindInterfaceMethodImplementationTarget(pResolvingInstanceType, cellInfo.InterfaceType, cellInfo.InterfaceSlot, null);
                /*if (pTargetCode == IntPtr.Zero && pInstanceType->IsIDynamicInterfaceCastable)
                {
                    pTargetCode = ((delegate*<object, MethodTable*, ushort, IntPtr>)(void*)pInstanceType->GetClasslibFunction(ClassLibFunctionId.IDynamicCastableGetInterfaceImplementation))(pObject, cellInfo.InterfaceType.ToPointer(), cellInfo.InterfaceSlot);
                }*/
                return pTargetCode;
            }
            if (cellInfo.CellType == DispatchCellType.VTableOffset)
            {
                return *(IntPtr*)((byte*)pInstanceType + cellInfo.VTableOffset);
            }

            Console.WriteLine("RhResolveDispatchWorker failed cellInfo.CellType:=>" +
                              (cellInfo.CellType).ToString("x"));
            //EH.FallbackFailFast(RhFailFastReason.InternalError, null);
            return IntPtr.Zero;
        }


        /*
        [RuntimeExport("RhResolveDispatch")]
        private unsafe static IntPtr RhResolveDispatch(object pObject, EETypePtr interfaceType, ushort slot)
        {
            DispatchCellInfo cellInfo = default(DispatchCellInfo);
            cellInfo.CellType = DispatchCellType.InterfaceAndSlot;
            cellInfo.InterfaceType = interfaceType;
            cellInfo.InterfaceSlot = slot;
            return RhResolveDispatchWorker(pObject, null, ref cellInfo);
        }

        [RuntimeExport("RhResolveDispatchOnType")]
        private unsafe static IntPtr RhResolveDispatchOnType(EETypePtr instanceType, EETypePtr interfaceType, ushort slot, EETypePtr* pGenericContext)
        {
            MethodTable* pInstanceType = instanceType.ToPointer();
            MethodTable* pInterfaceType = interfaceType.ToPointer();
            return DispatchResolve.FindInterfaceMethodImplementationTarget(pInstanceType, pInterfaceType, slot, (MethodTable**)pGenericContext);
        }

     
      */
    }

}
