using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Upstream.CommandLine.Exceptions;

namespace Upstream.CommandLine.Utilities
{
    public static class AttributeDeconstructor
    {
        public static (string Name, string? Description) GetCommandInfo(Type type)
        {
            var commandAttribute = (CommandAttribute?)Attribute.GetCustomAttribute(type, typeof(CommandAttribute));

            if (commandAttribute == null)
            {
                throw new CommandLineException($"Command type {type.Name} is not decorated with {nameof(CommandAttribute)}");
            }

            return (commandAttribute.Name, commandAttribute.Description);
        }

        public static IEnumerable<Symbol> GetSymbols(Type type)
        {
            foreach (var prop in type.GetProperties())
            {
                if (TryGetSymbol(prop, out var symbol))
                {
                    yield return symbol;
                }
            }
        }

        public static bool TryGetSymbol(PropertyInfo prop, [NotNullWhen(true)] out Symbol? symbol)
        {
            symbol = GetSymbol(prop);

            return symbol != null;
        }

        public static Symbol? GetSymbol(PropertyInfo prop)
        {
            var attribute =
                (DirectiveSymbolAttribute?)Attribute.GetCustomAttribute(prop, typeof(DirectiveSymbolAttribute));

            return attribute?.GetSymbol(prop);
        }
    }
}
