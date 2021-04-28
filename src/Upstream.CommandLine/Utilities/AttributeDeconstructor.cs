using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Upstream.CommandLine.Utilities
{
    public static class AttributeDeconstructor
    {
        public static List<Symbol> GetSymbols(Type type)
        {
            var symbols = new List<Symbol>();

            foreach (var prop in type.GetProperties())
            {
                if (TryGetSymbol(prop, out Symbol? symbol))
                {
                    symbols.Add(symbol);
                }
            }

            return symbols;
        }

        public static bool TryGetSymbol(PropertyInfo prop, [NotNullWhen(true)] out Symbol? symbol)
        {
            symbol = GetSymbol(prop);

            return symbol != null;
        }

        public static Symbol? GetSymbol(PropertyInfo prop)
        {
            var attribute = (CommandSymbolAttribute[])Attribute.GetCustomAttributes(prop, typeof(CommandSymbolAttribute));

            return attribute?.SingleOrDefault()?.GetSymbol(prop);
        }
    }
}
