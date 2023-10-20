# upstream-command-line

[![nuget](https://img.shields.io/nuget/v/Upstream.CommandLine)](https://www.nuget.org/packages/Upstream.CommandLine/) ![Publish Package status](https://github.com/tom-mckinney/upstream-command-line/workflows/Publish%20Package/badge.svg?branch=master) ![Run Tests status](https://github.com/tom-mckinney/upstream-command-line/workflows/Run%20Tests/badge.svg?branch=master)

A wrapper around `System.CommandLine` to allow for large, service-oriented application development. Some of the benefits
of this package include:

- **Native IOC support**: All commands are registered and instantiated via
  the [ServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.serviceprovider)
  .
- **Attribute-based command declaration**: Commands can be configured via readable, statically typed classes&mdash;no
  more mapping classes to functional configuration methods.
- **Leverage System.CommandLine**: This is not a fork, but a flexible wrapper around the .NET Foundation
  sponsored [System.CommandLine](https://github.com/dotnet/command-line-api) library. This means that your application
  will benefit from the continued development/maturity of that project, and can benefit from integrations
  like `dotnet-suggest` out of the box. If you wish to switch to a vanilla System.CommandLine implementation down the
  road, the migration is straightforward.

## Setup

Add the package as a dependency to your .NET project via the following command:

```
dotnet add package Upstream.CommandLine
```

## Usage

Refer to
the [Sample project](https://github.com/tom-mckinney/upstream-command-line/tree/main/src/Samples/SampleConsoleApp) for a
working example of this package. The following is a simple (non-functional) example of how to configure an
Upstream.CommandLine console application that uses dependency injected services, command groups, middleware, and
exception handling:

```csharp
public static class Program
{
    public static Task Main(string[] args)
    {
        return new CommandLineApplication()
            .AddCommand<FooCommandHandler, FooCommand>()
            .AddCommandGroup("gizmo", builder =>
            {
                builder.AddCommand<GadgetCommandHandler, GadgetCommand>();                
            })
            .AddMiddleware<GreetMiddleware>()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IBarService, BarService>();
            })
            .UseExceptionHandler(e =>
            {
                Console.WriteLine($"An exception occured: {e.Message}");
            })
            .InvokeAsync(args);
    }
}
```

## Commands

A command is a class-based representation of the arguments, options, and other tokens that make up a set of command line
instructions. Commands are defined by properties decorated with the `[Command]`, `[Argument]`, and/or `[Options]`
attributes.

Example:

```csharp
[Command("foo")]
public class FooCommand
{
    [Argument(Description = "Drink order at the Bar")]
    public Drink Drink { get; set; }
    
    [Option("-n", "--name")]
    public string Name { get; set; }
    
    [Option("-d", "--double")]
    public bool Double { get; set; }
}

public enum Drink
{
    Whiskey,
    Wine,
    Beer,
}
```

This following command invocation and class initialization are equivalent:

```bash
> foo beer -n Homer --double
```

```csharp
new FooCommand
{
    Drink = Drink.Beer,
    Name = "Homer",
    Double = true,
}
```

## Command Groups

Command groups are a feature that allows nested subcommands. The following configuration would enable the subsequent
command:

```csharp
new CommandLineApplication()
    .AddCommandGroup("foo", builder =>
    {
        builder.AddCommandGroup("bar", builder =>
        {
            builder.AddCommandGroup<BazCommandHandler, BazCommand>();
        });
    });
```

```bash
> foo bar baz
```

## Command Handlers

A command handler is a class that implements `ICommandHandler` and defines the execution of a [command](#commands).
Command handlers are instantiated via dependency injection to enable IoC patterns while developing your command line
application.

An `ICommandHandler` must return an integer representing the exit code of the command. A `0` typically indicates a
successful execution, while any other integer indicates an unsuccessful execution.

Example:

```csharp
public class FooCommandHandler : CommandHandler<FooCommand>
{
    private readonly IBarService _barService;

    public FooCommand(IBarService barService)
    {
        _barService = barService;
    }

    protected override async Task<int> ExecuteAsync(FooCommand command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Console.WriteLine($"{command.Name} walks into a Bar and orders a {command.Drink}");
        
        await _barService.OrderAsync(command.Drink);

        return 0;
    }
}
```

## Middleware

There are two types of middleware that are supported by this library.

#### System.CommandLine Middleware

Classes that implement `ICommandInvocationMiddleware` can be added to the application pipeline to validate or alter a
command. For more information, please refer to
the [System.CommandLine Middleware Documentation](https://docs.microsoft.com/en-us/dotnet/standard/commandline/use-middleware). These classes are invoked during parsing and model binding. Therefore,
you cannot access the Command class that is available in command handlers.

Example:

```csharp
public class GreetMiddleware : ICommandInvocationMiddleware
{
    public async Task InvokeAsync(InvocationContext context
        Func<InvocationContext, Task> next)
    {
        Console.WriteLine("Hello!");

        await next(context);
        
        Console.WriteLine("Goodbye!");
    }
}
```

#### Upstream.CommandLine Middleware

Classes that implement `ICommandHandlerMiddleware` can be registered with the service provider to alter or supplement command handling. These classes are invoked after model binding but before the matching `ICommandHandler<TCommand>` is executed.

Example:

```csharp
public class CommandTypeMiddleware : ICommandHandlerMiddleware
{
    public async Task InvokeAsync<TCommand>(TCommand command, Func<TCommand, Task> next, CancellationToken cancellationToken)
        where TCommand : class
    {
        Console.WriteLine($"Before executing command: {command.GetType().Name}");
        
        if (command is TestCommand testCommand)
        {
            Console.WriteLine($"Looks like this is a test!: {testCommand.Reason}");
        }

        await next(command);
        
        Console.WriteLine($"After executing command: {command.GetType().Name}");
    }
}
```