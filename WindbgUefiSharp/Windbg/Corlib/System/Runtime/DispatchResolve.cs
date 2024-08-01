using Internal.Runtime;
using System.Diagnostics;

namespace System.Runtime
{

    //  * TODO: URGENT: Dispatch resolve simply doesn't let us compile, I don't know why yet
    internal static class DispatchResolve
    {
        public unsafe static IntPtr FindInterfaceMethodImplementationTarget(MethodTable* pTgtType, MethodTable* pItfType, ushort itfSlotNumber, MethodTable** ppGenericContext)
        {
            /*DynamicModule* dynamicModule = pTgtType->DynamicModule;
            if (dynamicModule != null)
            {
                delegate*<MethodTable*, MethodTable*, ushort, IntPtr> resolver = dynamicModule->DynamicTypeSlotDispatchResolve;
                if (resolver != (delegate*<MethodTable*, MethodTable*, ushort, IntPtr>)null)
                {
                    return resolver(pTgtType, pItfType, itfSlotNumber);
                }
            }*/
            MethodTable* pCur = pTgtType;
            if (pItfType->IsCloned)
            {
                pItfType = pItfType->CanonicalEEType;
            }

           

            bool fDoDefaultImplementationLookup = false;
            ushort implSlotNumber = default(ushort);
        again:
            while (pCur != null)
            {
                
                if (FindImplSlotForCurrentType(
                        pCur, pItfType, itfSlotNumber, fDoDefaultImplementationLookup, &implSlotNumber, ppGenericContext))
                {

                    if (implSlotNumber < pCur->NumVtableSlots)
                    {
                        return pTgtType->GetVTableStartAddress()[(int)implSlotNumber];
                    }

                    return implSlotNumber switch
                    {
                        ushort.MaxValue => throw pTgtType->GetClasslibException(ExceptionIDs.EntrypointNotFound),
                        65534 => throw pTgtType->GetClasslibException(ExceptionIDs.AmbiguousImplementation),
                        _ => pCur->GetSealedVirtualSlot((ushort)(implSlotNumber - pCur->NumVtableSlots)),
                    };
                    /*IntPtr targetMethod;
                    if (implSlotNumber < pCur->NumVtableSlots)
                    {
                        // true virtual - need to get the slot from the target type in case it got overridden
                        targetMethod = pTgtType->GetVTableStartAddress()[implSlotNumber];
                    }
                    else if (implSlotNumber == SpecialDispatchMapSlot.Reabstraction)
                    {
                        throw pTgtType->GetClasslibException(ExceptionIDs.EntrypointNotFound);
                    }
                    else if (implSlotNumber == SpecialDispatchMapSlot.Diamond)
                    {
                        throw pTgtType->GetClasslibException(ExceptionIDs.AmbiguousImplementation);
                    }
                    else
                    {
                        // sealed virtual - need to get the slot form the implementing type, because
                        // it's not present on the target type
                        targetMethod = pCur->GetSealedVirtualSlot((ushort)(implSlotNumber - pCur->NumVtableSlots));
                    }
                    return targetMethod;*/
                }
                if (pCur->IsArray)
                    pCur = pCur->GetArrayEEType();
                else
                    pCur = pCur->NonArrayBaseType;
            }

            // If we haven't found an implementation, do a second pass looking for a default implementation.
            if (!fDoDefaultImplementationLookup)
            {
                fDoDefaultImplementationLookup = true;
                pCur = pTgtType;
                goto again;
            }

            return IntPtr.Zero;
        }



        /*
      
            while (true)
            {
                if (pCur != null)
                {
                   
                   // Console.WriteLine("FindImplSlotForCurrentType");
                    if (FindImplSlotForCurrentType(pCur, pItfType, itfSlotNumber, fDoDefaultImplementationLookup,
                            &implSlotNumber, ppGenericContext))
                    {



                        if (implSlotNumber < pCur->NumVtableSlots)
                        {
                            return pTgtType->GetVTableStartAddress()[(int)implSlotNumber];
                        }

                        return implSlotNumber switch
                        {
                            ushort.MaxValue => throw pTgtType->GetClasslibException(ExceptionIDs.EntrypointNotFound),
                            65534 => throw pTgtType->GetClasslibException(ExceptionIDs.AmbiguousImplementation),
                            _ => pCur->GetSealedVirtualSlot((ushort)(implSlotNumber - pCur->NumVtableSlots)),
                        };

                    
                    }
                    else
                    {
                      
                        if (fDoDefaultImplementationLookup)
                        {
                            break;
                        }

                        fDoDefaultImplementationLookup = true;
                        //   pCur = pTgtType;

                        pCur = ((!pCur->IsArray) ? pCur->NonArrayBaseType : pCur->GetArrayEEType());
                    }
                }
            }

            return IntPtr.Zero;
        }*/

