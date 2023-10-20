using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Upstream.CommandLine
{
    public static class ServiceBinderHandler
    {
        public static ICommandHandler Create<THandler, TCommand>(Func<IServiceProvider> getServiceProvider)
            where THandler : class, ICommandHandler<TCommand>
            where TCommand : class
        {
            return CommandHandler.Create<TCommand, CancellationToken>(async (command, cancellationToken) =>
            {
                var serviceProvider = getServiceProvider();
                var handler = serviceProvider.GetRequiredService<THandler>();
                var commandMiddlewares = serviceProvider.GetService<IEnumerable<ICommandHandlerMiddleware>>();

                return await new InvocationPipeline<THandler, TCommand>(handler, commandMiddlewares?.ToArray())
                    .InvokeAsync(command, cancellationToken);
            });
        }
    }
}