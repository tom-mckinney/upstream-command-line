using BenchmarkDotNet.Attributes;
using System.CommandLine;
using System.CommandLine.Builder;
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
        public async Task<int> CommandLineApplication()
        {
            var application = new CommandLineApplication()
                .AddCommand<FooHandler, FooHandler.FooCommand>();

            return await application.InvokeAsync(_args);
        }
        
        [Benchmark]
        public async Task<int> CommandLineApplication_no_Reflection()
        {
            var application = new CommandLineApplication()
                .AddCommand(new Command("foo")
                {
                    new Option<string>(new[] { "-b", "--bar" }),
                });

            return await application.InvokeAsync(_args);
        }

        [Benchmark]
        public async Task<int> VanillaSystemCommandLine()
        {
            var builder = new CommandLineBuilder()
                .UseDefaults();

            var fooCommand = new Command("foo")
            {
                new Option<string>(new[] { "-b", "--bar" }),
            };

            fooCommand.Handler = System.CommandLine.NamingConventionBinder.CommandHandler.Create<FooHandler.FooCommand, CancellationToken>((command, cancellationToken) =>
            {
                return Task.FromResult(command.Bar.GetHashCode());
            });
            
            builder.Command.AddCommand(fooCommand);

            return await builder.Build().InvokeAsync(_args);
        }

        [Benchmark]
        public async Task<int> SpectreCli()
        {
            var app = new Spectre.Console.Cli.CommandApp();

            app.Configure(config =>
            {
                config.AddCommand<SpectreFooCommand>("foo");
            });

            return await app.RunAsync(_args);
        }


        public class FooHandler : CommandHandler<FooHandler.FooCommand>
        {
            [Command("foo")]
            public class FooCommand
            {
                [Option("-b", "--bar", IsRequired = true)]
                public string Bar { get; set; }
            }
            
            protected override Task<int> ExecuteAsync(FooCommand command, CancellationToken cancellationToken)
            {
                return Task.FromResult(command.Bar.GetHashCode());
            }
        }

        public class SpectreFooCommand : Spectre.Console.Cli.Command<SpectreFooCommand.Settings>
        {
            public class Settings : Spectre.Console.Cli.CommandSettings
            {
                [Spectre.Console.Cli.CommandOption("-b|--bar")]
                public string Bar { get; set; }
            }

            public override int Execute([NotNull] Spectre.Console.Cli.CommandContext context, [NotNull] Settings settings)
            {
                return settings.Bar.GetHashCode();
            }
        }
    }
}
