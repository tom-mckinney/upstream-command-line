# upstream-command-line

[![nuget](https://img.shields.io/nuget/v/Upstream.CommandLine)](https://www.nuget.org/packages/Upstream.CommandLine/) ![Publish Package status](https://github.com/tom-mckinney/upstream-command-line/workflows/Publish%20Package/badge.svg?branch=master) ![Run Tests status](https://github.com/tom-mckinney/upstream-command-line/workflows/Run%20Tests/badge.svg?branch=master)

A wrapper around `System.CommandLine` to allow for large, service-oriented application development. Some of the benefits of this package include:

- **Native IOC support**: All commands are registered and instatiated via the [ServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.serviceprovider).
- **Attribute-based command declaration**: Commands can be configured via readable, statically typed classes&mdash;no more mapping classes to functional configuration methods.
- **Leverage System.CommandLine**: This is not a fork, but a flexible wrapper around the .NET Foundation sponsored [System.CommandLine](https://github.com/dotnet/command-line-api) library. This means that your application will benefit from the continued development/maturity of that project, and can benefit from integrations like `dotnet-suggest` out of the box. If you wish to switch to a vanilla System.CommandLine implementation down the road, the migration is straightforward.

## Setup

Add the package as a dependency to your .NET project via the following command:
```
dotnet add package Upstream.CommandLine
```

## Usage

Refer to the [Sample project](https://github.com/tom-mckinney/upstream-command-line/tree/main/src/Samples/SampleConsoleApp) for a working example of this package. The following is a simple (non-functional) example of how to configure an Upstream.CommandLine console application that uses dependency injected services:

```csharp
public static class Program
{
    public static Task Main(string[] args)
    {
        return new CommandLineApplication()
            .AddCommand<FooCommand, FooOptions>()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IBarService, BarService>();
            })
            .InvokeAsync(args);
    }
}

[Command("foo")]
public class FooCommand
{
    [Argument(Description = "Drink order at the Bar")]
    public string Drink { get; set; }
}

public class FooCommandHandler : CommandHandler<FooCommand>
{
    private readonly IBarService _barService;

    public FooCommand(IBarService barService)
    {
        _barService = barService;
    }

    protected override async Task<int> ExecuteAsync(FooCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Console.WriteLine($"Foo walks into a Bar and orders a {command.Drink}");
        
        await _barService.OrderAsync(command.Drink);

        return 0;
    }
}
```
