using Upstream.CommandLine;

namespace UpstreamSourceGeneratorApp;

[Command("foo", Description = "Foo command")]
public class TestFooCommand
{
    [Argument("value")] public string Value { get; set; } = null!;

    [Option("-v", "--verbose")] public bool Verbose { get; set; }
}