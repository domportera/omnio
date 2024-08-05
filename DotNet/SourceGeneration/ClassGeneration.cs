using System.Text;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SourceGeneration;

public static class ClassGeneration
{
    public static SyntaxTree GenerateJsonSerializable(string fullNamespace, string className, string comment)
    {
        // todo - runtime types and using statements
        // function to be called "CreateClassWithAttributes or something like that
        var usingDirectives = new[]
        {
            SF.UsingDirective(SF.ParseName("System")), SF.UsingDirective(SF.ParseName("System.Text.Json")),
            SF.UsingDirective(SF.ParseName("System.Text.Json.Serialization"))
        };

        var tree = SF.SyntaxTree(
            root: SF.CompilationUnit()
                .WithUsings(SF.List(usingDirectives))
                .WithMembers(SF.SingletonList<MemberDeclarationSyntax>(
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

            //todo - add seriap8zatioj attribute for field types 
            return SF.ClassDeclaration(jsonContextClassName)
                .WithAttributeLists(SF.List(
                        [
                            SF.AttributeList(SF.SingletonSeparatedList(SF
                                .Attribute(SF.IdentifierName("JsonSourceGenerationOptions"))
                                .WithArgumentList(SF.AttributeArgumentList(SF.SeparatedList<AttributeArgumentSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        SF.AttributeArgument(SF.LiteralExpression(SyntaxKind.TrueLiteralExpression))
                                            .WithNameEquals(SF.NameEquals(SF.IdentifierName("WriteIndented"))),
                                        SF.Token(SyntaxKind.CommaToken), SF.AttributeArgument(SF.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SF.IdentifierName("JsonSourceGenerationMode"),
                                                SF.IdentifierName("Serialization")))
                                            .WithNameEquals(SF.NameEquals(SF.IdentifierName("GenerationMode"))),
                                        SF.Token(SyntaxKind.CommaToken), SF.AttributeArgument(SF.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SF.IdentifierName("JsonCommentHandling"), SF.IdentifierName("Skip")))
                                            .WithNameEquals(SF.NameEquals("ReadCommentHandling")),
                                        SF.Token(SyntaxKind.CommaToken), SF
                                            .AttributeArgument(SF.LiteralExpression(SyntaxKind.TrueLiteralExpression))
                                            .WithNameEquals(SF.NameEquals("IgnoreReadOnlyFields"))
                                    }))))),
                            SF.AttributeList(SF.SingletonSeparatedList(
                                GenerateAttributeWithTypeArgument(fullyQualifiedClassName)))
                        ]
                    )
                )
                .WithModifiers(
                    SF.TokenList([SF.Token(scope), SF.Token(SyntaxKind.PartialKeyword)])
                )
                .WithBaseList(
                    SF.BaseList(
                        SF.SingletonSeparatedList<BaseTypeSyntax>(
                            SF.SimpleBaseType(
                                SF.IdentifierName(jsonContextName)
                            )
                        )
                    )
                );
        }
    }

    private static AttributeSyntax GenerateAttributeWithTypeArgument(string fullyQualifiedTypeName)
    {
        return SF.Attribute(SF.IdentifierName("JsonSerializable"))
            .WithArgumentList(SF.AttributeArgumentList(
                SF.SingletonSeparatedList(
                    SF.AttributeArgument(SF.TypeOfExpression(SF.IdentifierName(fullyQualifiedTypeName))))));
    }
}