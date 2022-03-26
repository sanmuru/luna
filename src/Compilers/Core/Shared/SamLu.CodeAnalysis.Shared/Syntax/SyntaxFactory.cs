using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    ;

/// <summary>
/// 此类型提供构造各种语法节点、标识和琐碎内容的工厂方法。
/// </summary>
public static partial class SyntaxFactory
{
    public static SyntaxTrivia CarriageReturnLineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.CarriageReturnLineFeed;
    public static SyntaxTrivia ElasticCarriageReturnLineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticCarriageReturnLineFeed;

    public static SyntaxTrivia LineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.LineFeed;
    public static SyntaxTrivia ElasticLineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticLineFeed;

    public static SyntaxTrivia CarriageReturn { get; } = Syntax.InternalSyntax.SyntaxFactory.CarriageReturn;
    public static SyntaxTrivia ElasticCarriageReturn { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticCarriageReturn;

    public static SyntaxTrivia Space { get; } = Syntax.InternalSyntax.SyntaxFactory.Space;
    public static SyntaxTrivia ElasticSpace { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticSpace;

    public static SyntaxTrivia Tab { get; } = Syntax.InternalSyntax.SyntaxFactory.Tab;
    public static SyntaxTrivia ElasticTab { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticTab;

    public static SyntaxTrivia ElasticMarker { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticZeroSpace;

    public static SyntaxTrivia EndOfLine(string text) => Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: false);

    public static SyntaxTrivia ElasticEndOfLine(string text) => Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: true);

    public static SyntaxTrivia Whitespace(string text) => Syntax.InternalSyntax.SyntaxFactory.Whitespace(text, elastic: false);

    public static SyntaxTrivia ElasticWhitespace(string text) => Syntax.InternalSyntax.SyntaxFactory.Whitespace(text, elastic: true);

    public static SyntaxTrivia Comment(string text) => Syntax.InternalSyntax.SyntaxFactory.Comment(text);

    public static SyntaxTrivia SyntaxTrivia(SyntaxKind kind, string text!!) =>
        kind switch
        {
            SyntaxKind.EndOfLineTrivia or
            SyntaxKind.WhitespaceTrivia or
            SyntaxKind.SingleLineCommentTrivia or
            SyntaxKind.MultiLineCommentTrivia =>
                new(default, new Syntax.InternalSyntax.SyntaxTrivia(kind, text), 0, 0),
            _ => throw new ArgumentException(null, nameof(kind))
        };

    public static SyntaxToken Token(SyntaxKind kind) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Token(SyntaxFactory.ElasticMarker.UnderlyingNode, kind, SyntaxFactory.ElasticMarker.UnderlyingNode));

    public static SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing) => new(Syntax.InternalSyntax.SyntaxFactory.Token(leading.Node, kind, trailing.Node));

    public static partial SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, string text, string valueText, SyntaxTriviaList trailing);

    public static SyntaxToken MissingToken(SyntaxKind kind) =>
        new(Syntax.InternalSyntax.SyntaxFactory.MissingToken(SyntaxFactory.ElasticMarker.UnderlyingNode, kind, SyntaxFactory.ElasticMarker.UnderlyingNode));

#warning 未完成
}
