using System;
using System.Collections.Generic;

namespace System.Runtime
{
    [AttributeUsage((AttributeTargets)96, Inherited = false)]
    public sealed class RuntimeImportAttribute : Attribute
    {
        private readonly string _dllName;
        private readonly string _entryPoint;

        public string DllName => _dllName;

        public string EntryPoint => _entryPoint;

        public RuntimeImportAttribute(string entry)
        {
            _entryPoint = entry;
        }

        public RuntimeImportAttribute(string dllName, string entry)
        {
            _dllName = dllName;
            _entryPoint = entry;
        }
    }


   
}
