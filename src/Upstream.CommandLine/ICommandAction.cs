using System.Threading;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    public interface ICommandAction<TOptions>
        where TOptions : class
    {
        Task InvokeAsync(TOptions options, CancellationToken cancellationToken);
    }
}
