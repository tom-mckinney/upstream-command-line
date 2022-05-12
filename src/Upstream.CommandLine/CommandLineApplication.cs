using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Upstream.CommandLine.Utilities;

namespace Upstream.CommandLine
{
    public class CommandLineApplication : ICommandBuilder
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly CommandLineBuilder _builder;

        public CommandLineApplication()
        {
            _builder = new CommandLineBuilder() // UseDefaults() except with custom exception handler
                .UseVersionOption()
                .UseHelp()
                .UseEnvironmentVariableDirective()
                .UseParseDirective()
                .UseSuggestDirective()
                .RegisterWithDotnetSuggest()
                .UseTypoCorrections()
                .UseParseErrorReporting()
                .CancelOnProcessTermination();
        }
        
        internal IServiceProvider? ServiceProvider { get; set; }
        
        internal Command? CurrentCommand { get; set; }

        /// <summary>
        /// Allows direct configuration of the underlying <see cref="CommandLineBuilder"/>
        /// </summary>
        /// <remarks>
        /// It is uncommon to have to interact with this class directly
        /// </remarks>
        public CommandLineBuilder Builder => _builder;

        /// <summary>
        /// Configure additional services to be used at runtime via the <see cref="IServiceProvider"/>
        /// </summary>
        /// <remarks>
        /// All commands are added to the service collection to allow the use of dependency injection
        /// </remarks>
        public CommandLineApplication ConfigureServices(Action<IServiceCollection> configureServices)
        {
            configureServices(_services);

            return this;
        }

        /// <summary>
        /// Adds middleware to the application. Can be used to short-circuit a command or alter the <see cref="ParseResult"/>.
        /// </summary>
        /// <remarks>
        /// This is the same mechanism used by <see cref="UseExceptionHandler"/>
        /// </remarks>
        public CommandLineApplication AddMiddleware(InvocationMiddleware middleware, MiddlewareOrder order = MiddlewareOrder.Default)
        {
            _builder.AddMiddleware(middleware, order);

            return this;
        }

        /// <summary>
        /// Adds <paramref name="exceptionHandler"/> as middleware. If a <see cref="TargetInvocationException"/>
        /// is thrown (default <c>System.CommandLine</c> exception), it will be invoked with the inner exception.
        /// </summary>
        /// <param name="exceptionHandler">Exception Handler</param>
        /// <returns>Exit Code</returns>
        public CommandLineApplication UseExceptionHandler(Func<Exception, int> exceptionHandler)
        {
            _builder.AddMiddleware(async (context, next) =>
            {
                try
                {
                    await next(context);
                }
                catch (TargetInvocationException invocationException)
                {
                    context.ExitCode = exceptionHandler(invocationException.InnerException ?? invocationException);
                }
                catch (Exception e)
                {
                    context.ExitCode = exceptionHandler(e);
                }
            }, MiddlewareOrder.ExceptionHandler);

            return this;
        }

        /// <summary>
        /// Used by <see cref="InvokeAsync"/> to build the <see cref="ServiceProvider"/>
        /// and application <see cref="Parser"/>.
        /// </summary>
        /// <remarks>
        /// Declared as <c>internal</c> for testing purposes
        /// </remarks>
        [MemberNotNull(nameof(ServiceProvider))]
        internal Parser Build()
        {
            ServiceProvider = _services.BuildServiceProvider();

            return _builder.Build();
        }

        /// <summary>
        /// Builds the command line application and invokes it with the <paramref name="args"/>
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Exit Code</returns>
        public Task<int> InvokeAsync(string[] args)
        {
            return Build().InvokeAsync(args);
        }

        public CommandLineApplication AddCommand<THandler, TCommand>()
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            return AddCommand<THandler, TCommand>(null);
        }
        
        public CommandLineApplication AddCommand<THandler, TCommand>(Action<ICommandBuilder>? builderActions)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            var type = typeof(TCommand);

            var (name, description) = AttributeDeconstructor.GetCommandInfo(type);

            return AddCommand<THandler, TCommand>(type, name, description, builderActions);
        }

        public CommandLineApplication AddCommand<THandler, TCommand>(string name, string? description = null,
            Action<ICommandBuilder>? builderActions = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            return AddCommand<THandler, TCommand>(typeof(TCommand), name, description, builderActions);
        }

        public CommandLineApplication AddCommand<THandler, TCommand>(Type type, string name, string? description = null,
            Action<ICommandBuilder>? builderActions = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            _services.AddSingleton<THandler>();

            CurrentCommand = CreateCommand<THandler, TCommand>(
                type,
                name,
                description);

            _builder.Command.AddCommand(CurrentCommand);

            builderActions?.Invoke(this);

            return this;
        }

        public CommandLineApplication AddCommand(Command command)
        {
            _builder.Command.AddCommand(command);

            return this;
        }
        
        public ICommandBuilder AddSubCommand<THandler, TCommand>()
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            return AddSubCommand<THandler, TCommand>(null);
        }
        
        public ICommandBuilder AddSubCommand<THandler, TCommand>(Action<ICommandBuilder>? builderAction)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            var type = typeof(TCommand);

            var (name, description) = AttributeDeconstructor.GetCommandInfo(type);
            
            return AddSubCommand<THandler, TCommand>(type, name, description, builderAction);
        }
        
        public ICommandBuilder AddSubCommand<THandler, TCommand>(string name, string? description = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            return AddSubCommand<THandler, TCommand>(typeof(TCommand), name, description);
        }
        
        public ICommandBuilder AddSubCommand<THandler, TCommand>(Type type, string name, string? description = null, Action<ICommandBuilder>? builderAction = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            _services.AddSingleton<THandler>();

            var subCommand = CreateCommand<THandler, TCommand>(type, name, description);

            CurrentCommand?.AddCommand(subCommand);

            if (builderAction != null)
            {
                var tempCurrentCommand = CurrentCommand; // must be set back to original current command
                CurrentCommand = subCommand;
                
                builderAction.Invoke(this);

                CurrentCommand = tempCurrentCommand;
            }

            return this;
        }
        
        internal Command CreateCommand<THandler, TCommand>(Type type, string name, string? description = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            var command = new Command(name, description)
            {
                Handler = CommandHandler.Create<TCommand, CancellationToken>((command, cancellationToken) =>
                {
                    var internalHandler = ServiceProvider?.GetRequiredService<THandler>()
                                          ?? throw new InvalidOperationException(
                                              "Command was invoked without building ServiceProvider");

                    return internalHandler.ExecuteAsync(command, cancellationToken);
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

            return command;
        }
    }
}