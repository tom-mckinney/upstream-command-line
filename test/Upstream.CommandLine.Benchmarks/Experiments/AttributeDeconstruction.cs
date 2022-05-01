using System.CommandLine;
using BenchmarkDotNet.Attributes;
using Upstream.CommandLine.Utilities;

namespace Upstream.CommandLine.Benchmarks.Experiments;

[MemoryDiagnoser()]
public class AttributeDeconstruction
{
    [Benchmark]
    public Command SingletonType()
    {
        var type = typeof(FooCommand);
        
        var (name, description) = AttributeDeconstructor.GetCommandInfo(type);

        var command = new System.CommandLine.Command(name, description);
        
        foreach (var symbol in AttributeDeconstructor.GetSymbols(type))
        {
            switch (symbol)
            {
                case Argument argument:
                    command.Add(argument);
                    break;
                case Option option:
                    command.Add(option);
                    break;
            }
        }

        return command;
    }
    
    [Benchmark]
    public Command GetTypeEachTime()
    {
        var (name, description) = AttributeDeconstructor.GetCommandInfo(typeof(FooCommand));

        var command = new System.CommandLine.Command(name, description);
        
        foreach (var symbol in AttributeDeconstructor.GetSymbols(typeof(FooCommand)))
        {
            switch (symbol)
            {
                case Argument argument:
                    command.Add(argument);
                    break;
                case Option option:
                    command.Add(option);
                    break;
            }
        }

        return command;
    }

    [Command("foo", "Foo is the name of the command")]
    public class FooCommand
    {
        [Argument(Description = "Foo's counterpart")]
        public string Bar { get; set; }

        [Option("-e", "--easy", "--easy-mode", Description = "Print if it was easy")]
        public bool EasyMode { get; set; }

        [Option("-w", "--wumbo", DefaultValue = "Yes it does", Description = "Does it Wumbo?")]
        public string Wumbo { get; set; }
    }
}