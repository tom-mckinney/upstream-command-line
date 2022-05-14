using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    public interface ICommandMiddleware
    {
        Task InvokeAsync(InvocationContext context, Func<InvocationContext, Task> next);
    }
}