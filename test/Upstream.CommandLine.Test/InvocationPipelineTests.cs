using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Upstream.CommandLine.Test;

public class InvocationPipelineTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Handler_without_middleware_is_executed_on_invocation(int expectedExitCode)
    {
        var command = new TestCommand();
        var cancellationToken = new CancellationToken();

        var handler = new Mock<ICommandHandler<TestCommand>>(MockBehavior.Strict);

        handler.Setup(h => h.ExecuteAsync(command, cancellationToken))
            .ReturnsAsync(expectedExitCode);

        var invocationPipeline =
            new InvocationPipeline<ICommandHandler<TestCommand>, TestCommand>(handler.Object, null);
        
        // no execution on initialization
        handler.Verify(h => 
            h.ExecuteAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);

        var actualExitCode = await invocationPipeline.InvokeAsync(command, cancellationToken);
        
        Assert.Equal(expectedExitCode, actualExitCode);
        Assert.Equal(expectedExitCode, invocationPipeline.ExitCode);
        
        handler.Verify(h => 
                h.ExecuteAsync(command, cancellationToken),
            Times.Once);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Handler_and_middleware_are_executed_on_invocation(int expectedExitCode)
    {
        var command = new TestCommand();
        var cancellationToken = new CancellationToken();

        var handler = new Mock<ICommandHandler<TestCommand>>(MockBehavior.Strict);
        
        handler.Setup(h => h.ExecuteAsync(command, cancellationToken))
            .ReturnsAsync(expectedExitCode);
        
        int middlewareInvocationOrder = 0;

        var fooMiddleware = new TestCommandHandlerMiddleware();
        var barMiddleware = new TestCommandHandlerMiddleware();
        
        // before services executed in order of declaration
        fooMiddleware.BeforeService.Setup(b => b.Execute(command))
            .Callback(() => Assert.Equal(0, middlewareInvocationOrder++));
        barMiddleware.BeforeService.Setup(b => b.Execute(command))
            .Callback(() => Assert.Equal(1, middlewareInvocationOrder++));

        // after services executed in reverse order of declaration
        barMiddleware.AfterService.Setup(b => b.Execute(command))
            .Callback(() => Assert.Equal(2, middlewareInvocationOrder++));
        fooMiddleware.AfterService.Setup(b => b.Execute(command))
            .Callback(() => Assert.Equal(3, middlewareInvocationOrder++));

        var invocationPipeline =
            new InvocationPipeline<ICommandHandler<TestCommand>, TestCommand>(handler.Object, new[]
            {
                fooMiddleware,
                barMiddleware,
            });
        
        // no execution on initialization
        handler.Verify(h => 
                h.ExecuteAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
        fooMiddleware.BeforeService.Verify(b => b.Execute(It.IsAny<TestCommand>()), Times.Never);
        fooMiddleware.AfterService.Verify(b => b.Execute(It.IsAny<TestCommand>()), Times.Never);
        barMiddleware.BeforeService.Verify(b => b.Execute(It.IsAny<TestCommand>()), Times.Never);
        barMiddleware.AfterService.Verify(b => b.Execute(It.IsAny<TestCommand>()), Times.Never);

        var actualExitCode = await invocationPipeline.InvokeAsync(command, cancellationToken);

        Assert.Equal(expectedExitCode, actualExitCode);
        Assert.Equal(expectedExitCode, invocationPipeline.ExitCode);
        
        handler.Verify(h => 
                h.ExecuteAsync(command, cancellationToken),
            Times.Once);
        fooMiddleware.BeforeService.Verify(b => b.Execute(command), Times.Once);
        fooMiddleware.AfterService.Verify(b => b.Execute(command), Times.Once);
        barMiddleware.BeforeService.Verify(b => b.Execute(command), Times.Once);
        barMiddleware.AfterService.Verify(b => b.Execute(command), Times.Once);
    }

    public class TestCommand
    {
        public string Foo { get; set; }

        public int Bar { get; set; }
    }
    
    public interface IBeforeService
    {
        void Execute<TCommand>(TCommand command);
    }
    
    public interface IAfterService
    {
        void Execute<TCommand>(TCommand command);
    }
    
    public class TestCommandHandlerMiddleware : ICommandHandlerMiddleware
    {
        public Mock<IBeforeService> BeforeService { get; } = new(MockBehavior.Strict);
        public Mock<IAfterService> AfterService { get; } = new(MockBehavior.Strict);
        
        public async Task InvokeAsync<TCommand>(TCommand command, Func<TCommand, Task> next, CancellationToken cancellationToken) where TCommand : class
        {
            BeforeService.Object.Execute(command);
            await next(command);
            AfterService.Object.Execute(command);
        }
    }
}