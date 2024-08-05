using System.Text;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGeneration;

public static class ClassGeneration
{
    public static SyntaxTree GenerateJsonSerializable(string fullNamespace, string className, string comment)
    {
        // todo - runtime types and using statements
        // function to be called "CreateClassWithAttributes or something like that
        var usingDirectives = new[]
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization"))
        };

        var tree = SyntaxFactory.SyntaxTree(
            root: SyntaxFactory.CompilationUnit()
                .WithUsings(SyntaxFactory.List(usingDirectives))
                .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                        CreateClassDeclaration(fullNamespace, className, SyntaxKind.PublicKeyword)
                    )
                )
                .NormalizeWhitespace(),
            encoding: Encoding.UTF8
        );

        return tree;

        static ClassDeclarationSyntax CreateClassDeclaration(string fullNamespace, string className, SyntaxKind scope)
        {
            switch (scope)
            {
                case SyntaxKind.InternalKeyword:
                    break;
                case SyntaxKind.PublicKeyword:
                    break;
                case SyntaxKind.ProtectedKeyword:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
/*
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(Person))]
    internal partial class PersonJsonContext : JsonSerializerContext
    {
    }
*/

            var jsonContextClassName = className + "JsonContext";
            var fullyQualifiedClassName = fullNamespace + "." + className;

            string jsonContextName = typeof(JsonSerializerContext).FullName!;

            return SyntaxFactory.ClassDeclaration(jsonContextClassName)
                .WithAttributeLists(
                    SyntaxFactory.List(
                        [
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(
                                            SyntaxFactory.IdentifierName("JsonSourceGenerationOptions"))
                                        .WithArgumentList(
                                            SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
                                                new SyntaxNodeOrToken[]
                                                {
                                                    SyntaxFactory.AttributeArgument(
                                                            SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression))
                                                        .WithNameEquals(
                                                            SyntaxFactory.NameEquals(
                                                                SyntaxFactory.IdentifierName("WriteIndented"))),
                                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                    SyntaxFactory.AttributeArgument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.IdentifierName("JsonSourceGenerationMode"),
                                                                SyntaxFactory.IdentifierName("Serialization")))
                                                        .WithNameEquals(
                                                            SyntaxFactory.NameEquals(
                                                                SyntaxFactory.IdentifierName("GenerationMode"))),
                                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                    SyntaxFactory.AttributeArgument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.IdentifierName("JsonCommentHandling"),
                                                                SyntaxFactory.IdentifierName("Skip")))
                                                        .WithNameEquals(SyntaxFactory.NameEquals("ReadCommentHandling")),
                                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                    SyntaxFactory.AttributeArgument(
                                                            SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression))
                                                        .WithNameEquals(SyntaxFactory.NameEquals("IgnoreReadOnlyFields"))
                                                }))))),
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(
                                            SyntaxFactory.IdentifierName("JsonSerializable"))
                                        .WithArgumentList(
                                            SyntaxFactory.AttributeArgumentList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.AttributeArgument(
                                                        SyntaxFactory.TypeOfExpression(
                                                            SyntaxFactory.IdentifierName(fullyQualifiedClassName))))))))
                        ]
                    )
                )
                .WithModifiers(
                    SyntaxFactory.TokenList([SyntaxFactory.Token(scope), SyntaxFactory.Token(SyntaxKind.PartialKeyword)])
                )
                .WithBaseList(
                    SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(
                                SyntaxFactory.IdentifierName(jsonContextName)
                            )
                        )
                    )
                );
        }
    }
}