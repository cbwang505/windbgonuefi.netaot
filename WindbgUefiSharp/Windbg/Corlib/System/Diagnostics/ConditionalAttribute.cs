using System;

namespace System.Diagnostics
{
    [AttributeUsage((AttributeTargets)68, AllowMultiple = true)]
    internal sealed class ConditionalAttribute : Attribute
    {
        private string _conditionString;

        public string ConditionString => _conditionString;

        public ConditionalAttribute(string conditionString)
        {
            _conditionString = conditionString;
        }
    }
}
