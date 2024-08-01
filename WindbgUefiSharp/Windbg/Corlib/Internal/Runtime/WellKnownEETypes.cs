namespace Internal.Runtime
{
    internal static class WellKnownEETypes
    {
        internal unsafe static bool IsSystemObject(MethodTable* pEEType)
        {
            if (pEEType->IsArray)
            {
                return false;
            }
            if (pEEType->NonArrayBaseType == null)
            {
                return !pEEType->IsInterface;
            }
            return false;
        }

        internal unsafe static bool IsSystemArray(MethodTable* pEEType)
        {
            return pEEType->ElementType == EETypeElementType.SystemArray;
        }
    }
}
