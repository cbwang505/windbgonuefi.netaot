namespace System.Runtime
{
    // Custom attribute that the compiler understands that instructs it
    // to export the method under the given symbolic name.
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class RuntimeExportAttribute : Attribute
    {
        public string EntryPoint;

        public RuntimeExportAttribute(string entry) 
        {
            EntryPoint = entry;
        }
    }
}
