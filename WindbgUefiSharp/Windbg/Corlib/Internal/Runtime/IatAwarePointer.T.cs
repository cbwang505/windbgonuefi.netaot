namespace Internal.Runtime
{
    internal readonly struct IatAwarePointer<T> where T : unmanaged
    {
        private unsafe readonly T* _value;

        public unsafe T* Value
        {
            get
            {
                if (((int)_value & 1) == 0)
                {
                    return _value;
                }
                return *(T**)((byte*)_value - 1);
            }
        }
    }
}
