using System;
using System.Threading;
using System.Threading.Tasks;
using Upstream.CommandLine;

namespace SampleConsoleApp.Handlers;

public class RecordCommandHandler : ICommandHandler<RecordCommandHandler.RecordCommand>
{
    [Command("custom-formats", "List custom formats in the guide for the specified service")]
    public record RecordCommand(
        [property:Argument(Description = "The service to list custom format for")]
        string Service,

        [property:Option("-a", "--another", Description = "Another argument")]
        string Another
    );


    public Task<int> ExecuteAsync(RecordCommand options, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Service: {options.Service} Another: {options.Another}");

        return Task.FromResult(0);
    }
}