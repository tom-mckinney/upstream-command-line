using System;
using System.Threading;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    internal delegate Task CommandHandlerMiddleware<TCommand>(
        TCommand command,
        Func<TCommand, Task> next,
        CancellationToken cancellationToken);
}