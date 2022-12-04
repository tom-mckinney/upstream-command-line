using System;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
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
            return CommandHandler.Create<TCommand, CancellationToken>((command, cancellationToken) =>
            {
                var internalHandler = getServiceProvider().GetRequiredService<THandler>();

                return internalHandler.ExecuteAsync(command, cancellationToken);
            });
        }
    }
}