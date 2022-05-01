using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

        public CommandLineApplication AddCommand<THandler, TCommand>()
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            var type = typeof(TCommand);
            
            var (name, description) = AttributeDeconstructor.GetCommandInfo(type);

            return AddCommand<THandler, TCommand>(type, name, description);
        }

        public CommandLineApplication AddCommand<THandler, TCommand>(string name, string? description = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            return AddCommand<THandler, TCommand>(typeof(TCommand), name, description);
        }
        
        public CommandLineApplication AddCommand<THandler, TCommand>(Type type, string name, string? description = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            _services.AddSingleton<THandler>();

            var command = new Command(name, description)
            {
                Handler = CommandHandler.Create<TCommand, CancellationToken>((command, cancellationToken) =>
                {
                    if (ServiceProvider == null)
                    {
                        throw new InvalidOperationException("Command was invoked without building ServiceProvider");
                    }

                    var internalHandler = ServiceProvider.GetRequiredService<THandler>();

                    return internalHandler.InvokeAsync(command, cancellationToken);
                })
            };

            foreach (var symbol in AttributeDeconstructor.GetSymbols(type))
            {
                switch (symbol)
                {
                    case Argument argument:
                        command.Add(argument);
                        break;
                    case Option option:
                        command.Add(option);
                        break;
                }
            }

            _builder.Command.AddCommand(command);

            return this;
        }

        public CommandLineApplication AddCommand(Command command)
        {
            _builder.Command.AddCommand(command);

            return this;
        }
    }
}
