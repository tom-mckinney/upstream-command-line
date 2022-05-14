using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Upstream.CommandLine;

namespace SampleConsoleApp.Middleware;

public class RedMiddleware : ICommandMiddleware
{
    public async Task InvokeAsync(InvocationContext context, Func<InvocationContext, Task> next)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("red");
        Console.ResetColor();
        
        await next(context);
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("red");
        Console.ResetColor();
    }
}