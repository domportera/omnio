using System.Text;
using Microsoft.CodeAnalysis;
using SyntaxReceiver = SourceGeneration.SyntaxReceiverByAttribute
    <System.SerializableAttribute, Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>;

namespace SourceGeneration;

public partial class JsonSerializerGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(receiverCreator: () =>
            new SyntaxReceiver(
                acceptSubTypes: true,
                acceptableDeclarationTypes: Utilities.NonInterfaceDeclarationTypes
            )
        );
    }

    public void Execute(GeneratorExecutionContext context)
    {
        Console.WriteLine("Execute JsonSerializerGenerator");
        var syntaxReceiver = context.SyntaxReceiver;
        if (!syntaxReceiver.Is(out SyntaxReceiver? receiver))
            return;

        List<INamespaceSymbol> namespaces = [];
        StringBuilder sb = new(80);
        foreach (var syntax in receiver.Collected)
        {
            var semanticModel = context.Compilation.GetSemanticModel(syntax.SyntaxTree);
            var symbol = semanticModel.GetDeclaredSymbol(syntax);
            if (symbol == null)
            {
                Console.Error.WriteLine($"Symbol not found for {syntax.Kind()} at {syntax.GetLocation()}");
                continue;
            }

            if (!Utilities.TryGetNamespace(symbol, namespaces, sb, out var namespaceString))
                continue;

            var tree = ClassGeneration.GenerateJsonSerializable(namespaceString, symbol.Name, "Generated at build time");

            if (!tree.TryGetText(out var text))
            {
                text = tree.GetText();
            }

            Console.WriteLine("--------------------");
            Console.WriteLine($"Generated code:\n{text}\n");
            Console.WriteLine("--------------------\n\n");

            var fullyQualifiedClassName = namespaceString + "." + symbol.Name;
            var code = tree.GetText();
            context.AddSource(fullyQualifiedClassName + ".Generated.cs", code);

            Evil.RunJsonNetSourceGenerator(context.AddSource, context.Compilation, code);
        }
    }
}