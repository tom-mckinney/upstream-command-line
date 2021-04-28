using SampleConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Upstream.CommandLine;

namespace SampleConsoleApp.Commands
{
    public class FooOptions
    {
        [Argument(Description = "Foo's counterpart")]
        public string Bar { get; set; }

        [Option("-e", "--easy", "--easy-mode", Description = "Print if it was easy")]
        public bool EasyMode { get; set; }

        [Option("-w", "--wumbo", DefaultValue = "Yes it does", Description = "Does it Wumbo?")]
        public string Wumbo { get; set; }
    }

    public class FooCommand : CommandBase<FooOptions>
    {
        private readonly IRandomService _randomService;

        public FooCommand(IRandomService randomService)
        {
            _randomService = randomService;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"When I say \"Foo\", you say \"{Options.Bar}\"!");
            Console.WriteLine($"Random number: {_randomService.GetInt()}");
            Console.WriteLine($"Does it Wumbo?: {Options.Wumbo}");

            Program.Stopwatch.Stop();

            Console.WriteLine($"Ellapsed time: {Program.Stopwatch.ElapsedMilliseconds}ms");

            if (Options.EasyMode)
            {
                Console.WriteLine("THAT WAS EASY!");
            }

            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }
}
