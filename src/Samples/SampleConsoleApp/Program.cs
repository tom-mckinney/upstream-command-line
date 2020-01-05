using SampleConsoleApp.Commands;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Upstream.CommandLine;

namespace SampleConsoleApp
{
    public class Program
    {
        public static Stopwatch Stopwatch { get; } = new Stopwatch();

        public static Task Main(string[] args)
        {
            Stopwatch.Start();

            return new CommandLineApplication()
                .AddCommand<FooCommand>()
                .InvokeAsync(args);
        }
    }
}
