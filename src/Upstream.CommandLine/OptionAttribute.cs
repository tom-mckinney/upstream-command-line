using System;
using System.CommandLine;
using System.Reflection;
using Upstream.CommandLine.Extensions;

namespace Upstream.CommandLine
{
    public class OptionAttribute : DirectiveSymbolAttribute
    {
        private static readonly object _uninitializedDefaultValue = new();
        private object _defaultValue = _uninitializedDefaultValue;

        public OptionAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }

        public string[]? Aliases { get; set; }

        public bool IsRequired { get; set; } = false;

        public object DefaultValue
        {
            get => _defaultValue;
            set
            {
                _defaultValue = value;
                SetDefaultValue = () => DefaultValue;
            }
        }

        public Func<object?>? SetDefaultValue { get; private set; }

        public bool HasDefaultValue => DefaultValue != _uninitializedDefaultValue;

        public override Symbol GetSymbol(PropertyInfo property)
        {
            var option = new Option(
                this.GetValidatedAliases(property.Name),
                description: Description,
                getDefaultValue: SetDefaultValue,
                argumentType: property.PropertyType);

            if (IsRequired)
            {
                option.IsRequired = true;
            }

            return option;
        }
    }
}
