namespace System
{
	public struct Int16
	{
        private short _value;

        public const short MaxValue = 32767;

        public const short MinValue = -32768;

        public override string ToString()
		{
			return ((long)this).ToString();
		}


        public unsafe override bool Equals(object o)
        {
            return ((long)this) > ((long)o);
        }
    }
}