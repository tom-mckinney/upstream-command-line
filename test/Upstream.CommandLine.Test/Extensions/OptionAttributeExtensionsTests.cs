using System;
using Upstream.CommandLine.Extensions;
using Xunit;

namespace Upstream.CommandLine.Test.Extensions;

public class OptionAttributeExtensionsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetValidatedAliases_returns_property_name_when_null(bool aliasesNull)
    {
        var option = new OptionAttribute
        {
            Aliases = aliasesNull ? null : new string[] { },
        };

        var output = option.GetValidatedAliases("foo");

        Assert.Equal("foo", Assert.Single(output));
    }

    [Theory]
    [InlineData("fooBar", "--foo-bar")]
    [InlineData("FooBar", "--foo-bar")]
    [InlineData("FooBar", "--foobar")]
    [InlineData("FooBar", "-foobar")]
    [InlineData("FooBar", "foobar")]
    [InlineData("FooBar", "f-o-o-b-a-r")]
    [InlineData("FooBar", "f.o@o!b a_r")]
    public void GetValidatedAliases_returns_aliases_when_property_name_matches(string propertyName, string alias)
    {
        var option = new OptionAttribute
        {
            Aliases = new[] { alias, "baz", "buzz", "food" },
        };

        var output = option.GetValidatedAliases(propertyName);

        Assert.Equal(4, output.Length);
        Assert.Same(option.Aliases, output);
    }

    [Fact]
    public void GetValidatedAliases_throws_if_property_name_does_not_match()
    {
        var option = new OptionAttribute
        {
            Aliases = new[] { "foo" },
        };

        Assert.Throws<InvalidOperationException>(() => option.GetValidatedAliases("food")); // one letter off
    }
}