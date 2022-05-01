using Microsoft.Extensions.DependencyInjection;
using SampleConsoleApp.Commands;
using SampleConsoleApp.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Upstream.CommandLine;

namespace SampleConsoleApp
{
    public static class Program
    {
        public static Stopwatch Stopwatch { get; } = new Stopwatch();

        public static async Task Main(string[] args)
        {
            Stopwatch.Start();

            try
            {
                await BuildCommandLineApplication()
                    .InvokeAsync(args);
            }
            finally
            {
                Stopwatch.Stop();

                Console.WriteLine($"Ellapsed time: {Stopwatch.ElapsedMilliseconds}ms");
            }
        }

        public static CommandLineApplication BuildCommandLineApplication()
        {
            return new CommandLineApplication()
                    .AddCommand<FooCommandHandler, FooCommand>()
                    .AddCommand<BarCommandHandler, BarCommand>("bar")
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<IRandomService, RandomService>();
                        services.AddSingleton<IDeterministicService, DeterministicService>();
                    });
        }
    }
}


