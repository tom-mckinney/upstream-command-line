using Microsoft.Extensions.DependencyInjection;
using SampleConsoleApp.Services;
using System;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Threading.Tasks;
using SampleConsoleApp.Handlers;
using SampleConsoleApp.Middleware;
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
            return new CommandLineApplication("sample", "This is a sample application")
                .AddCommand<FooCommandHandler, FooCommand>()
                .AddCommand<BarCommandHandler, BarCommand>("bar")
                .AddCommand<RecordCommandHandler, RecordCommandHandler.RecordCommand>()
                .AddCommandGroup("gizmo", builder =>
                {
                    builder.AddCommandGroup("gadget", builder =>
                    {
                        builder.AddCommand<NestedCommandHandler, NestedCommandHandler.WidgetCommand>();
                    });
                    builder.AddCommand<NestedCommandHandler, NestedCommandHandler.WumboCommand>();
                })
                .AddMiddleware<BlueInvocationMiddleware>(MiddlewareOrder.Configuration)
                .AddMiddleware<RedInvocationMiddleware>(MiddlewareOrder.ErrorReporting)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IRandomService, RandomService>();
                    services.AddSingleton<IDeterministicService, DeterministicService>();
                    services.AddSingleton<ICommandHandlerMiddleware, CommandTypeMiddleware>();
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
                });
        }
    }
}