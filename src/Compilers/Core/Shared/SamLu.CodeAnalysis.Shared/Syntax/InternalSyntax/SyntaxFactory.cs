using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax.InternalSyntax;

/// <summary>
/// 此类型提供构造各种内部的语法节点、标识和琐碎内容的工厂方法。
/// </summary>
internal static partial class SyntaxFactory
{
    /// <summary>表示回车符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia CarriageReturn = SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, "\r");
    /// <summary>表示可变的回车符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia ElasticCarriageReturn = SyntaxFactory.CarriageReturn.AsElastic();

    /// <summary>表示换行符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia LineFeed = SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, "\n");
    /// <summary>表示可变的换行符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia ElasticLineFeed = SyntaxFactory.LineFeed.AsElastic();

    /// <summary>表示回车换行符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia CarriageReturnLineFeed = SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, "\r\n");
    /// <summary>表示可变的回车换行符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia ElasticCarriageReturnLineFeed = SyntaxFactory.CarriageReturnLineFeed.AsElastic();

    /// <summary>表示垂直制表符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia VerticalTab = SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, "\v");
    /// <summary>表示可变的垂直制表符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia ElasticVerticalTab = SyntaxFactory.VerticalTab.AsElastic();

    /// <summary>表示换页符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia FormFeed = SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, "\f");
    /// <summary>表示可变的换页符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia ElasticFormFeed = SyntaxFactory.FormFeed.AsElastic();

    /// <summary>表示空格符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia Space = SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, " ");
    /// <summary>表示可变的空格符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia ElasticSpace = SyntaxFactory.Space.AsElastic();

    /// <summary>表示制表符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia Tab = SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, "\t");
    /// <summary>表示可变的制表符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia ElasticTab = SyntaxFactory.Tab.AsElastic();

    /// <summary>表示可变的零空格符的语法琐碎内容。</summary>
    internal static readonly SyntaxTrivia ElasticZeroSpace = SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, string.Empty).AsElastic();

    /// <summary>
    /// 将语法琐碎内容转换为可变的。
    /// </summary>
    /// <param name="trivia">要转换的语法琐碎内容。</param>
    /// <returns>可变的语法琐碎内容。</returns>
    internal static SyntaxTrivia AsElastic(this SyntaxTrivia trivia) => trivia.WithAnnotationsGreen(new[] { SyntaxAnnotation.ElasticAnnotation });

    /// <summary>
    /// 构造表示行末的内部语法琐碎内容。
    /// </summary>
    /// <param name="text">表示行末的字符串。</param>
    /// <param name="elastic">生成的语法琐碎内容是否为可变的。</param>
    /// <returns>表示行末的内部语法琐碎内容。</returns>
    internal static SyntaxTrivia EndOfLine(string text, bool elastic = false) =>
        text switch
        {
            "\r" => elastic ? SyntaxFactory.ElasticCarriageReturn : SyntaxFactory.CarriageReturn,
            "\n" => elastic ? SyntaxFactory.ElasticLineFeed : SyntaxFactory.LineFeed,
            "\r\n" => elastic ? SyntaxFactory.ElasticCarriageReturnLineFeed : SyntaxFactory.CarriageReturnLineFeed,
            "\v" => elastic ? SyntaxFactory.ElasticVerticalTab : SyntaxFactory.Tab,
            "\f" => elastic ? SyntaxFactory.ElasticFormFeed : SyntaxFactory.FormFeed,
            _ => elastic switch
            {
                false => SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, text),
                true => SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, text).AsElastic()
            }
        };

    /// <summary>
    /// 构造表示空白内容的内部语法琐碎内容。
    /// </summary>
    /// <param name="text">表示空白内容的字符串。</param>
    /// <param name="elastic">生成的语法琐碎内容是否为可变的。</param>
    /// <returns>表示空白内容的内部语法琐碎内容。</returns>
    internal static SyntaxTrivia Whitespace(string text, bool elastic = false) =>
        (text, elastic) switch
        {
            (" ", _) => elastic ? SyntaxFactory.ElasticSpace : SyntaxFactory.Space,
            ("\t", _) => elastic ? SyntaxFactory.ElasticTab : SyntaxFactory.Tab,
            ("", true) => SyntaxFactory.ElasticZeroSpace,
            _ => elastic switch
            {
                false => SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, text),
                true => SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, text).AsElastic()
            }
        };

    /// <summary>
    /// 构造表示注释的内部语法琐碎内容。
    /// </summary>
    /// <param name="text">表示注释的字符串。</param>
    /// <returns>表示注释的内部语法琐碎内容。</returns>
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
