using System.Threading;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    public interface ICommandHandler<in TCommand>
        where TCommand : class
    {
        Task<int> ExecuteAsync(TCommand options, CancellationToken cancellationToken);
    }
}
