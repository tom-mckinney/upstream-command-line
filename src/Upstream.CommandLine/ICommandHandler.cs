using System.Threading;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    public interface ICommandHandler<in TCommand>
        where TCommand : class
    {
        Task<int> InvokeAsync(TCommand options, CancellationToken cancellationToken);
    }
}
