using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Upstream.CommandLine.Utilities;

namespace Upstream.CommandLine
{
    public class CommandLineApplication
    {
        private readonly CommandLineBuilder _builder;
        private readonly IServiceCollection _services = new ServiceCollection();

        public CommandLineApplication()
        {
            _builder = new CommandLineBuilder()
                .UseDefaults();
        }

        public IServiceProvider? ServiceProvider { get; private set; }

        public CommandLineApplication ConfigureServices(Action<IServiceCollection> configureServices)
        {
            configureServices(_services);

            return this;
        }

        [MemberNotNull(nameof(ServiceProvider))]
        public Task<int> InvokeAsync(string[] args)
        {
            ServiceProvider = _services.BuildServiceProvider();

            return _builder.Build().InvokeAsync(args);
        }

        public CommandLineApplication AddCommand<TAction, TOptions>(string name, string? description = null)
            where TAction : class, ICommandAction<TOptions>
            where TOptions : class
        {
            _services.AddScoped<TAction>();

            var command = new System.CommandLine.Command(name, description)
            {
                Handler = CommandHandler.Create<TOptions, CancellationToken>((options, cancellationToken) =>
                {
                    if (ServiceProvider == null)
                    {
                        throw new InvalidOperationException("Command was invoked without building ServiceProvider");
                    }

                    var action = ServiceProvider.GetRequiredService<TAction>();

                    return action.InvokeAsync(options, cancellationToken);
                })
            };

            foreach (var symbol in AttributeDeconstructor.GetSymbols(typeof(TOptions)))
            {
                command.Add(symbol);
            }

            _builder.AddCommand(command);

            return this;
        }

        public CommandLineApplication AddCommand(System.CommandLine.Command command)
        {
            _builder.AddCommand(command);

            return this;
        }
    }
}
