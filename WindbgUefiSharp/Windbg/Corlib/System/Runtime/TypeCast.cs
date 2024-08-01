using Internal.Runtime;
using Internal.Runtime.CompilerHelpers;
using System.Runtime.CompilerServices;
using static Internal.Runtime.MethodTable;

namespace System.Runtime
{
    public static partial class TypeCast
    {
        /* TODO: URGENT: the following simply doesn't let us compile, I don't know why yet
        [Flags]
        internal enum AssignmentVariation
        {
            Normal = 0,
            BoxedSource = 1,
            AllowSizeEquivalence = 2
        }

        internal struct ArrayElement
        {
            public object Value;
        }

        internal struct EETypePairList
        {
            private unsafe MethodTable* _eetype1;

            private unsafe MethodTable* _eetype2;

            private unsafe EETypePairList* _next;

            public unsafe EETypePairList(MethodTable* pEEType1, MethodTable* pEEType2, EETypePairList* pNext)
            {
                _eetype1 = pEEType1;
                _eetype2 = pEEType2;
                _next = pNext;
            }

            public unsafe static bool Exists(EETypePairList* pList, MethodTable* pEEType1, MethodTable* pEEType2)
            {
                while (pList != null)
                {
                    if (pList->_eetype1 == pEEType1 && pList->_eetype2 == pEEType2)
                    {
                        return true;
                    }
                    if (pList->_eetype1 == pEEType2 && pList->_eetype2 == pEEType1)
                    {
                        return true;
                    }
                    pList = pList->_next;
                }
                return false;
            }
        }

        //[EagerStaticClassConstruction]
        private static class CastCache
        {
            private sealed class Entry
            {
                public Entry Next;

                public Key Key;

                public bool Result;
            }

            private struct Key
            {
                private IntPtr _sourceTypeAndVariation;

                private IntPtr _targetType;

                public AssignmentVariation Variation => (AssignmentVariation)((int)(long)_sourceTypeAndVariation & 3);

                public unsafe MethodTable* SourceType => (MethodTable*)((long)_sourceTypeAndVariation & -4);

                public unsafe MethodTable* TargetType => (MethodTable*)(void*)_targetType;

                public unsafe Key(MethodTable* pSourceType, MethodTable* pTargetType, AssignmentVariation variation)
                {
                    _sourceTypeAndVariation = (IntPtr)((byte*)pSourceType + (int)variation);
                    _targetType = (IntPtr)pTargetType;
                }

                private static int GetHashCode(IntPtr intptr)
                {
                    return (int)(long)intptr;
                }

                public int CalculateHashCode()
                {
                    return (GetHashCode(_targetType) >> 4) ^ GetHashCode(_sourceTypeAndVariation);
                }

                public bool Equals(ref Key other)
                {
                    if (_sourceTypeAndVariation == other._sourceTypeAndVariation)
                    {
                        return _targetType == other._targetType;
                    }
                    return false;
                }
            }

            private const int InitialCacheSize = 128;

            private const int DefaultCacheSize = 1024;

            private const int MaximumCacheSize = 131072;

            private static Entry[] s_cache = new Entry[128];

            //private static UnsafeGCHandle s_previousCache;

            //private static ulong s_tickCountOfLastOverflow = InternalCalls.RhpGetTickCount64();

            private static int s_entries;

            private static bool s_roundRobinFlushing;

            public unsafe static bool AreTypesAssignableInternal(MethodTable* pSourceType, MethodTable* pTargetType, AssignmentVariation variation, EETypePairList* pVisited)
            {
                if (pSourceType == pTargetType)
                {
                    return true;
                }
                Key key = new Key(pSourceType, pTargetType, variation);
                //Missing CacheMiss might bust some stuff, but I can't afford toplug it right now
                return LookupInCache(s_cache, ref key)?.Result; //?? CacheMiss(ref key, pVisited);
            }

            //AreTypesAssignableInternal_SourceNotTarget_BoxedSource

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static Entry LookupInCache(Entry[] cache, ref Key key)
            {
                int entryIndex = key.CalculateHashCode() & (cache.Length - 1);
                Entry entry = cache[entryIndex];
                while (entry != null && !entry.Key.Equals(ref key))
                {
                    entry = entry.Next;
                }
                return entry;
            }

            //CacheMiss

            //ResizeCacheForNewEntryAsNecessary

        }

        */

