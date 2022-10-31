using System.Collections.Generic;
using System.CommandLine;
using Upstream.CommandLine.Utilities;
using Xunit;

namespace Upstream.CommandLine.Test.Utilities
{
    public class AttributeDeconstructorTests
    {
        class GoodOptions
        {
            [Argument("foo")]
            public string Foo { get; set; }

            [Argument("barr")]
            public int Bar { get; set; }

            [Argument("wumbo", 11)]
            public int Wumbo { get; set; }

            [Argument(DefaultValue = null)]
            public bool? FooBar { get; set; }

            [Option("-r", "--reason", Description = "Reason for doing the thing")]
            public string Reason { get; set; }

            [Option(AllowMultipleArgumentsPerToken = true)]
            public IEnumerable<string> ListItems { get; set; }
        }

        [Fact]
        public void GetSymbol_arguments()
        {
            var type = typeof(GoodOptions);

            var fooProperty = type.GetProperty(nameof(GoodOptions.Foo));
            var fooArg = (Argument)AttributeDeconstructor.GetSymbol(fooProperty);
            Assert.Equal("foo", fooArg.Name);
            Assert.False(fooArg.HasDefaultValue);
            Assert.Equal(fooProperty.PropertyType, fooArg.ValueType);

            var barrProperty = type.GetProperty(nameof(GoodOptions.Bar));
            var barrArg = (Argument)AttributeDeconstructor.GetSymbol(barrProperty);
            Assert.Equal("barr", barrArg.Name);
            Assert.False(barrArg.HasDefaultValue);
            Assert.Equal(barrProperty.PropertyType, barrArg.ValueType);

            var wumboProperty = type.GetProperty(nameof(GoodOptions.Wumbo));
            var wumboArg = (Argument)AttributeDeconstructor.GetSymbol(wumboProperty);
            Assert.Equal("wumbo", wumboArg.Name);
            Assert.True(wumboArg.HasDefaultValue);
            Assert.Equal(11, wumboArg.GetDefaultValue());
            Assert.Equal(wumboProperty.PropertyType, wumboArg.ValueType);

            var fooBarProperty = type.GetProperty(nameof(GoodOptions.FooBar));
            var fooBarArg = (Argument)AttributeDeconstructor.GetSymbol(fooBarProperty);
            Assert.Equal("foo-bar", fooBarArg.Name);
            Assert.True(fooBarArg.HasDefaultValue);
            Assert.Null(fooBarArg.GetDefaultValue());
            Assert.Equal(fooBarProperty.PropertyType, fooBarArg.ValueType);
        }

        [Fact]
        public void GetSymbol_options()
        {
            var type = typeof(GoodOptions);

            var reasonProperty = type.GetProperty(nameof(GoodOptions.Reason));
            var reasonOption = (Option)AttributeDeconstructor.GetSymbol(reasonProperty);
            Assert.Equal(2, reasonOption.Aliases.Count);
            Assert.Contains("-r", reasonOption.Aliases);
            Assert.Contains("--reason", reasonOption.Aliases);
            Assert.Equal("Reason for doing the thing", reasonOption.Description);
            Assert.Equal(reasonProperty.PropertyType, reasonOption.ValueType);
            Assert.Equal(ArgumentArity.ExactlyOne, reasonOption.Arity);

            var listItemProperty = type.GetProperty(nameof(GoodOptions.ListItems));
            var listItemOption = (Option)AttributeDeconstructor.GetSymbol(listItemProperty);
            Assert.True(listItemOption.AllowMultipleArgumentsPerToken);
            Assert.Equal(ArgumentArity.OneOrMore, listItemOption.Arity);
        }
    }
}
