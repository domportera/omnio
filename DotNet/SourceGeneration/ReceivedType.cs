using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SourceGeneration;

public readonly record struct ReceivedType<TSyntax> where TSyntax : MemberDeclarationSyntax
{
    public ReceivedType(TSyntax Syntax)
    {
        this.Syntax = Syntax;
        Name = Syntax switch {
            TypeDeclarationSyntax classSyntax => classSyntax.Identifier.Text,
            MethodDeclarationSyntax methodSyntax => methodSyntax.Identifier.Text,
            PropertyDeclarationSyntax propertySyntax => propertySyntax.Identifier.Text,
            _ => Syntax.TryGetInferredMemberName()
        };
    }

    public TSyntax Syntax { get; }
    public string? Name { get; }
}