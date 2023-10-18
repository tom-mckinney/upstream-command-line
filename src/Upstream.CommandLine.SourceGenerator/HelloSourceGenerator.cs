using Microsoft.CodeAnalysis;

namespace Upstream.CommandLine.SourceGenerator
{
    [Generator]
    public class HelloSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // Find the main method
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

            string source = $$"""
                              // <auto-generated/>
                              using System;

                              namespace {{mainMethod.ContainingNamespace.ToDisplayString()}}
                              {
                                  public static partial class {{mainMethod.ContainingType.Name}}
                                  {
                                      static partial void HelloFrom(string name) =>
                                          Console.WriteLine($"Generator says: Hi from '{name}' test");
                                  }
                              }
                              """;

            var typeName = mainMethod.ContainingType.Name;

            // Add the source code to the compilation
            context.AddSource($"{typeName}.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}