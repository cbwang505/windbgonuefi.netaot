namespace Internal.Runtime
{
    internal readonly struct Pointer<T> where T : unmanaged
    {
        private unsafe readonly T* _value;

        public unsafe T* Value => _value;
    }
}
