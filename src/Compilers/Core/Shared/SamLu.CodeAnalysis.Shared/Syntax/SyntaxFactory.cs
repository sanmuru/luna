using System.Text;
using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;

using ThisSyntaxNode = LuaSyntaxNode;
using ThisSyntaxTree = LuaSyntaxTree;
using ThisParseOptions = LuaParseOptions;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;

using ThisSyntaxNode = MoonScriptSyntaxNode;
using ThisSyntaxTree = MoonScriptSyntaxTree;
using ThisParseOptions = MoonScriptParseOptions;
#endif

/// <summary>
/// 此类型提供构造各种语法节点、标识和琐碎内容的工厂方法。
/// </summary>
public static partial class SyntaxFactory
{
    /// <summary>
    /// 获取包含回车符和换行符的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.EndOfLineTrivia"/>的语法琐碎内容，其包含回车符和换行符。
    /// </value>
    public static SyntaxTrivia CarriageReturnLineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.CarriageReturnLineFeed;
    /// <summary>
    /// 获取包含回车符和换行符的可变的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.EndOfLineTrivia"/>的可变的语法琐碎内容，其包含回车符和换行符。
    /// </value>
    /// <remarks>
    /// 可变的语法琐碎内容用于表示那些不是从解析代码文本过程中产生的琐碎内容，它们一般在格式化时不会被保留。
    /// </remarks>
    public static SyntaxTrivia ElasticCarriageReturnLineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticCarriageReturnLineFeed;

    /// <summary>
    /// 获取包含换行符的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.EndOfLineTrivia"/>的语法琐碎内容，其包含单个换行符。
    /// </value>
    public static SyntaxTrivia LineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.LineFeed;
    /// <summary>
    /// 获取包含换行符的可变的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.EndOfLineTrivia"/>的可变的语法琐碎内容，其包含单个换行符。
    /// </value>
    /// <remarks>
    /// 可变的语法琐碎内容用于表示那些不是从解析代码文本过程中产生的琐碎内容，它们一般在格式化时不会被保留。
    /// </remarks>
    public static SyntaxTrivia ElasticLineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticLineFeed;

    /// <summary>
    /// 获取包含回车符的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.EndOfLineTrivia"/>的语法琐碎内容，其包含单个回车符。
    /// </value>
    public static SyntaxTrivia CarriageReturn { get; } = Syntax.InternalSyntax.SyntaxFactory.CarriageReturn;
    /// <summary>
    /// 获取包含回车符的可变的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.EndOfLineTrivia"/>的可变的语法琐碎内容，其包含单个回车符。
    /// </value>
    /// <remarks>
    /// 可变的语法琐碎内容用于表示那些不是从解析代码文本过程中产生的琐碎内容，它们一般在格式化时不会被保留。
    /// </remarks>
    public static SyntaxTrivia ElasticCarriageReturn { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticCarriageReturn;

    /// <summary>
    /// 获取包含空格符的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.WhitespaceTrivia"/>的语法琐碎内容，其包含单个空格符。
    /// </value>
    public static SyntaxTrivia Space { get; } = Syntax.InternalSyntax.SyntaxFactory.Space;
    /// <summary>
    /// 获取包含空格符的可变的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.WhitespaceTrivia"/>的可变的语法琐碎内容，其包含单个空格符。
    /// </value>
    /// <remarks>
    /// 可变的语法琐碎内容用于表示那些不是从解析代码文本过程中产生的琐碎内容，它们一般在格式化时不会被保留。
    /// </remarks>
    public static SyntaxTrivia ElasticSpace { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticSpace;

    /// <summary>
    /// 获取包含制表符的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.WhitespaceTrivia"/>的语法琐碎内容，其包含单个制表符。
    /// </value>
    public static SyntaxTrivia Tab { get; } = Syntax.InternalSyntax.SyntaxFactory.Tab;
    /// <summary>
    /// 获取包含制表符的可变的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.WhitespaceTrivia"/>的可变的语法琐碎内容，其包含单个制表符。
    /// </value>
    /// <remarks>
    /// 可变的语法琐碎内容用于表示那些不是从解析代码文本过程中产生的琐碎内容，它们一般在格式化时不会被保留。
    /// </remarks>
    public static SyntaxTrivia ElasticTab { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticTab;

    /// <summary>
    /// 获取表示可变记号的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.WhitespaceTrivia"/>的可变的语法琐碎内容，其不包含任何字符。
    /// </value>
    /// <remarks>
    /// 当语法琐碎内容没有明确时，工厂方法将自动置入可变记号。在语法格式化阶段，可变记号将会被替换为合适的语法琐碎内容。
    /// </remarks>
    public static SyntaxTrivia ElasticMarker { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticZeroSpace;

    #region 琐碎内容
    public static SyntaxTrivia EndOfLine(string text) => Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: false);

    public static SyntaxTrivia ElasticEndOfLine(string text) => Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: true);

    public static SyntaxTrivia Whitespace(string text) => Syntax.InternalSyntax.SyntaxFactory.Whitespace(text, elastic: false);

    public static SyntaxTrivia ElasticWhitespace(string text) => Syntax.InternalSyntax.SyntaxFactory.Whitespace(text, elastic: true);

    public static SyntaxTrivia Comment(string text) => Syntax.InternalSyntax.SyntaxFactory.Comment(text);

    public static SyntaxTrivia SyntaxTrivia(SyntaxKind kind, string text)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));

