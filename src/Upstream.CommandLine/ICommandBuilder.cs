using System;

namespace Upstream.CommandLine
{
    /// <summary>
    /// Used to modify or extend the configuration of a command
    /// </summary>
    public interface ICommandBuilder
    {
        ICommandBuilder AddSubCommand<THandler, TCommand>()
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class;

        ICommandBuilder AddSubCommand<THandler, TCommand>(Action<ICommandBuilder>? builderAction)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class;

        ICommandBuilder AddSubCommand<THandler, TCommand>(string name, string? description = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class;

        ICommandBuilder AddSubCommand<THandler, TCommand>(Type type, string name, string? description = null,
            Action<ICommandBuilder>? builderAction = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class;
    }
}