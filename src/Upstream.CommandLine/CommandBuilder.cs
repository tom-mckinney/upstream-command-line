﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Upstream.CommandLine.Utilities;

namespace Upstream.CommandLine
{
    public class CommandBuilder : ICommandBuilder
    {
        private readonly Stack<Command> _commandStack = new();
        private readonly IServiceCollection _services;
        private IServiceProvider? _serviceProvider;

        public CommandBuilder(IServiceCollection services, Command? rootCommand = null)
        {
            _services = services;
            CommandLineBuilder =
                new CommandLineBuilder(rootCommand) // UseDefaults() except with custom exception handler
                    .UseVersionOption()
                    .UseHelp()
                    .UseEnvironmentVariableDirective()
                    .UseParseDirective()
                    .UseSuggestDirective()
                    .RegisterWithDotnetSuggest()
                    .UseTypoCorrections()
                    .UseParseErrorReporting()
                    .CancelOnProcessTermination();

            _commandStack.Push(CommandLineBuilder.Command);
        }

        public CommandLineBuilder CommandLineBuilder { get; }

        public IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException(
            "Command was invoked without building ServiceProvider");

        [MemberNotNull(nameof(_serviceProvider))]
        internal Parser Build()
        {
            _serviceProvider = _services.BuildServiceProvider();

            return CommandLineBuilder.Build();
        }

        public void AddMiddleware(InvocationMiddleware middleware, MiddlewareOrder order = MiddlewareOrder.Default)
        {
            CommandLineBuilder.AddMiddleware(middleware, order);
        }

        public void AddMiddleware<TMiddleware>(MiddlewareOrder order = MiddlewareOrder.Default)
            where TMiddleware : class, ICommandMiddleware
        {
            _services.TryAddSingleton<TMiddleware>();

            CommandLineBuilder.AddMiddleware(
                (context, next) => ServiceProvider.GetRequiredService<TMiddleware>().InvokeAsync(context, next), order);
        }

        public void AddMiddleware<TMiddleware, TImplementation>(MiddlewareOrder order = MiddlewareOrder.Default)
            where TMiddleware : class, ICommandMiddleware
            where TImplementation : class, TMiddleware
        {
            _services.TryAddSingleton<TMiddleware, TImplementation>();

            CommandLineBuilder.AddMiddleware(
                (context, next) => ServiceProvider.GetRequiredService<TMiddleware>().InvokeAsync(context, next), order);
        }

        public ICommandBuilder AddCommandGroup(string name, Action<ICommandBuilder> builderAction)
        {
            return AddCommandGroup(name, null, builderAction, null);
        }

        public ICommandBuilder AddCommandGroup(string name, string? description, Action<ICommandBuilder> builderAction)
        {
            return AddCommandGroup(name, description, builderAction, null);
        }

        public ICommandBuilder AddCommandGroup<THandler, TCommand>(string name, string? description,
            Action<ICommandBuilder> builderAction)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            _services.AddSingleton<THandler>();

            return AddCommandGroup(name, description, builderAction,
                ServiceBinderHandler.Create<THandler, TCommand>(() => ServiceProvider));
        }

        public ICommandBuilder AddCommandGroup(
            string name,
            string? description,
            Action<ICommandBuilder> builderAction,
            ICommandHandler? commandGroupHandler
        )
        {
            var commandGroup = new Command(name, description)
            {
                Handler = commandGroupHandler
            };

            AddScopedCommand(commandGroup, () => builderAction(this));

            return this;
        }

        public ICommandBuilder AddCommand<THandler, TCommand>() where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            var type = typeof(TCommand);

            var (name, description) = AttributeDeconstructor.GetCommandInfo(type);

            return AddCommand<THandler, TCommand>(type, name, description);
        }

        public ICommandBuilder AddCommand<THandler, TCommand>(string name, string? description = null)
            where THandler : class, ICommandHandler<TCommand> where TCommand : class
        {
            return AddCommand<THandler, TCommand>(typeof(TCommand), name, description);
        }

        private ICommandBuilder AddCommand<THandler, TCommand>(Type type, string name, string? description = null)
            where THandler : class, ICommandHandler<TCommand> where TCommand : class
        {
            _services.AddSingleton<THandler>();

            var command = CreateCommand<THandler, TCommand>(
                type,
                name,
                description);

            AddScopedCommand(command, null);

            return this;
        }

        public ICommandBuilder AddCommand(Command command)
        {
            AddScopedCommand(command, null);

            return this;
        }

        private void AddScopedCommand(Command command, Action? builderAction)
        {
            _commandStack.Peek().AddCommand(command);

            if (builderAction is null)
            {
                return; // no need to add to command stack
            }

            _commandStack.Push(command);

            builderAction.Invoke();

            _ = _commandStack.Pop();
        }

        internal Command CreateCommand<THandler, TCommand>(Type type, string name, string? description = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            var command = new Command(name, description)
            {
                Handler = ServiceBinderHandler.Create<THandler, TCommand>(() => ServiceProvider),
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