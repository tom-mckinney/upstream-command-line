using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Text;

namespace Upstream.CommandLine
{
    public class OptionAttribute : CommandSymbolAttribute
    {
        private static readonly object _uninitializedDefaultValue = new object();

        public OptionAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }

        public string[] Aliases { get; set; }

        public SymbolType Type { get; set; }

        public bool Required { get; set; } = false;

        public object DefaultValue { get; set; } = _uninitializedDefaultValue;

        public bool HasDefaultValue => DefaultValue != _uninitializedDefaultValue;

        public override Symbol GetSymbol(PropertyInfo property)
        {
            var aliases = Aliases?.Length > 0 ? Aliases : new[] { property.Name.ToKebabCase() };

            var option = new Option(aliases);

            if (Type == SymbolType.Default)
            {
                option.Argument = new Argument(Name)
                {
                    ArgumentType = property.PropertyType
                };
            }

            if (!string.IsNullOrEmpty(Description))
            {
                option.Description = Description;
            }

            if (Required)
            {
                option.Required = true;
            }

            if (HasDefaultValue)
            {
                option.Argument.SetDefaultValue(DefaultValue);
            }

            return option;
        }
    }
}
