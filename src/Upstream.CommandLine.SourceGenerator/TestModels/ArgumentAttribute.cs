using System;
// using System.CommandLine;
// using System.Reflection;
// using Upstream.CommandLine.Exceptions;
// using Upstream.CommandLine.Extensions;

namespace Upstream.CommandLine
{
    public class ArgumentAttribute : DirectiveSymbolAttribute
    {
        private object? _defaultValue;

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

        public object? DefaultValue
        {
            get => _defaultValue;
            set
            {
                _defaultValue = value;
                HasDefaultValue = true;
            }
        }

        public bool HasDefaultValue { get; private set; }

        // public override Symbol GetSymbol(PropertyInfo property)
        // {
        //     var argumentType = typeof(Argument<>).MakeGenericType(property.PropertyType);
        //     var argumentObject =
        //         Activator.CreateInstance(argumentType, Name ?? property.Name.ToKebabCase(), Description);
        //
        //     if (argumentObject is not Argument argument)
        //     {
        //         throw new CommandLineException($"Unable to instantiate argument of type {argumentType}");
        //     }
        //
        //     if (!string.IsNullOrEmpty(Description))
        //     {
        //         argument.Description = Description;
        //     }
        //
        //     if (HasDefaultValue)
        //     {
        //         argument.SetDefaultValue(DefaultValue);
        //     }
        //
        //     return argument;
        // }
    }
}