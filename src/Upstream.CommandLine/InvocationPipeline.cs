using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    internal class InvocationPipeline<THandler, TCommand>
        where THandler : class, ICommandHandler<TCommand>
        where TCommand : class
    {
        private readonly CommandHandlerMiddleware<TCommand> _invocationPipeline;
        
        public InvocationPipeline(THandler commandHandler, ICollection<ICommandHandlerMiddleware>? commandMiddlewares)
        {
            if (commandMiddlewares?.Any() != true) // short circuit if no command middleware has been configured
            {
                _invocationPipeline = async (command, _, cancellationToken) =>
                {
                    ExitCode = await commandHandler.ExecuteAsync(command, cancellationToken);
                };
                return;
            }
            
            var invocations = new List<CommandHandlerMiddleware<TCommand>>(commandMiddlewares.Count + 1);
            
            invocations.AddRange(
                commandMiddlewares.Select<ICommandHandlerMiddleware, CommandHandlerMiddleware<TCommand>>(m =>
                    async (command, next, cancellationToken) => await m.InvokeAsync(command, next, cancellationToken)));

            invocations.Add(async (command, _, cancellationToken) =>
            {
                ExitCode = await commandHandler.ExecuteAsync(command, cancellationToken);
            });
                
            _invocationPipeline = invocations.Aggregate(
                (first, second) =>
                    (command, next, cancellationToken) =>
                        first(command, c => second(c, next, cancellationToken), cancellationToken));
        }

        public int ExitCode { get; private set; } = -1;

        public async Task<int> InvokeAsync(TCommand command, CancellationToken cancellationToken)
        {
            await _invocationPipeline.Invoke(command, _ => Task.CompletedTask, cancellationToken);

            return ExitCode;
        }
    }
}