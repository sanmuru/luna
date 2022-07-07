﻿using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua;

using System.Text;
using Syntax;

public static partial class SyntaxFactory
{
    #region 标志
    private static partial void ValidateTokenKind(SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind.IdentifierToken:
                throw new ArgumentException(LuaResources.UseIdentifierForTokens, nameof(kind));

            case SyntaxKind.NumericLiteralToken:
                throw new ArgumentException(LuaResources.UseLiteralForNumeric, nameof(kind));
        }

        if (!SyntaxFacts.IsAnyToken(kind))
            throw new ArgumentException(string.Format(LuaResources.ThisMethodCanOnlyBeUsedToCreateTokens, kind), nameof(kind));
    }

    #region 字面量
    public static partial SyntaxToken Literal(long value) =>
        SyntaxFactory.Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None), value);

    public static partial SyntaxToken Literal(string text, long value) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Literal(
            SyntaxFactory.ElasticMarker.UnderlyingNode,
            text,
            value,
            SyntaxFactory.ElasticMarker.UnderlyingNode));

    public static partial SyntaxToken Literal(
        SyntaxTriviaList leading,
        string text,
        long value,
        SyntaxTriviaList trailing) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Literal(
            leading.Node,
            text,
            value,
            trailing.Node));

    public static partial SyntaxToken Literal(ulong value) =>
        SyntaxFactory.Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None), value);

    public static partial SyntaxToken Literal(string text, ulong value) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Literal(
            SyntaxFactory.ElasticMarker.UnderlyingNode,
            text,
            value,
            SyntaxFactory.ElasticMarker.UnderlyingNode));

    public static partial SyntaxToken Literal(
        SyntaxTriviaList leading,
        string text,
        ulong value,
        SyntaxTriviaList trailing) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Literal(
            leading.Node,
            text,
            value,
            trailing.Node));

    public static partial SyntaxToken Literal(double value) =>
        SyntaxFactory.Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None), value);

    public static partial SyntaxToken Literal(string text, double value) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Literal(
            SyntaxFactory.ElasticMarker.UnderlyingNode,
            text,
            value,
            SyntaxFactory.ElasticMarker.UnderlyingNode));

    public static partial SyntaxToken Literal(
        SyntaxTriviaList leading,
        string text,
        double value,
        SyntaxTriviaList trailing) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Literal(
            leading.Node,
            text,
            value,
            trailing.Node));

    public static partial SyntaxToken Literal(string value) =>
        SyntaxFactory.Literal(SymbolDisplay.FormatLiteral(value, quoteStrings: true), value);

    public static partial SyntaxToken Literal(string text, string value) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Literal(
            SyntaxFactory.ElasticMarker.UnderlyingNode,
            text,
            value,
            SyntaxFactory.ElasticMarker.UnderlyingNode));

    public static partial SyntaxToken Literal(
        SyntaxTriviaList leading,
        string text,
        string value,
        SyntaxTriviaList trailing) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Literal(
            leading.Node,
            text,
            value,
            trailing.Node));
    #endregion
    #endregion

    public static ChunkSyntax ParseCompilationUnit(string text, int offset = 0, LuaParseOptions? options = null)
    {
        // note that we do not need a "consumeFullText" parameter, because parsing a compilation unit always must
        // consume input until the end-of-file
        using (var lexer = SyntaxFactory.MakeLexer(text, offset, options))
        using (var parser = SyntaxFactory.MakeParser(lexer))
        {
            var node = parser.ParseCompilationUnit();
            return (ChunkSyntax)node.CreateRed();
        }
    }
}
