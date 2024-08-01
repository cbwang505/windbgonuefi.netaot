namespace System
{
    public partial class Exception
    {
        private string _exceptionString;

        public Exception()
        {
            
        }

        public override string ToString()
        {
            return _exceptionString;
        }

        public Exception(string str)
        {
            _exceptionString = str;
        }

        //public string ExceptionString => _exceptionString;
    }
}
