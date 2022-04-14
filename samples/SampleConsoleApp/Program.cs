using Microsoft.Extensions.DependencyInjection;
using SampleConsoleApp.Commands;
using SampleConsoleApp.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Upstream.CommandLine;

namespace SampleConsoleApp;

public static class Program
{
    public static Stopwatch Stopwatch { get; } = new Stopwatch();

    public static async Task Main(string[] args)
    {
        Stopwatch.Start();

        try
        {
            await new CommandLineApplication()
                .AddCommand<FooCommand, FooOptions>("foo")
                .AddCommand<BarCommand, BarOptions>("bar")
                .ConfigureServices(services =>
                {
                    services.AddTransient<IRandomService, RandomService>();
                    services.AddTransient<IDeterministicService, DeterministicService>();
                })
                .InvokeAsync(args);
        }
        finally
        {
            Stopwatch.Stop();

            Console.WriteLine($"Ellapsed time: {Program.Stopwatch.ElapsedMilliseconds}ms");
        }

    }
}

