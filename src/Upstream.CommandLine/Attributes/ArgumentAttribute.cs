using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;
using Upstream.CommandLine.Extensions;

namespace Upstream.CommandLine
{
    public class ArgumentAttribute : CommandSymbolAttribute
    {
        private static readonly object _uninitializedDefaultValue = new();

        public ArgumentAttribute()
        {
        }

        public ArgumentAttribute(string name)
        {
            Name = name;
        }

        public ArgumentAttribute(string name, object defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public object DefaultValue { get; set; } = _uninitializedDefaultValue;

        public bool HasDefaultValue => DefaultValue != _uninitializedDefaultValue;

        public override Symbol GetSymbol(PropertyInfo property)
        {
            var argument = new Argument(Name ?? property.Name.ToKebabCase())
            {
                ValueType = property.PropertyType
            };

            if (!string.IsNullOrEmpty(Description))
            {
                argument.Description = Description;
            }

            if (HasDefaultValue)
            {
                argument.SetDefaultValue(DefaultValue);
            }

            return argument;
        }
    }
}
