using FluentAssertions;
using Upstream.CommandLine.Extensions;
using Xunit;

namespace Upstream.CommandLine.Test.Extensions
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("Option123", "option123")]
        [InlineData("dWORD", "d-word")]
        [InlineData("MSBuild", "msbuild")]
        [InlineData("NoEdit", "no-edit")]
        [InlineData("SetUpstreamBranch", "set-upstream-branch")]
        [InlineData("lowerCaseFirst", "lower-case-first")]
        [InlineData("_field", "field")]
        [InlineData("__field", "field")]
        [InlineData("___field", "field")]
        [InlineData("m_field", "m-field")]
        [InlineData("m_Field", "m-field")]
        public void ToKebabCase(string input, string expected) => input.ToKebabCase().Should().Be(expected);
    }
}
