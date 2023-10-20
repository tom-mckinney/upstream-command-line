﻿using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Upstream.CommandLine
{
    /// <summary>
    /// Entrypoint for a Command Line Application.
    /// Can be used to configure commands, services, and middleware.
    /// </summary>
    /// <example>
    /// <code>
    /// await new CommandLineApplication()
    ///     .AddCommand&lt;HelloHandler, HelloCommand&gt;()
    ///     .InvokeAsync(args);
    /// </code>
    /// </example>
    public class CommandLineApplication
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly CommandBuilder _builder;

        /// <summary>
        /// Initializes an instance of <see cref="CommandLineApplication"/>
        /// </summary>
        /// <param name="applicationName">Name of the <see cref="RootCommand"/>. Will be displayed in help text.</param>
        /// <param name="description">Description of the <see cref="RootCommand"/>. Will be displayed in help text.</param>
        public CommandLineApplication(string? applicationName = null, string? description = null)
            : this(CreateRootCommand(applicationName, description))
        {
        }
        
        /// <summary>
        /// Initializes an instance of <see cref="CommandLineApplication"/> with a custom root command handler
        /// </summary>
        /// <param name="rootHandler">Root handler action</param>
        /// <param name="applicationName">Name of the <see cref="RootCommand"/>. Will be displayed in help text.</param>
        /// <param name="description">Description of the <see cref="RootCommand"/>. Will be displayed in help text.</param>
        public CommandLineApplication(Action rootHandler, string? applicationName = null, string? description = null)
            : this((_) => rootHandler(), applicationName, description)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandLineApplication"/> with a custom root command handler
        /// </summary>
        /// <param name="rootHandler">Root handler action</param>
        /// <param name="applicationName">Name of the <see cref="RootCommand"/>. Will be displayed in help text.</param>
        /// <param name="description">Description of the <see cref="RootCommand"/>. Will be displayed in help text.</param>
        public CommandLineApplication(Action<InvocationContext> rootHandler, string? applicationName = null, string? description = null)
            : this(CreateRootCommand(applicationName, description, rootHandler))
        {
        }
        
        private static Command CreateRootCommand(string? name = null, string? description = null,
            Action<InvocationContext>? handler = null)
        {
            var command = name is not null
                ? new Command(name, description)
                : new RootCommand(description ?? string.Empty); // will use executable filename by default


            if (handler is not null)
            {
                command.SetHandler(handler);
            }

            return command;
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandLineApplication"/>
        /// </summary>
        /// <param name="rootCommand"><see cref="Command"/></param>
        public CommandLineApplication(Command? rootCommand)
        {
            _builder = new CommandBuilder(_services, rootCommand);
        }

        /// <summary>
        /// Provides access to the <see cref="CommandBuilder"/>'s <see cref="IServiceProvider"/>
        /// </summary>
        /// <remarks>
        /// Declared as <c>internal</c> for testing purposes
        /// </remarks>
        /// <exception cref="InvalidOperationException">Throws if accessed before building the application</exception>
        internal IServiceProvider ServiceProvider => _builder.ServiceProvider;

        /// <summary>
        /// Allows direct configuration of the underlying <see cref="System.CommandLine.Builder.CommandLineBuilder"/>
        /// </summary>
        /// <remarks>
        /// It is uncommon to have to interact with this class directly
        /// </remarks>
        public CommandLineBuilder CommandLineBuilder => _builder.CommandLineBuilder;

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

        public CommandLineApplication AddMiddleware<TMiddleware>()
            where TMiddleware : class, ICommandHandlerMiddleware
        {
            // this is pulled from ServiceProvider during handler instantiation
            _services.AddSingleton<ICommandHandlerMiddleware, TMiddleware>();
            
            return this;
        }

        /// <summary>
        /// Adds middleware to the application. Can be used to short-circuit a command or alter the <see cref="ParseResult"/>.
        /// </summary>
        /// <remarks>
        /// This is the same mechanism used by <see cref="UseExceptionHandler(System.Action{System.Exception})"/>
        /// </remarks>
        public CommandLineApplication AddMiddleware(InvocationMiddleware middleware,
            MiddlewareOrder order = MiddlewareOrder.Default)
        {
            _builder.AddMiddleware(middleware, order);

            return this;
        }

        /// <summary>
        /// Adds an implementation of <see cref="ICommandInvocationMiddleware"/> to the application that is
        /// invoked via dependency injection.
        /// </summary>
        /// <param name="order">Default position in the middleware pipeline</param>
        /// <typeparam name="TMiddleware">Middleware</typeparam>
        public CommandLineApplication AddMiddleware<TMiddleware>(MiddlewareOrder order = MiddlewareOrder.Default)
            where TMiddleware : class, ICommandInvocationMiddleware
        {
            _builder.AddMiddleware<TMiddleware>();

            return this;
        }

        /// <inheritdoc cref="AddMiddleware{TMiddleware}"/>
        /// <typeparam name="TMiddleware">Middleware</typeparam>
        /// <typeparam name="TImplementation">Class implementing <c>TMiddleware</c></typeparam>
        public CommandLineApplication AddMiddleware<TMiddleware, TImplementation>(
            MiddlewareOrder order = MiddlewareOrder.Default)
            where TMiddleware : class, ICommandInvocationMiddleware
            where TImplementation : class, TMiddleware
        {
            _builder.AddMiddleware<TMiddleware, TImplementation>();

            return this;
        }

        /// <summary>
        /// Adds <paramref name="exceptionHandler"/> as middleware. If a <see cref="TargetInvocationException"/>
        /// is thrown (default <c>System.CommandLine</c> exception), it will be invoked with the inner exception.
        /// </summary>
        /// <param name="exceptionHandler">Exception Handler</param>
        public CommandLineApplication UseExceptionHandler(Action<Exception> exceptionHandler)
        {
            return UseExceptionHandler((e, _) => exceptionHandler(e));
        }

        /// <inheritdoc cref="UseExceptionHandler(System.Action{System.Exception})"/>
        public CommandLineApplication UseExceptionHandler(Action<Exception, InvocationContext> exceptionHandler)
        {
            return UseExceptionHandler((exception, context) =>
            {
                exceptionHandler(exception, context);

                return Task.CompletedTask;
            });
        }

        /// <inheritdoc cref="UseExceptionHandler(System.Action{System.Exception})"/>
        public CommandLineApplication UseExceptionHandler(Func<Exception, Task> exceptionHandler)
        {
            return UseExceptionHandler((e, _) => exceptionHandler(e));
        }

        /// <inheritdoc cref="UseExceptionHandler(System.Action{System.Exception})"/>
        public CommandLineApplication UseExceptionHandler(Func<Exception, InvocationContext, Task> exceptionHandler)
        {
            _builder.AddMiddleware(async (context, next) =>
            {
                try
                {
                    await next(context);
                }
                catch (TargetInvocationException invocationException)
                {
                    await exceptionHandler(invocationException.InnerException ?? invocationException, context);

                    context.ExitCode = 1;
                }
                catch (Exception e)
                {
                    await exceptionHandler(e, context);

                    context.ExitCode = 1;
                }
            }, MiddlewareOrder.ExceptionHandler);

            return this;
        }

        /// <summary>
        /// Used by <see cref="InvokeAsync"/> to build the underlying <see cref="CommandBuilder"/>
        /// </summary>
        /// <remarks>
        /// Declared as <c>internal</c> for testing purposes
        /// </remarks>
        internal Parser Build()
        {
            return _builder.Build();
        }

        /// <summary>
        /// Builds the command line application and invokes it with the <paramref name="args"/>
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="console"><see cref="IConsole"/></param>
        /// <returns>Exit Code</returns>
        public Task<int> InvokeAsync(string[] args, IConsole? console = null)
        {
            return Build().InvokeAsync(args, console);
        }

        /// <summary>
        /// Adds a top-level command to the application
        /// </summary>
        /// <typeparam name="THandler">Handler that implements <see cref="ICommandHandler{TCommand}"/></typeparam>
        /// <typeparam name="TCommand">Class representation of command arguments and options</typeparam>
        /// <returns>Root <see cref="CommandLineApplication"/></returns>
        /// <remarks>
        /// Requires that <typeparamref name="TCommand"/> is decorated with
        /// a <see cref="CommandAttribute"/> to infer the name and description
        /// </remarks>
        public CommandLineApplication AddCommand<THandler, TCommand>()
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            _ = _builder.AddCommand<THandler, TCommand>();

            return this;
        }

        /// <inheritdoc cref="AddCommand{THandler,TCommand}()" />
        /// <param name="name">Name of the command</param>
        /// <param name="description">Description of the command</param>
        /// <remarks>
        /// Uses <paramref name="name"/> and <paramref name="description"/>
        /// to define the command's name and description
        /// </remarks>
        public CommandLineApplication AddCommand<THandler, TCommand>(string name, string? description = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            _ = _builder.AddCommand<THandler, TCommand>(name, description);

            return this;
        }

        /// <summary>
        /// Directly adds a <see cref="Command"/> that must be initialized manually
        /// </summary>
        /// <param name="command"><c>System.CommandLine</c> Command</param>
        /// <returns>Root <see cref="CommandLineApplication"/></returns>
        /// <remarks>
        /// Can be useful if the <see cref="CommandLineApplication"/> does not expose
        /// a feature of the underlying <c>System.CommandLine</c> API
        /// </remarks>
        public CommandLineApplication AddCommand(Command command)
        {
            _ = _builder.AddCommand(command);

            return this;
        }

        /// <summary>
        /// Adds a command group that can be used to organize commands
        /// under a common namespace
        /// </summary>
        /// <param name="name">Name of the group</param>
        /// <param name="builderAction">Action to configure subcommands and subgroups</param>
        /// <returns>Root <see cref="CommandLineApplication"/></returns>
        public CommandLineApplication AddCommandGroup(string name, Action<ICommandBuilder> builderAction)
        {
            return AddCommandGroup(name, null, builderAction, null);
        }

        /// <inheritdoc cref="AddCommandGroup(string,System.Action{Upstream.CommandLine.ICommandBuilder})" />
        /// <param name="name">Name of the group</param>
        /// <param name="description">Description of the group</param>
        /// <param name="builderAction">Action to configure subcommands and subgroups</param>
        public CommandLineApplication AddCommandGroup(string name, string? description,
            Action<ICommandBuilder> builderAction)
        {
            return AddCommandGroup(name, description, builderAction, null);
        }

        /// <inheritdoc cref="AddCommandGroup(string,System.Action{Upstream.CommandLine.ICommandBuilder})"/>
        /// <typeparam name="THandler">Handler that implements <see cref="ICommandHandler{TCommand}"/></typeparam>
        /// <typeparam name="TCommand">Command class</typeparam>
        /// <returns></returns>
        public CommandLineApplication AddCommandGroup<THandler, TCommand>(string name,
            Action<ICommandBuilder> builderAction)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            return AddCommandGroup<THandler, TCommand>(name, null, builderAction);
        }

        /// <inheritdoc cref="AddCommandGroup(string,System.Action{Upstream.CommandLine.ICommandBuilder})"/>
        /// <param name="description">Description for the command group</param>
        /// <typeparam name="THandler">Handler that implements <see cref="ICommandHandler{TCommand}"/></typeparam>
        /// <typeparam name="TCommand">Command class</typeparam>
        /// <returns></returns>
        public CommandLineApplication AddCommandGroup<THandler, TCommand>(string name, string? description,
            Action<ICommandBuilder> builderAction)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            _ = _builder.AddCommandGroup<THandler, TCommand>(name, description, builderAction);

            return this;
        }

        /// <inheritdoc cref="AddCommandGroup(string,System.Action{Upstream.CommandLine.ICommandBuilder})"/>
        /// <param name="description">Description for the command group</param>
        /// <param name="commandGroupHandler">Optional handler for the command group</param>
        /// <returns></returns>
        public CommandLineApplication AddCommandGroup(
            string name,
            string? description,
            Action<ICommandBuilder> builderAction,
            ICommandHandler? commandGroupHandler
        )
        {
            _ = _builder.AddCommandGroup(name, description, builderAction, commandGroupHandler);

            return this;
        }
    }
}