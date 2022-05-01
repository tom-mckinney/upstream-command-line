using System.Threading;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : class
    {
        public Task<int> InvokeAsync(TCommand command, CancellationToken cancellationToken)
        { 
            return ExecuteAsync(command, cancellationToken);
        }

        protected abstract Task<int> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
    }
}
