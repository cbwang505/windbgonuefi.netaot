
namespace System
{
    public class Nullable<T> where T : struct
    {

        public T Value;
        public bool HasValue;
        public Nullable()
        {
            HasValue = false;
        }

        public Nullable(T valuefrom)
        {
            Value = valuefrom;
            HasValue=true;
        }
    }
}
