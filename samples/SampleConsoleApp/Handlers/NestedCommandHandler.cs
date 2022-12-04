using System;
using System.Threading;
using System.Threading.Tasks;
using Upstream.CommandLine;

namespace SampleConsoleApp.Handlers;

public class NestedCommandHandler : ICommandHandler<NestedCommandHandler.WidgetCommand>,
    ICommandHandler<NestedCommandHandler.WumboCommand>
{
    [Command("widget")]
    public class WidgetCommand
    {
    }

    [Command("wumbo")]
    public class WumboCommand
    {
        [Option("-g", "--good", "--is-good")]
        public bool IsGood { get; init; }
    }

    public Task<int> ExecuteAsync(WidgetCommand options, CancellationToken cancellationToken)
    {
        Console.WriteLine("Widget!");

        return Task.FromResult(0);
    }

    public Task<int> ExecuteAsync(WumboCommand options, CancellationToken cancellationToken)
    {
        throw new SampleException($"Wumbo! {options.IsGood}");
    }
}