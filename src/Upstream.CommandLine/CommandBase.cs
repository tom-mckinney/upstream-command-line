using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    public abstract class CommandBase
    {
        public abstract Command BuildCommandSymbol();

        protected abstract string Name { get; }

        protected virtual string Description { get; } = null;

        protected virtual string[] Aliases { get; } = new string[] { };

        protected virtual Argument[] Arguments { get; } = new Argument[0] { };

        protected virtual Option[] CommandOptions { get; } = new Option[0] { };
    }

    public abstract class CommandBase<TOptions> : CommandBase
        where TOptions : class
    {
        protected TOptions Options { get; private set; }

        public override Command BuildCommandSymbol()
        {
            var command = new Command(Name, Description)
            {
                Handler = CommandHandler.Create<TOptions>((options) =>
                {
                    return InvokeAsync(options);
                })
            };

            foreach (var alias in Aliases)
            {
                command.AddAlias(alias);
            }

            foreach (var argument in Arguments)
            {
                command.AddArgument(argument);
            }

            foreach (var option in CommandOptions)
            {
                command.AddOption(option);
            }

            return command;
        }

        public async Task InvokeAsync(TOptions options)
        {
            Options = options;

            await BeforeExecuteAsync();

            await ExecuteAsync();

            await AfterExecuteAsync();
        }

        protected virtual Task BeforeExecuteAsync()
        {
            return Task.CompletedTask;
        }

        protected abstract Task ExecuteAsync();

        protected virtual Task AfterExecuteAsync()
        {
            return Task.CompletedTask;
        }
    }
}