        private unsafe static bool FindImplSlotForCurrentType(MethodTable* pTgtType, MethodTable* pItfType, ushort itfSlotNumber, bool fDoDefaultImplementationLookup, ushort* pImplSlotNumber, MethodTable** ppGenericContext)
        {
            bool fRes = false;
            if (!pItfType->IsInterface)
            {
                *pImplSlotNumber = itfSlotNumber;
                return pTgtType == pItfType;
            }
            if (pTgtType->HasDispatchMap)
            {
                bool fDoVariantLookup = false;
                fRes = FindImplSlotInSimpleMap(pTgtType, pItfType, itfSlotNumber, pImplSlotNumber, ppGenericContext, fDoVariantLookup, fDoDefaultImplementationLookup);
                if (!fRes)
                {
                    fDoVariantLookup = true;
                    fRes = FindImplSlotInSimpleMap(pTgtType, pItfType, itfSlotNumber, pImplSlotNumber, ppGenericContext, fDoVariantLookup, fDoDefaultImplementationLookup);
                }
            }
            /*else
            {
                IntPtr pTgtTypetPtr = (IntPtr)pTgtType;
                UInt32 DispatchMapIndex = pTgtType->DispatchMapIndex;
                IntPtr OptionalFieldsPtr = (IntPtr)pTgtType->OptionalFieldsPtr;
                Console.WriteLine("pTgtType->HasDispatchMap pTgtTypetPtr:=>"+ pTgtTypetPtr.ToString("x")+ ",DispatchMapIndex:=>" + DispatchMapIndex.ToString("x")+ ",OptionalFieldsPtr:=>" + OptionalFieldsPtr.ToString("x"));
            }*/
            return fRes;
        }
        private unsafe static bool FindImplSlotInSimpleMap(MethodTable* pTgtType,
                                   MethodTable* pItfType,
                                   uint itfSlotNumber,
                                   ushort* pImplSlotNumber,
                                   MethodTable** ppGenericContext,
                                   bool actuallyCheckVariance,
                                   bool checkDefaultImplementations)
        {
            // Debug.Assert(pTgtType->HasDispatchMap, "Missing dispatch map");

            MethodTable* pItfOpenGenericType = null;
            EETypeRef* itfInstantiation = null;
            int itfArity = 0;
            GenericVariance* pItfVarianceInfo = null;

            bool fCheckVariance = false;
            bool fArrayCovariance = false;

            if (actuallyCheckVariance)
            {
                fCheckVariance = pItfType->HasGenericVariance;
                fArrayCovariance = pTgtType->IsArray;

                // Non-arrays can follow array variance rules iff
                // 1. They have one generic parameter
                // 2. That generic parameter is array covariant.
                //
                // This special case is to allow array enumerators to work
                if (!fArrayCovariance && pTgtType->HasGenericVariance)
                {
                    int tgtEntryArity = (int)pTgtType->GenericArity;
                    GenericVariance* pTgtVarianceInfo = pTgtType->GenericVariance;

                    if ((tgtEntryArity == 1) && pTgtVarianceInfo[0] == GenericVariance.ArrayCovariant)
                    {
                        fArrayCovariance = true;
                    }
                }

                // Arrays are covariant even though you can both get and set elements (type safety is maintained by
                // runtime type checks during set operations). This extends to generic interfaces implemented on those
                // arrays. We handle this by forcing all generic interfaces on arrays to behave as though they were
                // covariant (over their one type parameter corresponding to the array element type).
                if (fArrayCovariance && pItfType->IsGeneric)
                    fCheckVariance = true;

                // If there is no variance checking, there is no operation to perform. (The non-variance check loop
                // has already completed)
                if (!fCheckVariance)
                {
                    return false;
                }
            }

            // It only makes sense to ask for generic context if we're asking about a static method
            bool fStaticDispatch = ppGenericContext != null;

            // We either scan the instance or static portion of the dispatch map. Depends on what the caller wants.
            DispatchMap* pMap = pTgtType->DispatchMap;
            IntPtr pMapIntPtr = (IntPtr)pMap;
            Debug.Assert(pMapIntPtr!=IntPtr.Zero, "pMapIntPtr!=IntPtr.Zero");

            DispatchMap.DispatchMapEntry* i = fStaticDispatch ?
                pMap->GetStaticEntry(checkDefaultImplementations ? (int)pMap->NumStandardStaticEntries : 0) :
                pMap->GetEntry(checkDefaultImplementations ? (int)pMap->NumStandardEntries : 0);
            DispatchMap.DispatchMapEntry* iEnd = fStaticDispatch ?
                pMap->GetStaticEntry(checkDefaultImplementations ? (int)(pMap->NumStandardStaticEntries + pMap->NumDefaultStaticEntries) : (int)pMap->NumStandardStaticEntries) :
                pMap->GetEntry(checkDefaultImplementations ? (int)(pMap->NumStandardEntries + pMap->NumDefaultEntries) : (int)pMap->NumStandardEntries);

            IntPtr iIntPtr = (IntPtr)i;
            IntPtr iEndIntPtr = (IntPtr)iEnd;
            Debug.Assert(iIntPtr != IntPtr.Zero & iEndIntPtr != IntPtr.Zero,
                "IntPtr != IntPtr.Zero && iEndIntPtr != IntPtr.Zero");

            for (; i != iEnd; i = fStaticDispatch ? (DispatchMap.DispatchMapEntry*)(((DispatchMap.StaticDispatchMapEntry*)i) + 1) : i + 1)
            {
                if (i->_usInterfaceMethodSlot == itfSlotNumber)
                {
                    MethodTable* pCurEntryType =
                        pTgtType->InterfaceMap[i->_usInterfaceIndex].InterfaceEntryType;

                
                    if (pCurEntryType == pItfType)
                    {
                        *pImplSlotNumber = i->_usImplMethodSlot;

                        // If this is a static method, the entry point is not usable without generic context.
                        // (Instance methods acquire the generic context from their `this`.)
                        if (fStaticDispatch)
                            *ppGenericContext = GetGenericContextSource(pTgtType, i);

                        return true;
                    }
                    else if (fCheckVariance && ((fArrayCovariance && pCurEntryType->IsGeneric) || pCurEntryType->HasGenericVariance))
                    {
                        // Interface types don't match exactly but both the target interface and the current interface
                        // in the map are marked as being generic with at least one co- or contra- variant type
                        // parameter. So we might still have a compatible match.

                        // Retrieve the unified generic instance for the callsite interface if we haven't already (we
                        // lazily get this then cache the result since the lookup isn't necessarily cheap).
                        if (pItfOpenGenericType == null)
                        {
                            pItfOpenGenericType = pItfType->GenericDefinition;
                            itfArity = (int)pItfType->GenericArity;
                            itfInstantiation = pItfType->GenericArguments;
                            pItfVarianceInfo = pItfType->GenericVariance;
                        }

                        // Retrieve the unified generic instance for the interface we're looking at in the map.
                        MethodTable* pCurEntryGenericType = pCurEntryType->GenericDefinition;

                        // If the generic types aren't the same then the types aren't compatible.
                        if (pItfOpenGenericType != pCurEntryGenericType)
                            continue;

                        // Grab instantiation details for the candidate interface.
                        EETypeRef* curEntryInstantiation = pCurEntryType->GenericArguments;

                        // The types represent different instantiations of the same generic type. The
                        // arity of both had better be the same.
                        Debug.Assert(itfArity == (int)pCurEntryType->GenericArity, "arity mismatch between generic instantiations");

                        if (TypeCast.TypeParametersAreCompatible(itfArity, curEntryInstantiation, itfInstantiation, pItfVarianceInfo, fArrayCovariance, null))
                        {
                            *pImplSlotNumber = i->_usImplMethodSlot;

                            // If this is a static method, the entry point is not usable without generic context.
                            // (Instance methods acquire the generic context from their `this`.)
                            if (fStaticDispatch)
                                *ppGenericContext = GetGenericContextSource(pTgtType, i);

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private unsafe static MethodTable* GetGenericContextSource(MethodTable* pTgtType, DispatchMap.DispatchMapEntry* pEntry)
        {
            ushort usEncodedValue = ((DispatchMap.StaticDispatchMapEntry*)pEntry)->_usContextMapSource;
            return usEncodedValue switch
            {
                0 => null,
                1 => pTgtType,
                _ => pTgtType->InterfaceMap[usEncodedValue - 2].InterfaceType,
            };
        }
    }


}
