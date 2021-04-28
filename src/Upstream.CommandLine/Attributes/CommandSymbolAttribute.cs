using System;
using System.CommandLine;
using System.Reflection;

namespace Upstream.CommandLine
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class CommandSymbolAttribute : Attribute
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public abstract Symbol GetSymbol(PropertyInfo property);
    }
}
