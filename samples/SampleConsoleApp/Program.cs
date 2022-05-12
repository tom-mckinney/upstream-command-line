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
            catch (SampleException e)
            {
                Console.WriteLine($"Exception caught!\n{e.Message}");
            }
            finally
            {
                Stopwatch.Stop();

                Console.WriteLine($"Elapsed time: {Stopwatch.ElapsedMilliseconds}ms");
            }
        }

        public static CommandLineApplication BuildCommandLineApplication()
        {
            return new CommandLineApplication()
                .AddCommand<FooCommandHandler, FooCommand>()
                .AddCommand<BarCommandHandler, BarCommand>("bar")
                .AddCommand<NestedCommandHandler, NestedCommandHandler.GizmoCommand>(builder =>
                {
                    builder.AddSubCommand<NestedCommandHandler, NestedCommandHandler.GadgetCommand>(builder =>
                    {
                        builder.AddSubCommand<NestedCommandHandler, NestedCommandHandler.WidgetCommand>();
                    });
                    builder.AddSubCommand<NestedCommandHandler, NestedCommandHandler.WumboCommand>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IRandomService, RandomService>();
                    services.AddSingleton<IDeterministicService, DeterministicService>();
                })
                .UseExceptionHandler(e =>
                {
                    if (e is SampleException)
                    {
                        Console.WriteLine($"An expected exception occured: {e.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"HELLO?: {e.Message} {e.GetType().Name} {e.InnerException?.Message}");
                    }

                    return 1;
                });
        }
    }
}