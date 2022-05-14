using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Upstream.CommandLine;

namespace SampleConsoleApp.Middleware;

public class BlueMiddleware : ICommandMiddleware
{
    public async Task InvokeAsync(InvocationContext context, Func<InvocationContext, Task> next)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("blue");
        Console.ResetColor();

        await next(context);
        
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("blue");
        Console.ResetColor();
    }
}