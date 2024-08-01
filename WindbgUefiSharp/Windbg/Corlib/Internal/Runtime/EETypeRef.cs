namespace Internal.Runtime
{
    internal struct EETypeRef
    {
        private unsafe byte* _value;

        public unsafe MethodTable* Value
        {
            get
            {
                if (((int)_value & 1) == 0)
                {
                    return (MethodTable*)_value;
                }
                return *(MethodTable**)(_value - 1);
            }
        }
    }
}