        /*

        [RuntimeExport("RhTypeCast_IsInstanceOfClass")]
        public unsafe static object IsInstanceOfClass(MethodTable* pTargetType, object obj)
        {
            if (obj == null || obj.GetMethodTable() == pTargetType)
            {
                return obj;
            }
            MethodTable* pObjType = obj.GetMethodTable();
            if (MethodTable.BothSimpleCasting(pObjType, pTargetType))
            {
                do
                {
                    pObjType = pObjType->RawBaseType;
                    if (pObjType == null)
                    {
                        return null;
                    }
                    if (pObjType == pTargetType)
                    {
                        return obj;
                    }
                }
                while (pObjType->SimpleCasting());
            }
            return IsInstanceOfClass_Helper(pTargetType, obj, pObjType);
        }
        
        private unsafe static object IsInstanceOfClass_Helper(MethodTable* pTargetType, object obj, MethodTable* pObjType)
        {
            if (pTargetType->IsCloned)
            {
                pTargetType = pTargetType->CanonicalEEType;
            }
            if (pObjType->IsCloned)
            {
                pObjType = pObjType->CanonicalEEType;
            }
            if (pObjType == pTargetType)
            {
                return obj;
            }
            if (pTargetType->HasGenericVariance && pObjType->HasGenericVariance)
            {
                if (CastCache.AreTypesAssignableInternal(pObjType, pTargetType, AssignmentVariation.BoxedSource, null))
                {
                    return obj;
                }
                return null;
            }
            if (pObjType->IsArray)
            {
                if (WellKnownEETypes.IsSystemObject(pTargetType))
                {
                    return obj;
                }
                if (WellKnownEETypes.IsSystemArray(pTargetType))
                {
                    return obj;
                }
                return null;
            }
            do
            {
                pObjType = pObjType->NonClonedNonArrayBaseType;
                if (pObjType == null)
                {
                    return null;
                }
                if (pObjType->IsCloned)
                {
                    pObjType = pObjType->CanonicalEEType;
                }
            }
            while (pObjType != pTargetType);
            return obj;
        }

        */

        [RuntimeExport("RhTypeCast_CheckCastClass")]
        public static unsafe object CheckCastClass(MethodTable* pTargetEEType, object obj)
        {
            // a null value can be cast to anything
            if (obj == null)
                return null;
            //TODO: probably don't call from startupcodehelpers
            object result = StartupCodeHelpers.RhTypeCast_IsInstanceOfClass(pTargetEEType, obj);

            if (result == null)
            {
                // Throw the invalid cast exception defined by the classlib, using the input EEType* 
                // to find the correct classlib.

                //throw pTargetEEType->GetClasslibException(ExceptionIDs.InvalidCast);
                return null;
            }

            return result;
        }

        //IsInstanceOfArray

        [RuntimeExport("RhTypeCast_IsInstanceOfArray")]
        public static unsafe object IsInstanceOfArray(MethodTable* pTargetType, object obj)
        {
            if (obj == null)
            {
                return null;
            }

            MethodTable* pObjType = obj.m_pEEType;

            // if the types match, we are done
            if (pObjType == pTargetType)
            {
                return obj;
            }

            // if the object is not an array, we're done
            if (!pObjType->IsArray)
            {
                return null;
            }

            // compare the array types structurally

            if (pObjType->ParameterizedTypeShape != pTargetType->ParameterizedTypeShape)
            {
                // If the shapes are different, there's one more case to check for: Casting SzArray to MdArray rank 1.
                if (!pObjType->IsSzArray || pTargetType->ArrayRank != 1)
                {
                    return null;
                }
            }

            /*
            if (CastCache.AreTypesAssignableInternal(pObjType->RelatedParameterType, pTargetType->RelatedParameterType,
                AssignmentVariation.AllowSizeEquivalence, null))
            {
                return obj;
            }
            */

            return null;
        }

        [RuntimeExport("RhTypeCast_CheckCastArray")]
        public static unsafe object CheckCastArray(MethodTable* pTargetEEType, object obj)
        {
            // a null value can be cast to anything
            if (obj == null)
                return null;

            object result = IsInstanceOfArray(pTargetEEType, obj);

            if (result == null)
            {
                // Throw the invalid cast exception defined by the classlib, using the input EEType* 
                // to find the correct classlib.

                //throw pTargetEEType->GetClasslibException(ExceptionIDs.InvalidCast);
                return null;
            }

