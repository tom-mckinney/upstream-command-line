using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    /// <summary>
    /// Classes that implement <see cref="ICommandInvocationMiddleware"/> will be invoked as part of application invocation lifecycle.
    /// This interface differs from <seealso cref="ICommandHandlerMiddleware"/> in that it is executed as part of
    /// paring and model binding, and is invoked prior to binding the parse result to a Command class. This can be useful when
    /// modify the <seealso cref="InvocationContext"/> and parse result.
    /// </summary>
    public interface ICommandInvocationMiddleware
    {
        Task InvokeAsync(InvocationContext context, Func<InvocationContext, Task> next);
    }
}