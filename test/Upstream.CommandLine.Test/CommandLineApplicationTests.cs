using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NuGet.Frameworks;
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
        TestClass.ConfigureServices(services =>
        {
            services.AddSingleton<FooCommand>();
        });

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
            .ConfigureServices(services =>
            {
                services.AddSingleton<IOutput>(outputMock.Object);
            });

        var exitCode = await TestClass.InvokeAsync(new[] { "foo" });
        
        Assert.Equal(0, exitCode);
        
        outputMock.VerifyAll();
    }
    
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

        public const string Value = "Foo!";

        public Task<int> ExecuteAsync(FooCommand options, CancellationToken cancellationToken)
        {
            _output.Write(Value);

            return Task.FromResult(0);
        }
    }
}