        return kind switch
        {
            SyntaxKind.EndOfLineTrivia or
            SyntaxKind.WhitespaceTrivia or
            SyntaxKind.SingleLineCommentTrivia or
            SyntaxKind.MultiLineCommentTrivia =>
                new(default, new Syntax.InternalSyntax.SyntaxTrivia(kind, text), 0, 0),
            _ => throw new ArgumentException(null, nameof(kind))
        };
    }
    #endregion

    #region 标志
    public static SyntaxToken Token(SyntaxKind kind) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Token(
            SyntaxFactory.ElasticMarker.UnderlyingNode,
            kind,
            SyntaxFactory.ElasticMarker.UnderlyingNode));

    public static SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Token(
            leading.Node,
            kind,
            trailing.Node));

    public static partial SyntaxToken Token(
        SyntaxTriviaList leading,
        SyntaxKind kind,
        string text,
        string valueText,
        SyntaxTriviaList trailing);

    public static SyntaxToken MissingToken(SyntaxKind kind) =>
        new(Syntax.InternalSyntax.SyntaxFactory.MissingToken(
            SyntaxFactory.ElasticMarker.UnderlyingNode,
            kind,
            SyntaxFactory.ElasticMarker.UnderlyingNode));

    public static SyntaxToken MissingToken(
        SyntaxTriviaList leading,
        SyntaxKind kind,
        SyntaxTriviaList trailing) =>
        new(Syntax.InternalSyntax.SyntaxFactory.MissingToken(
            leading.Node,
            kind,
            trailing.Node));

    #region 标识符
    public static SyntaxToken Identifier(string text) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Identifier(
            SyntaxFactory.ElasticMarker.UnderlyingNode,
            text,
            SyntaxFactory.ElasticMarker.UnderlyingNode));

    public static SyntaxToken Identifier(
        SyntaxTriviaList leading,
        string text,
        SyntaxTriviaList trailing) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Identifier(
            leading.Node,
            text,
            trailing.Node));

    public static SyntaxToken Identifier(
        SyntaxTriviaList leading,
        SyntaxKind contextualKind,
        string text,
        string valueText,
        SyntaxTriviaList trailing) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Identifier(
            contextualKind,
            leading.Node,
            text,
            valueText,
            trailing.Node));
    #endregion

    #region 字面量
    public static SyntaxToken Literal(int value) =>
        ThisParseOptions.
    #endregion
    #endregion

#warning 未完成

    public static SyntaxTree SyntaxTree(
        SyntaxNode root,
        ParseOptions? options = null,
        string path = "",
        Encoding? encoding = null) =>
        ThisSyntaxTree.Create(
            (ThisSyntaxNode)root,
            (ThisParseOptions?)options,
            path,
            encoding);
}
