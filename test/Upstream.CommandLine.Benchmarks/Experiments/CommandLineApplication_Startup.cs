using BenchmarkDotNet.Attributes;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace Upstream.CommandLine.Benchmarks.Experiments
{
    [MemoryDiagnoser]
    public class CommandLineApplication_Startup
    {
        private readonly string[] _args;

        readonly string[] OptionVariants = new[] { "-b", "--bar" };

        readonly string[] ValueVariants = new[]
        {
            "foo",
            "bar",
            "baz",
            "wumbo",
        };

        public CommandLineApplication_Startup()
        {
            var random = new Random(420);

            _args = new[]
            {
                "foo",
                OptionVariants[random.Next(0, OptionVariants.Length)],
                ValueVariants[random.Next(0, ValueVariants.Length)],
            };
        }

        [Benchmark]
        public async Task<int> Upstream_Default()
        {
            var application = new CommandLineApplication()
                .AddCommand<FooHandler, FooHandler.FooCommand>();

            return await application.InvokeAsync(_args);
        }
        
        [Benchmark]
        public async Task<int> Upstream_simple_middleware()
        {
            var application = new CommandLineApplication()
                .AddCommand<FooHandler, FooHandler.FooCommand>()
                .AddMiddleware(async (context, next) =>
                {
                    await next(context);
                });

            return await application.InvokeAsync(_args);
        }
        
        [Benchmark]
        public async Task<int> Upstream_dependency_injection_middleware()
        {
            var application = new CommandLineApplication()
                .AddCommand<FooHandler, FooHandler.FooCommand>()
                .AddMiddleware<UselessInvocationMiddleware>();

            return await application.InvokeAsync(_args);
        }
        
        [Benchmark]
        public async Task<int> Upstream_no_CommandAttribute()
        {
            var application = new CommandLineApplication()
                .AddCommand<FooHandler, FooHandler.FooCommand>("foo");

            return await application.InvokeAsync(_args);
        }

        [Benchmark]
        public async Task<int> Upstream_no_Reflection()
        {
            var barOption = new Option<string>(new[] { "-b", "--bar" }); 
            var command = new Command("foo")
            {
                barOption,
            };

            command.SetHandler((string bar) => Task.FromResult(bar.GetHashCode()), barOption);

            var application = new CommandLineApplication()
                .AddCommand(command);

            return await application.InvokeAsync(_args);
        }

        [Benchmark]
        public async Task<int> Upstream_NamingConventionBinder_no_Reflection()
        {
            var command = new Command("foo")
            {
                new Option<string>(new[] { "-b", "--bar" }),
            };

            command.Handler =
                CommandHandler.Create<FooHandler.FooCommand, CancellationToken>((foo, cancellationToken) => Task.FromResult(foo.Bar.GetHashCode()));

            var application = new CommandLineApplication()
                .AddCommand(command);

            return await application.InvokeAsync(_args);
        }

        [Benchmark]
        public async Task<int> SystemCommandLine()
        {
            var builder = new CommandLineBuilder()
                .UseDefaults();

            var barOption = new Option<string>(new[] { "-b", "--bar" }); 
            var fooCommand = new Command("foo")
            {
                barOption,
            };

            fooCommand.SetHandler((string bar) => Task.FromResult(bar.GetHashCode()), barOption);

            builder.Command.AddCommand(fooCommand);

            return await builder.Build().InvokeAsync(_args);
        }
        
        [Benchmark]
        public async Task<int> SystemCommandLine_middleware()
        {
            var builder = new CommandLineBuilder()
                .UseDefaults();

            var barOption = new Option<string>(new[] { "-b", "--bar" }); 
            var fooCommand = new Command("foo")
            {
                barOption,
            };

            fooCommand.SetHandler((string bar) => Task.FromResult(bar.GetHashCode()), barOption);

            builder.Command.AddCommand(fooCommand);

            builder.AddMiddleware(async (context, next) =>
            {
                await next(context);
            });

            return await builder.Build().InvokeAsync(_args);
        }

        [Benchmark]
        public async Task<int> SystemCommandLine_NamingConventionBinder()
        {
            var builder = new CommandLineBuilder()
                .UseDefaults();

            var fooCommand = new Command("foo")
            {
                new Option<string>(new[] { "-b", "--bar" }),
            };

            fooCommand.Handler = CommandHandler.Create<FooHandler.FooCommand, CancellationToken>(
                (command, cancellationToken) => Task.FromResult(command.Bar.GetHashCode()));

            builder.Command.AddCommand(fooCommand);

            return await builder.Build().InvokeAsync(_args);
        }

        [Benchmark]
        public async Task<int> SpectreCli()
        {
            var app = new Spectre.Console.Cli.CommandApp();

            app.Configure(config => { config.AddCommand<SpectreFooCommand>("foo"); });

            return await app.RunAsync(_args);
        }


        public class FooHandler : ICommandHandler<FooHandler.FooCommand>
        {
            [Command("foo")]
            public class FooCommand
            {
                [Option("-b", "--bar", IsRequired = true)]
                public string Bar { get; set; } = null!;
            }

            public Task<int> ExecuteAsync(FooCommand command, CancellationToken cancellationToken)
            {
                return Task.FromResult(command.Bar.GetHashCode());
            }
        }
        
        public class UselessInvocationMiddleware : ICommandInvocationMiddleware
        {
            public async Task InvokeAsync(InvocationContext context, Func<InvocationContext, Task> next)
            {
                await next(context);
            }
        }

        public class SpectreFooCommand : Spectre.Console.Cli.Command<SpectreFooCommand.Settings>
        {
            public class Settings : Spectre.Console.Cli.CommandSettings
            {
                [Spectre.Console.Cli.CommandOption("-b|--bar")]
                public string Bar { get; set; } = null!;
            }

            public override int Execute([NotNull] Spectre.Console.Cli.CommandContext context,
                [NotNull] Settings settings)
            {
                return settings.Bar.GetHashCode();
            }
        }
    }
}