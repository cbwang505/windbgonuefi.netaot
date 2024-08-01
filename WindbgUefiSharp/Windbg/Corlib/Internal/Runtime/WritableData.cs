namespace Internal.Runtime
{
    internal static class WritableData
    {
        public static int GetSize(int pointerSize)
        {
            return pointerSize;
        }

        public static int GetAlignment(int pointerSize)
        {
            return pointerSize;
        }
    }
}
