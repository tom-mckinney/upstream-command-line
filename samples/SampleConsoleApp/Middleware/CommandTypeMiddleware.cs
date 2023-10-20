using System;
using System.Threading;
using System.Threading.Tasks;
using Upstream.CommandLine;

namespace SampleConsoleApp.Middleware;

public class CommandTypeMiddleware : ICommandHandlerMiddleware
{
    public async Task InvokeAsync<TCommand>(TCommand command, Func<TCommand, Task> next, CancellationToken cancellationToken)
        where TCommand : class
    {
        Console.WriteLine($"Before executing command: {command.GetType().Name}");

        await next(command);
        
        Console.WriteLine($"After executing command: {command.GetType().Name}");
    }
}