using System;
using System.Threading;
using System.Threading.Tasks;
using Upstream.CommandLine;
using UpstreamExampleApp.Services;

namespace UpstreamExampleApp.Handlers
{
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

    public class FooCommandHandler : ICommandHandler<FooCommand>
    {
        private readonly IRandomService _randomService;

        public FooCommandHandler(IRandomService randomService)
        {
            _randomService = randomService;
        }

        public Task<int> ExecuteAsync(FooCommand command, CancellationToken cancellationToken)
        {
            Console.WriteLine($"When I say \"Foo\", you say \"{command.Bar}\"!");
            Console.WriteLine($"Random number: {_randomService.GetInt()}");
            Console.WriteLine($"Does it Wumbo?: {command.Wumbo}");

            if (command.EasyMode)
            {
                Console.WriteLine("THAT WAS EASY!");
            }

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(0);
        }
    }
}
