using SampleConsoleApp.Services;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
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

    public class BarOptions
    {
        [Argument(Description = "Bar's counterpart")]
        public string Foo { get; set; }

        [Option("-o", "--other", Description = "The other value")]
        public string Other { get; set; }

        [Option("-c", "--color", Description = "Text color")]
        public Color Color { get; set; }
    }

    public class BarCommand : ICommandAction<BarOptions>
    {
        private readonly IDeterministicService _deterministicService;

        public BarCommand(IDeterministicService deterministicService)
        {
            _deterministicService = deterministicService;
        }

        public Task InvokeAsync(BarOptions options, CancellationToken cancellationToken)
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

            return Task.CompletedTask;
        }
    }
}
