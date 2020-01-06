using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Upstream.CommandLine
{
    public class CommandLineApplication
    {
        private readonly List<CommandBase> _commands = new List<CommandBase>();

        public IServiceProvider Services { get; private set; }

        public Task<int> InvokeAsync(string[] args)
        {
            var builder = new CommandLineBuilder()
                .UseDefaults();

            foreach (var command in _commands)
            {
                builder.AddCommand(command.BuildCommandSymbol());
            }

            return builder.Build().InvokeAsync(args);
        }

        public CommandLineApplication AddCommand<TCommand>()
            where TCommand : CommandBase
        {
            var type = typeof(TCommand);

            return AddCommand(type);
        }

        public CommandLineApplication AddCommand(Type type)
        {
            ConstructorInfo constructor;

            try
            {
                constructor = type.GetConstructors().Single();
            }
            catch (Exception e)
            {
                throw new CommandLineException("Commands must have a single constructor for dependency injection.", e);
            }

            var parameters = constructor.GetParameters();
            var dependencies = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                dependencies[i] = Services.GetRequiredService(parameters[i].ParameterType);
            }

            _commands.Add((CommandBase)Activator.CreateInstance(type, dependencies));

            return this;
        }

        public CommandLineApplication DiscoverCommands()
        {
            var commandBaseType = typeof(CommandBase);
            var allCommandTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsGenericType && !t.IsAbstract && commandBaseType.IsAssignableFrom(t));

            foreach (var commandType in allCommandTypes)
            {
                AddCommand(commandType);
            }

            return this;
        }
    }
}
