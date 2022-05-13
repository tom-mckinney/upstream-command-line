using System;
using System.Text.RegularExpressions;

namespace Upstream.CommandLine.Extensions
{
    public static class OptionAttributeExtensions
    {
        public static string[] GetValidatedAliases(this OptionAttribute option, string propertyName)
        {
            if (option?.Aliases == null || option.Aliases.Length <= 0)
            {
                return new[] { propertyName.ToKebabCase() };
            }

            // work backwards since the matching alias is typically the last one declared
            for (int i = option.Aliases.Length - 1; i >= 0; i--)
            {
                if (propertyName.Equals(Regex.Replace(option.Aliases[i], @"[^A-Za-z\d]", ""),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return option.Aliases;
                }
            }

            throw new InvalidOperationException(
                $"At least one alias must matching the property for value assignment. Recommended alias: {propertyName.ToKebabCase()}");
        }
    }
}