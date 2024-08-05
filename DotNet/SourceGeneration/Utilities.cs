﻿using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGeneration;

public static class Utilities
{
    public static readonly Type[] NonInterfaceDeclarationTypes =
    {
        typeof(ClassDeclarationSyntax),
        typeof(StructDeclarationSyntax),
        typeof(RecordDeclarationSyntax)
    };
    
    public static bool TryGetNamespace(ISymbol symbol, List<INamespaceSymbol> namespaces, StringBuilder sb,
        [NotNullWhen(true)] out string? namespaceString)
    {
        var nsp = symbol.ContainingNamespace;

        if (nsp == null || nsp.IsGlobalNamespace)
        {
            Console.Error.WriteLine(
                $"Namespace not found for {symbol.Name}");
            namespaceString = null;
            return false;
        }

        var nspString = nsp.ToString();

        while (true)
        {
            namespaces.Add(nsp);
            var newNsp = nsp.ContainingNamespace;
            if (newNsp == null)
                break;

            if (newNsp.IsGlobalNamespace)
                break;

            var newNspString = newNsp.ToString();
            if (nspString == newNspString)
                break;

            nsp = newNsp;
            nspString = newNspString;
        }

        for (int i = namespaces.Count - 1; i >= 0; i--)
        {
            sb.Append(namespaces[i]).Append('.');
            namespaces.RemoveAt(i);
        }


        namespaceString = sb.ToString(0, sb.Length - 1);
        sb.Clear();
        return true;
    }
}