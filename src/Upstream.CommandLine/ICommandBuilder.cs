using System;

namespace Upstream.CommandLine
{
    /// <summary>
    /// Used to modify or extend the configuration of a command
    /// </summary>
    public interface ICommandBuilder
    {
        ICommandBuilder AddCommandGroup(string name, Action<ICommandBuilder> builderAction);
        ICommandBuilder AddCommandGroup(string name, string? description, Action<ICommandBuilder> builderAction);

        ICommandBuilder AddCommand<THandler, TCommand>()
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class;

        ICommandBuilder AddCommand<THandler, TCommand>(string name, string? description = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class;
    }
}