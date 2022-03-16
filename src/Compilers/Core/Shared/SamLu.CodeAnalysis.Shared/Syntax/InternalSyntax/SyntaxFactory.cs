using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax.InternalSyntax;

internal static partial class SyntaxFactory
{
    private const string CrLf = "\r\n";
    internal static readonly SyntaxTrivia CarriageReturnLineFeed = SyntaxFactory.EndOfLine(SyntaxFactory.CrLf);
    internal static readonly SyntaxTrivia ElasticCarriageReturnLineFeed = SyntaxFactory.EndOfLine(SyntaxFactory.CrLf, elastic: true);

    internal static readonly SyntaxTrivia LineFeed = SyntaxFactory.EndOfLine("\n");
    internal static readonly SyntaxTrivia ElasticLineFeed = SyntaxFactory.EndOfLine("\n", elastic: true);

    internal static readonly SyntaxTrivia CarriageReturn = SyntaxFactory.EndOfLine("\r");
    internal static readonly SyntaxTrivia ElasticCarriageReturn = SyntaxFactory.EndOfLine("\r", elastic: true);

    internal static readonly SyntaxTrivia Space = SyntaxFactory.Whitespace(" ");
    internal static readonly SyntaxTrivia ElasticSpace = SyntaxFactory.Whitespace(" ", elastic: true);

    internal static readonly SyntaxTrivia Tab = SyntaxFactory.Whitespace("\r");
    internal static readonly SyntaxTrivia ElasticTab = SyntaxFactory.Whitespace("\r", elastic: true);

    internal static readonly SyntaxTrivia ElasticZeroSpace = SyntaxFactory.Whitespace(string.Empty, elastic: true);

    internal static SyntaxTrivia EndOfLine(string text, bool elastic = false) =>
        text switch
        {
            "\r" => elastic ? SyntaxFactory.ElasticCarriageReturn : SyntaxFactory.CarriageReturn,
            "\n" => elastic ? SyntaxFactory.ElasticLineFeed : SyntaxFactory.LineFeed,
            "\r\n" => elastic ? SyntaxFactory.ElasticCarriageReturnLineFeed : SyntaxFactory.CarriageReturnLineFeed
            _ => elastic switch
            {
                false => SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, text),
                true => SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, text).WithAnnotationsGreen(new[] { SyntaxAnnotation.ElasticAnnotation })
            }
        };

    internal static SyntaxTrivia Whitespace(string text, bool elastic = false) =>
        elastic switch
        {
            false => SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, text),
            true => SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, text).WithAnnotationsGreen(new[] { SyntaxAnnotation.ElasticAnnotation })
        };

    internal static SyntaxTrivia Comment(string text)
    {
        // 检测text是否为多行注释的格式（“--[”与“[”之间间隔零个或复数个“=”）。
        if (text[2] == '[')
        {
            for (int i = 3; i < text.Length; i++)
            {
                if (i == '[')
                    return SyntaxTrivia.Create(SyntaxKind.MultiLineCommentTrivia, text);
                else if (i == '=')
                    continue;
            }
        }

        // 否则text为单行注释。
        return SyntaxTrivia.Create(SyntaxKind.SingleLineCommentTrivia, text);
    }

    public static SyntaxToken Token(SyntaxKind kind) => SyntaxToken.Create(kind);

    internal static SyntaxToken Token(GreenNode? leading, SyntaxKind kind, GreenNode? trailing) => SyntaxToken.Create(kind, leading, trailing);

    internal static SyntaxToken Token(GreenNode? leading, SyntaxKind kind, string text, string valueText, GreenNode? trailing)
    {
        Debug.Assert(SyntaxFacts.IsAnyToken(kind));
#if LANG_LUA
        Debug.Assert(kind != SyntaxKind.IdentifierToken);
        Debug.Assert(kind != SyntaxKind.NumericLiteralToken);
#elif LANG_MOONSCRIPT
#warning 未进行对应断言检查
#endif

        string defaultText = SyntaxFacts.GetText(kind);
        return kind >= SyntaxToken.FirstTokenWithWellKnownText && kind <= SyntaxToken.LastTokenWithWellKnownText && text == defaultText && valueText == defaultText
            ? SyntaxFactory.Token(leading, kind, trailing)
            : SyntaxToken.WithValue(kind, leading, text, valueText, trailing);
    }

#warning 未完成
}
