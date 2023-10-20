using System;
using System.Threading;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    /// <summary>
    /// Classes that implement <see cref="ICommandHandlerMiddleware"/> will be invoked as part of command handling.
    /// This interface differs from <seealso cref="ICommandInvocationMiddleware"/> in that it is executed after the parse result
    /// is bound to the appropriate Command class.
    /// </summary>
    /// <remarks>
    /// To perform a task for only a particular type of Command, use pattern matching within <see cref="InvokeAsync{TCommand}"/>:
    /// <c>
    /// if (command is TestCommand testCommand)
    /// </c>
    /// </remarks>
    public interface ICommandHandlerMiddleware
    {
        Task InvokeAsync<TCommand>(TCommand command, Func<TCommand, Task> next, CancellationToken cancellationToken)
            where TCommand : class;
    }
}