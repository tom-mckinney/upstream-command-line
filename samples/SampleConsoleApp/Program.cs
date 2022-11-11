using Microsoft.Extensions.DependencyInjection;
using SampleConsoleApp.Commands;
using SampleConsoleApp.Services;
using System;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
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
            var hostBuilder = Host.CreateDefaultBuilder()
                // Use a custom factory here to integrate other DI libraries like Autofac
                .UseServiceProviderFactory(new DefaultServiceProviderFactory());

            return new CommandLineApplication(hostBuilder)
                .AddCommand<FooCommandHandler, FooCommand>()
                .AddCommand<BarCommandHandler, BarCommand>("bar")
                .AddCommandGroup("gizmo", builder =>
                {
                    builder.AddCommandGroup("gadget", builder =>
                    {
                        builder.AddCommand<NestedCommandHandler, NestedCommandHandler.WidgetCommand>();
                    });
                    builder.AddCommand<NestedCommandHandler, NestedCommandHandler.WumboCommand>();
                })
                .AddMiddleware<BlueMiddleware>(MiddlewareOrder.Configuration)
                .AddMiddleware<RedMiddleware>(MiddlewareOrder.ErrorReporting)
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
                });
        }
    }
}
