using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    public abstract class CommandBase<TOptions> : ICommandAction<TOptions>
        where TOptions : class
    {
        protected TOptions Options { get; private set; } = null!;

        [MemberNotNull(nameof(Options))]
        public async Task InvokeAsync(TOptions options, CancellationToken cancellationToken)
        {
            Options = options;

            await BeforeExecuteAsync(cancellationToken);

            await ExecuteAsync(cancellationToken);

            await AfterExecuteAsync(cancellationToken);
        }

        protected virtual Task BeforeExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected abstract Task ExecuteAsync(CancellationToken cancellationToken);

        protected virtual Task AfterExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
