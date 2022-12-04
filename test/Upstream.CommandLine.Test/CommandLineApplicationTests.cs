using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Upstream.Testing;
using Xunit;

namespace Upstream.CommandLine.Test;

public class CommandLineApplicationTests : TestBase<CommandLineApplication>
{
    protected override CommandLineApplication CreateTestClass()
    {
        return new CommandLineApplication();
    }

    [Fact]
    public void ServiceProvider_throws_if_not_initialized()
    {
        Assert.Throws<InvalidOperationException>(() => TestClass.ServiceProvider);
    }

    [Fact]
    public void ConfigureServices_can_be_used_for_dependency_injection()
    {
        TestClass.ConfigureServices(serviceCollection => serviceCollection.AddSingleton<FooCommand>());

        _ = TestClass.Build();

        Assert.NotNull(TestClass.ServiceProvider);

        var foo1 = TestClass.ServiceProvider.GetRequiredService<FooCommand>();
        var foo2 = TestClass.ServiceProvider.GetRequiredService<FooCommand>();

        Assert.NotNull(foo1);
        Assert.NotNull(foo2);
        Assert.Same(foo1, foo2);
    }

    [Fact]
    public async Task AddCommand_success()
    {
        var outputMock = new Mock<IOutput>(MockBehavior.Strict);
        outputMock.Setup(m => m.Write(FooCommandHandler.Value));

        TestClass.AddCommand<FooCommandHandler, FooCommand>()
            .ConfigureServices(serviceCollection => serviceCollection.AddSingleton(outputMock.Object));

        var exitCode = await TestClass.InvokeAsync(new[] { "foo" });

        Assert.Equal(0, exitCode);

        outputMock.VerifyAll();
    }

    [Fact]
    public async Task AddCommandGroup_success()
    {
        var outputMock = new Mock<IOutput>(MockBehavior.Strict);
        outputMock.Setup(m => m.Write(FooCommandHandler.Value));

        TestClass
            .AddCommandGroup("bar", (builder) => { builder.AddCommand<FooCommandHandler, FooCommand>(); })
            .ConfigureServices(serviceCollection => serviceCollection.AddSingleton(outputMock.Object));

        var exitCode = await TestClass.InvokeAsync(new[] { "bar", "foo" });

        Assert.Equal(0, exitCode);

        outputMock.VerifyAll();
    }

    [Fact]
    public async Task AddCommandGroup_default_handler()
    {
        TestClass
            .AddCommandGroup("bar", (builder) => builder.AddCommand<FooCommandHandler, FooCommand>());

        var testConsole = new TestConsole();

        var exitCode = await TestClass.InvokeAsync(new[] { "bar" }, testConsole);

        Assert.Equal(1, exitCode);
        Assert.StartsWith("Required command was not provided.", testConsole.GetOutput());
    }

    [Fact]
    public async Task AddCommandGroup_custom_handler()
    {
        var outputMock = new Mock<IOutput>(MockBehavior.Strict);
        outputMock.Setup(m => m.Write("Bar"));

        TestClass
            .AddCommandGroup<BarCommandHandler, FooCommand>("bar",
                (builder) => builder.AddCommand<FooCommandHandler, FooCommand>())
            .ConfigureServices(services => services.AddSingleton(outputMock.Object));

        var testConsole = new TestConsole();

        var exitCode = await TestClass.InvokeAsync(new[] { "bar" }, testConsole);

        Assert.Equal(0, exitCode);

        outputMock.VerifyAll();
    }

    #region Test classes
    
    public interface IOutput
    {
        void Write(string value);
    }

    [Command("foo", "Foo is Bar")]
    private class FooCommand
    {
    }

    private class FooCommandHandler : ICommandHandler<FooCommand>
    {
        private readonly IOutput _output;

        public FooCommandHandler(IOutput output)
        {
            _output = output;
        }

        public const string Value = "Foo";

        public virtual string GetValue() => Value;

        public Task<int> ExecuteAsync(FooCommand options, CancellationToken cancellationToken)
        {
            _output.Write(GetValue());

            return Task.FromResult(0);
        }
    }

    private class BarCommandHandler : FooCommandHandler
    {
        public BarCommandHandler(IOutput output) : base(output)
        {
        }

        public override string GetValue() => "Bar";
    }
    
    #endregion
}