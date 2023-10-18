using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class AugmentingGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a factory that can create our custom syntax receiver
        context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // the generator infrastructure will create a receiver and populate it
        // we can retrieve the populated instance via the context
        MySyntaxReceiver syntaxReceiver = (MySyntaxReceiver)context.SyntaxReceiver;

        // get the recorded user class
        ClassDeclarationSyntax userClass = syntaxReceiver.ClassToAugment;
        if (userClass is null)
        {
            // if we didn't find the user class, there is nothing to do
            return;
        }
        
        // userClass.ContainingNamespace.ToDisplayString()

        // add the generated implementation to the compilation
        SourceText sourceText = SourceText.From($@"
public partial class {userClass.Identifier}
{{
    private void GeneratedMethod()
    {{
        // generated code
    }}
}}", Encoding.UTF8);
        context.AddSource("UserClass.Generated.cs", sourceText);
    }

// var source = $$"""
//                public partial class {{userClass.Identifier}}
//                {
//                    private void BuildCommand()
//                    {
//                        Console.WriteLine("Testing!!!");
//                        // generated code
//                    }
//                }
//                """;
//         
//         SourceText sourceText = SourceText.From(source, Encoding.UTF8);
//         context.AddSource($"{userClass.Identifier}.Generated.cs", sourceText);
    // }

    class MySyntaxReceiver : ISyntaxReceiver
    {
        public ClassDeclarationSyntax ClassToAugment { get; private set; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // if (syntaxNode is ClassDeclarationSyntax cds &&
            //     cds.Identifier.ValueText == "TestCommand")
            // {
            //     ClassToAugment = cds;
            //     return;
            // }
            
            // if (syntaxNode is ClassDeclarationSyntax cds1)
            // {
            //     var commandAttributes = cds1.AttributeLists.SelectMany(x => x.Attributes)
            //         .Where(a => a.Name.ToString() == "CommandGeneratorAttribute")
            //         .ToArray();
            //
            //     if (commandAttributes.Any())
            //     {
            //         ClassToAugment = cds1;
            //     }
            // }
            // Business logic to decide what we're interested in goes here
            if (syntaxNode is ClassDeclarationSyntax cds &&
                cds.Identifier.ValueText == "UserClass")
            {
                ClassToAugment = cds;
            }
            // }
        }
    }
}