namespace System
{
    public struct Byte
    {
        private byte _value;

        public const byte MaxValue = 255;

        public const byte MinValue = 0;

        public unsafe override string ToString()
        {
            return ((ulong)this).ToString();
        }

        public string ToString(string format)
        {
            return ((ulong)this).ToString(format);
        }
    }
}
