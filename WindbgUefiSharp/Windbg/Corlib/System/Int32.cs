namespace System
{
	public struct Int32
	{
        private int _value;

        public const int MaxValue = 2147483647;

        public const int MinValue = -2147483648;

        public override string ToString()
		{
			return ((long)this).ToString();
		}


        public string ToString(string format)
        {
            return ((ulong)this).ToString(format);
        }

        public unsafe override bool Equals(object o)
        {
            return ((long)this) == ((long)o);
        }
        public static implicit operator uint(int value)=>(uint)value;
	}
}