            return result;
        }
        /*[RuntimeExport("RhTypeCast_IsInstanceOfInterface")]
       public static unsafe object IsInstanceOfInterface(MethodTable* pTargetType, object obj)
       {
           if (obj == null)
           {
               return null;
           }

           MethodTable* pObjType = obj.MethodTable;

         

           // If object type implements IDynamicInterfaceCastable then there's one more way to check whether it implements
           // the interface.
           if (pObjType->IsIDynamicInterfaceCastable && IsInstanceOfInterfaceViaIDynamicInterfaceCastable(pTargetType, obj, throwing: false))
               return obj;

           return null;
       }
        private static unsafe bool IsInstanceOfInterfaceViaIDynamicInterfaceCastable(MethodTable* pTargetType, object obj, bool throwing)
        {
            var pfnIsInterfaceImplemented = (delegate*<object, MethodTable*, bool, bool>)
                pTargetType->GetClasslibFunction(ClassLibFunctionId.IDynamicCastableIsInterfaceImplemented);
            return pfnIsInterfaceImplemented(obj, pTargetType, throwing);
        }*/
        //[RuntimeExport("RhTypeCast_IsInstanceOfInterface")]
        //IsInstanceOfInterface

        //IsInstanceOfInterfaceViaIDynamicInterfaceCastable

        //ImplementsInterface



        //[RuntimeExport("RhTypeCast_AreTypesAssignable")]
        //AreTypesAssignable

        //AreTypesAssignableInternal

        //[RuntimeExport("RhTypeCast_CheckCastInterface")]
        //CheckCastInterface

        //[RuntimeExport("RhTypeCast_CheckArrayStore")]
        //CheckArrayStore

        //[RuntimeExport("RhTypeCast_CheckArrayStore")]
        //CheckVectorElemAddr

        //[RuntimeExport("RhTypeCast_CheckVectorElemAddr")]
        //CheckVectorElemAddr

        //[RuntimeExport("RhpStelemRef")]
        //StelemRef

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //StelemRef_Helper

        //[RuntimeExport("RhpLdelemaRef")]
        //LdelemaRef

        //IsDerived
        //TypesAreCompatibleViaGenericVariance
        // TODO: URGENT: TypeParametersAreCompatible simply doesn't let us compile, I don't know why yet
        internal unsafe static bool TypeParametersAreCompatible(int arity, EETypeRef* pSourceInstantiation, EETypeRef* pTargetInstantiation, GenericVariance* pVarianceInfo, bool fForceCovariance, void* pVisited)
        {

            for (int i = 0; i < arity; i++)
            {
                MethodTable* pTargetArgType = pTargetInstantiation[i].Value;
                MethodTable* pSourceArgType = pSourceInstantiation[i].Value;
                switch ((!fForceCovariance) ? pVarianceInfo[i] : GenericVariance.ArrayCovariant)
                {
                    case GenericVariance.NonVariant:
                        if (!AreTypesEquivalent(pSourceArgType, pTargetArgType))
                        {
                            return false;
                        }
                        break;
                    case GenericVariance.Covariant:
                        /*if (!CastCache.AreTypesAssignableInternal(pSourceArgType, pTargetArgType, AssignmentVariation.Normal, pVisited))
                        {
                            return false;
                        }*/
                        break;
                    case GenericVariance.ArrayCovariant:
                        /*if (!CastCache.AreTypesAssignableInternal(pSourceArgType, pTargetArgType, AssignmentVariation.AllowSizeEquivalence, pVisited))
                        {
                            return false;
                        }*/
                        break;
                    case GenericVariance.Contravariant:
                        /*if (!CastCache.AreTypesAssignableInternal(pTargetArgType, pSourceArgType, AssignmentVariation.Normal, pVisited))
                        {
                            return false;
                        }*/
                        break;
                }
            }

            return false;
        }
        [RuntimeExport("RhTypeCast_AreTypesEquivalent")]
        public unsafe static bool AreTypesEquivalent(MethodTable* pType1, MethodTable* pType2)
        {
            if (pType1 == pType2)
            {
                return true;
            }
            if (pType1->IsCloned)
            {
                pType1 = pType1->CanonicalEEType;
            }
            if (pType2->IsCloned)
            {
                pType2 = pType2->CanonicalEEType;
            }
            if (pType1 == pType2)
            {
                return true;
            }
            if (pType1->IsParameterizedType && pType2->IsParameterizedType)
            {
                if (AreTypesEquivalent(pType1->RelatedParameterType, pType2->RelatedParameterType))
                {
                    return pType1->ParameterizedTypeShape == pType2->ParameterizedTypeShape;
                }
                return false;
            }
            return false;
        }

        //[RuntimeExport("RhTypeCast_IsInstanceOf")]
        //IsInstanceOf

        //[RuntimeExport("RhTypeCast_IsInstanceOfException")]
        //IsInstanceOfException

        //[RuntimeExport("RhTypeCast_CheckCast")]
        //CheckCast

        //CheckCastNonArrayParameterizedType

        //GetNormalizedIntegralArrayElementType
    }
}
