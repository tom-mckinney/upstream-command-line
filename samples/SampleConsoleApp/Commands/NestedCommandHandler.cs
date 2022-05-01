using System;
using System.Threading;
using System.Threading.Tasks;
using Upstream.CommandLine;

namespace SampleConsoleApp.Commands;

public class NestedCommandHandler : ICommandHandler<NestedCommandHandler.GizmoCommand>, 
    ICommandHandler<NestedCommandHandler.GadgetCommand>, 
    ICommandHandler<NestedCommandHandler.WidgetCommand>,
    ICommandHandler<NestedCommandHandler.WumboCommand>
{
    [Command("gizmo")]
    public class GizmoCommand
    {
    }

    [Command("gadget")]
    public class GadgetCommand
    {
    }
    
    [Command("widget")]
    public class WidgetCommand
    {
    }

    [Command("wumbo")]
    public class WumboCommand
    {
    }

    public Task<int> ExecuteAsync(GizmoCommand options, CancellationToken cancellationToken)
    {
        Console.WriteLine("Gizmo!");

        return Task.FromResult(0);
    }

    public Task<int> ExecuteAsync(GadgetCommand options, CancellationToken cancellationToken)
    {
        Console.WriteLine("Gadget!");

        return Task.FromResult(0);
    }
    
    public Task<int> ExecuteAsync(WidgetCommand options, CancellationToken cancellationToken)
    {
        Console.WriteLine("Widget!");

        return Task.FromResult(0);
    }

    public Task<int> ExecuteAsync(WumboCommand options, CancellationToken cancellationToken)
    {
        throw new SampleException("Wumbo!");
    }
}