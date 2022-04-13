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

        public static Task Main(string[] args)
        {
            Stopwatch.Start();

            return new CommandLineApplication()
                .AddCommand<FooCommand, FooOptions>("foo")
                .AddCommand<BarCommand, BarOptions>("bar")
                .ConfigureServices(services =>
                {
                    services.AddTransient<IRandomService, RandomService>();
                    services.AddTransient<IDeterministicService, DeterministicService>();
                })
                .InvokeAsync(args);
        }
    }
}
