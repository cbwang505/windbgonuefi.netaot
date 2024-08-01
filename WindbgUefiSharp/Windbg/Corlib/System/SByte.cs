
namespace System
{
    public struct SByte
    {
        private sbyte _value;

        public const sbyte MaxValue = 127;

        public const sbyte MinValue = -128;

        public override string ToString()
        {
            return ((long)this).ToString();
        }
    }
}
