using System;
using System.Threading;
using System.Threading.Tasks;
using SampleConsoleApp.Services;
using Upstream.CommandLine;

namespace SampleConsoleApp.Commands
{
    public enum Color
    {
        Default,
        Blue,
        Green,
        Red,
    }

    public class BarCommand
    {
        [Argument(Description = "Bar's counterpart")]
        public string Foo { get; set; }

        [Option("-o", "--other", Description = "The other value")]
        public string Other { get; set; }

        [Option("-c", "--color", Description = "Text color")]
        public Color Color { get; set; }
    }

    public class BarCommandHandler : ICommandHandler<BarCommand>
    {
        private readonly IDeterministicService _deterministicService;

        public BarCommandHandler(IDeterministicService deterministicService)
        {
            _deterministicService = deterministicService;
        }

        public Task<int> InvokeAsync(BarCommand options, CancellationToken cancellationToken)
        {
            switch (options.Color)
            {
                case Color.Blue:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case Color.Green:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case Color.Red:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    break;
            }

            Console.WriteLine($"When I say \"Bar\", you say \"{options.Foo}\"!");
            Console.WriteLine($"Everyone loves {_deterministicService.GetWhatEveryoneLoves()}!");
            if (!string.IsNullOrWhiteSpace(options.Other))
            {
                Console.WriteLine($"Bar is: {options.Other}");
            }

            cancellationToken.ThrowIfCancellationRequested();

            Console.ResetColor();

            return Task.FromResult(0);
        }
    }
}
