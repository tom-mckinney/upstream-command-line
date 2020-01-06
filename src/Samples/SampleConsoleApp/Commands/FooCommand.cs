using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;
using Upstream.CommandLine;

namespace SampleConsoleApp.Commands
{
    public class FooOptions
    {
        public string Bar { get; set; }
    }

    public class FooCommand : CommandBase<FooOptions>
    {
        protected override string Name => "foo";

        protected override Argument[] Arguments => new[]
        {
            new Argument<string>("bar")
        };

        protected override Task ExecuteAsync()
        {
            Console.WriteLine($"When I say \"Foo\", you say \"{Options.Bar}\"!");

            Program.Stopwatch.Stop();

            Console.WriteLine($"Ellapsed time: {Program.Stopwatch.ElapsedMilliseconds}ms");

            return Task.CompletedTask;
        }
    }
}
