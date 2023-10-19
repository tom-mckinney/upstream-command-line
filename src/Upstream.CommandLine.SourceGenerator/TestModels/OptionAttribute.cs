using System;
// using System.CommandLine;
// using System.Reflection;
// using Upstream.CommandLine.Exceptions;
// using Upstream.CommandLine.Extensions;

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

        public bool AllowMultipleArgumentsPerToken { get; set; } = false;

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

        // public override Symbol GetSymbol(PropertyInfo property)
        // {
        //     var optionType = typeof(Option<>).MakeGenericType(property.PropertyType);
        //     var optionObject = Activator.CreateInstance(optionType, this.GetValidatedAliases(property.Name), Description);
        //
        //     if (optionObject is not Option option)
        //     {
        //         throw new CommandLineException($"Unable to instantiate option of type {optionType}");
        //     }
        //
        //     if (IsRequired)
        //     {
        //         option.IsRequired = true;
        //     }
        //
        //     if (AllowMultipleArgumentsPerToken)
        //     {
        //         option.AllowMultipleArgumentsPerToken = true;
        //     }
        //
        //     if (SetDefaultValue != null)
        //     {
        //         option.SetDefaultValueFactory(SetDefaultValue);
        //     }
        //
        //     return option;
        // }
    }
}