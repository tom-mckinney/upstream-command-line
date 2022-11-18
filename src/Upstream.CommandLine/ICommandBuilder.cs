using System;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    /// <summary>
    /// Used to modify or extend the configuration of a command
    /// </summary>
    public interface ICommandBuilder
    {
        ICommandBuilder AddCommandGroup(string name, Action<ICommandBuilder> builderAction);

        ICommandBuilder AddCommandGroup(string name, string? description, Action<ICommandBuilder> builderAction);

        ICommandBuilder AddCommandGroup<THandler, TCommand>(string name, string? description,
            Action<ICommandBuilder> builderAction)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class;

        ICommandBuilder AddCommandGroup(string name, string? description, Action<ICommandBuilder> builderAction,
            ICommandHandler? commandGroupHandler);

        ICommandBuilder AddCommand<THandler, TCommand>()
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class;

        ICommandBuilder AddCommand<THandler, TCommand>(string name, string? description = null)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class;
    }
}