using Upstream.CommandLine;

namespace UpstreamSourceGeneratorApp;

partial class Program
{
    static void Main(string[] args)
    {
        var builder = new CommandLineApplication();
        
        builder.AutoWireCommands();
    }
}