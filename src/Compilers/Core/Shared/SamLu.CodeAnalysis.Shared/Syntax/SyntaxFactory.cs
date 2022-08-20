using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.Text;

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
/// 此类型提供构造各种语法节点、标志和琐碎内容的工厂方法。
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
    /// 一个语法类型为<see cref="SyntaxKind.WhiteSpaceTrivia"/>的语法琐碎内容，其包含单个空格符。
    /// </value>
    public static SyntaxTrivia Space { get; } = Syntax.InternalSyntax.SyntaxFactory.Space;
    /// <summary>
    /// 获取包含空格符的可变的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.WhiteSpaceTrivia"/>的可变的语法琐碎内容，其包含单个空格符。
    /// </value>
    /// <remarks>
    /// 可变的语法琐碎内容用于表示那些不是从解析代码文本过程中产生的琐碎内容，它们一般在格式化时不会被保留。
    /// </remarks>
    public static SyntaxTrivia ElasticSpace { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticSpace;

    /// <summary>
    /// 获取包含制表符的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.WhiteSpaceTrivia"/>的语法琐碎内容，其包含单个制表符。
    /// </value>
    public static SyntaxTrivia Tab { get; } = Syntax.InternalSyntax.SyntaxFactory.Tab;
    /// <summary>
    /// 获取包含制表符的可变的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.WhiteSpaceTrivia"/>的可变的语法琐碎内容，其包含单个制表符。
    /// </value>
    /// <remarks>
    /// 可变的语法琐碎内容用于表示那些不是从解析代码文本过程中产生的琐碎内容，它们一般在格式化时不会被保留。
    /// </remarks>
    public static SyntaxTrivia ElasticTab { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticTab;

    /// <summary>
    /// 获取表示可变记号的语法琐碎内容。
    /// </summary>
    /// <value>
    /// 一个语法类型为<see cref="SyntaxKind.WhiteSpaceTrivia"/>的可变的语法琐碎内容，其不包含任何字符。
    /// </value>
    /// <remarks>
    /// 当语法琐碎内容没有明确时，工厂方法将自动置入可变记号。在语法格式化阶段，可变记号将会被替换为合适的语法琐碎内容。
    /// </remarks>
    public static SyntaxTrivia ElasticMarker { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticZeroSpace;

    #region 琐碎内容
    public static SyntaxTrivia EndOfLine(string text) => Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: false);

    public static SyntaxTrivia ElasticEndOfLine(string text) => Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: true);

    public static SyntaxTrivia WhiteSpace(string text) => Syntax.InternalSyntax.SyntaxFactory.WhiteSpace(text, elastic: false);

    public static SyntaxTrivia ElasticWhiteSpace(string text) => Syntax.InternalSyntax.SyntaxFactory.WhiteSpace(text, elastic: true);

    public static SyntaxTrivia Comment(string text) => Syntax.InternalSyntax.SyntaxFactory.Comment(text);

    public static SyntaxTrivia SyntaxTrivia(SyntaxKind kind, string text)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));

        return kind switch
        {
            SyntaxKind.EndOfLineTrivia or
            SyntaxKind.WhiteSpaceTrivia or
            SyntaxKind.SingleLineCommentTrivia or
            SyntaxKind.MultiLineCommentTrivia =>
                new(default, new Syntax.InternalSyntax.SyntaxTrivia(kind, text), 0, 0),
            _ => throw new ArgumentException(null, nameof(kind))
        };
    }

    public static SyntaxTrivia Trivia(Syntax.StructuredTriviaSyntax node) => new(default, node.Green, position: 0, index: 0);
    #endregion

    #region 标志
    public static SyntaxToken Token(SyntaxKind kind) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Token(
            SyntaxFactory.ElasticMarker.UnderlyingNode,
            kind,
            SyntaxFactory.ElasticMarker.UnderlyingNode));

    public static SyntaxToken Token(
        SyntaxTriviaList leading,
        SyntaxKind kind,
        SyntaxTriviaList trailing) =>
        new(Syntax.InternalSyntax.SyntaxFactory.Token(
            leading.Node,
            kind,
            trailing.Node));

    public static SyntaxToken Token(
        SyntaxTriviaList leading,
        SyntaxKind kind,
        string text,
        string valueText,
        SyntaxTriviaList trailing)
    {
        SyntaxFactory.ValidateTokenKind(kind);

        return new(Syntax.InternalSyntax.SyntaxFactory.Token(
            leading.Node,
            kind,
            text,
            valueText,
            trailing.Node));
    }

    private static partial void ValidateTokenKind(SyntaxKind kind);

#if DEBUG
    internal static ThisSyntaxNode Mock() => new ThisSyntaxNode.MockNode(Syntax.InternalSyntax.SyntaxFactory.Mock());

    internal static SyntaxToken Token(SyntaxNode parent, Syntax.InternalSyntax.SyntaxToken token, int position, int index) => SyntaxFactory.Token((ThisSyntaxNode)parent, token, position, index);

    internal static SyntaxToken Token(ThisSyntaxNode parent, Syntax.InternalSyntax.SyntaxToken token, int position, int index) => new(parent, token, position, index);
#endif

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

    public static SyntaxToken BadToken(
        SyntaxTriviaList leading,
        string text,
        SyntaxTriviaList trailing) =>
        new(Syntax.InternalSyntax.SyntaxFactory.BadToken(
            leading.Node,
            text,
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
    /// <summary>
    /// 构造表示64位有符号整数的语法标志。
    /// </summary>
    /// <param name="value">表示的64位有符号整数。</param>
    /// <returns>表示64位有符号整数的语法标志。</returns>
    public static partial SyntaxToken Literal(long value);

    /// <summary>
    /// 构造表示64位有符号整数的语法标志，使用指定的字符串表示。
    /// </summary>
    /// <param name="text">指定的<paramref name="value"/>的字符串表示。</param>
    /// <param name="value">表示的64位有符号整数。</param>
    /// <returns>表示64位有符号整数的语法标志。</returns>
    public static partial SyntaxToken Literal(string text, long value);

    /// <summary>
    /// 构造表示64位有符号整数的语法标志，使用指定的字符串表示以及前后方语法琐碎内容。
    /// </summary>
    /// <param name="leading">指定的前方语法琐碎内容。</param>
    /// <param name="text">指定的<paramref name="value"/>的字符串表示。</param>
    /// <param name="value">表示的64位有符号整数。</param>
    /// <param name="trailing">指定的后方语法琐碎内容。</param>
    /// <returns>表示64位有符号整数的语法标志。</returns>
    public static partial SyntaxToken Literal(
        SyntaxTriviaList leading,
        string text,
        long value,
        SyntaxTriviaList trailing);

    /// <summary>
    /// 构造表示64位无符号整数的语法标志。
    /// </summary>
    /// <param name="value">表示的64位无符号整数。</param>
    /// <returns>表示64位无符号整数的语法标志。</returns>
    public static partial SyntaxToken Literal(ulong value);

    /// <summary>
    /// 构造表示64位无符号整数的语法标志，使用指定的字符串表示。
    /// </summary>
    /// <param name="text">指定的<paramref name="value"/>的字符串表示。</param>
    /// <param name="value">表示的64位无符号整数。</param>
    /// <returns>表示64位无符号整数的语法标志。</returns>
    public static partial SyntaxToken Literal(string text, ulong value);

    /// <summary>
    /// 构造表示64位无符号整数的语法标志，使用指定的字符串表示以及前后方语法琐碎内容。
    /// </summary>
    /// <param name="leading">指定的前方语法琐碎内容。</param>
    /// <param name="text">指定的<paramref name="value"/>的字符串表示。</param>
    /// <param name="value">表示的64位无符号整数。</param>
    /// <param name="trailing">指定的后方语法琐碎内容。</param>
    /// <returns>表示64位无符号整数的语法标志。</returns>
    public static partial SyntaxToken Literal(
        SyntaxTriviaList leading,
        string text,
        ulong value,
        SyntaxTriviaList trailing);

    /// <summary>
    /// 构造表示双精度浮点数的语法标志。
    /// </summary>
    /// <param name="value">表示的双精度浮点数。</param>
    /// <returns>表示双精度浮点数的语法标志。</returns>
    public static partial SyntaxToken Literal(double value);

    /// <summary>
    /// 构造表示双精度浮点数的语法标志，使用指定的字符串表示。
    /// </summary>
    /// <param name="text">指定的<paramref name="value"/>的字符串表示。</param>
    /// <param name="value">表示的双精度浮点数。</param>
    /// <returns>表示双精度浮点数的语法标志。</returns>
    public static partial SyntaxToken Literal(string text, double value);

    /// <summary>
    /// 构造表示双精度浮点数的语法标志，使用指定的字符串表示以及前后方语法琐碎内容。
    /// </summary>
    /// <param name="leading">指定的前方语法琐碎内容。</param>
    /// <param name="text">指定的<paramref name="value"/>的字符串表示。</param>
    /// <param name="value">表示的双精度浮点数。</param>
    /// <param name="trailing">指定的后方语法琐碎内容。</param>
    /// <returns>表示双精度浮点数的语法标志。</returns>
    public static partial SyntaxToken Literal(
        SyntaxTriviaList leading,
        string text,
        double value,
        SyntaxTriviaList trailing);

    /// <summary>
    /// 构造表示字符串的语法标志。
    /// </summary>
    /// <param name="value">表示的字符串。</param>
    /// <returns>表示字符串的语法标志。</returns>
    public static partial SyntaxToken Literal(string value);

    /// <summary>
    /// 构造表示字符串的语法标志，使用指定的字符串表示。
    /// </summary>
    /// <param name="text">指定的<paramref name="value"/>的字符串表示。</param>
    /// <param name="value">表示的字符串。</param>
    /// <returns>表示字符串的语法标志。</returns>
    public static partial SyntaxToken Literal(string text, string value);

    /// <summary>
    /// 构造表示字符串的语法标志，使用指定的字符串表示以及前后方语法琐碎内容。
    /// </summary>
    /// <param name="leading">指定的前方语法琐碎内容。</param>
    /// <param name="text">指定的<paramref name="value"/>的字符串表示。</param>
    /// <param name="value">表示的字符串。</param>
    /// <param name="trailing">指定的后方语法琐碎内容。</param>
    /// <returns>表示字符串的语法标志。</returns>
    public static partial SyntaxToken Literal(
        SyntaxTriviaList leading,
        string text,
        string value,
        SyntaxTriviaList trailing);
    #endregion
    #endregion

    #region List
    public static SyntaxList<TNode> List<TNode>() where TNode : ThisSyntaxNode => default;

    public static SyntaxList<TNode> SingletonList<TNode>(TNode node) where TNode : ThisSyntaxNode => new(node);

    public static SyntaxList<TNode> List<TNode>(IEnumerable<TNode> nodes) where TNode : ThisSyntaxNode => new(nodes);
    #endregion

    #region TokenList
    public static SyntaxTokenList TokenList() => default;

    public static SyntaxTokenList TokenList(SyntaxToken token) => new(token);

    public static SyntaxTokenList TokenList(params SyntaxToken[] tokens) => new(tokens);

    public static SyntaxTokenList TokenList(IEnumerable<SyntaxToken> tokens) => new(tokens);
    #endregion

    #region TriviaList
    public static SyntaxTriviaList TriviaList() => default;

    public static SyntaxTriviaList TriviaList(SyntaxTrivia trivia) => new(trivia);

    public static SyntaxTriviaList TriviaList(params SyntaxTrivia[] trivia) => new(trivia);

    public static SyntaxTriviaList TriviaList(IEnumerable<SyntaxTrivia> trivia) => new(trivia);
    #endregion

    #region SeparatedList
    public static SeparatedSyntaxList<TNode> SeparatedList<TNode>() where TNode : ThisSyntaxNode => default;

    public static SeparatedSyntaxList<TNode> SingletonSeparatedList<TNode>(TNode node) where TNode : ThisSyntaxNode => new(new SyntaxNodeOrTokenList(node, index: 0));

    public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<TNode>? nodes) where TNode : ThisSyntaxNode
    {
        if (nodes is null) return default;

        var collection = nodes as ICollection<TNode>;
        if (collection is not null && collection.Count == 0) return default;

        using var enumerator = nodes.GetEnumerator();
        if (!enumerator.MoveNext()) return default;

        var firstNode = enumerator.Current;
        if (!enumerator.MoveNext())
            return SingletonSeparatedList(firstNode);

        var builder = new SeparatedSyntaxListBuilder<TNode>(collection?.Count ?? 3);
        builder.Add(firstNode);
        var commaToken = SyntaxFactory.Token(SyntaxKind.CommaToken);
        do
        {
            builder.AddSeparator(commaToken);
            builder.Add(enumerator.Current);
        }
        while (enumerator.MoveNext());

        return builder.ToList();
    }

#warning 未完成

    #endregion

#warning 未完成

    public static SyntaxTree ParseSyntaxTree(
        SourceText text,
        ParseOptions? options,
        string path,
        CancellationToken cancellationToken) =>
        SyntaxFactory.ParseSyntaxTree(text, (ThisParseOptions?)options, path, cancellationToken);

    public static SyntaxTree ParseSyntaxTree(
        SourceText text,
        ThisParseOptions? options,
        string path,
        CancellationToken cancellationToken)
    {
        return ThisSyntaxTree.ParseText(text, options, path, cancellationToken);
    }

    public static SyntaxTree SyntaxTree(
        SyntaxNode root,
        ParseOptions? options = null,
        string path = "",
        Encoding? encoding = null) =>
        SyntaxFactory.SyntaxTree((ThisSyntaxNode)root, (ThisParseOptions?)options, path, encoding);

    public static SyntaxTree SyntaxTree(
        ThisSyntaxNode root,
        ThisParseOptions? options = null,
        string path = "",
        Encoding? encoding = null) =>
        ThisSyntaxTree.Create(
            root,
            options,
            path,
            encoding);

    #region 语法树相等判断
    public static bool AreEquivalent(SyntaxTree? oldTree, SyntaxTree? newTree, bool topLevel) => SyntaxFactory.AreEquivalent((ThisSyntaxTree?)oldTree, (ThisSyntaxTree?)newTree, topLevel);

    public static bool AreEquivalent(ThisSyntaxTree? oldTree, ThisSyntaxTree? newTree, bool topLevel)
    {
#warning 未完成。
        throw new NotImplementedException();
    }

    public static bool AreEquivalent(SyntaxNode? oldNode, SyntaxNode? newNode, bool topLevel) => SyntaxFactory.AreEquivalent((ThisSyntaxNode?)oldNode, (ThisSyntaxNode?)newNode, topLevel);

    public static bool AreEquivalent(ThisSyntaxNode? oldNode, ThisSyntaxNode? newNode, bool topLevel)
    {
#warning 未完成。
        throw new NotImplementedException();
    }

    public static bool AreEquivalent(SyntaxNode? oldNode, SyntaxNode? newNode, Func<SyntaxKind, bool>? ignoreChildNode = null) => SyntaxFactory.AreEquivalent((ThisSyntaxNode?)oldNode, (ThisSyntaxNode?)newNode, ignoreChildNode);

    public static bool AreEquivalent(ThisSyntaxNode? oldNode, ThisSyntaxNode? newNode, Func<SyntaxKind, bool>? ignoreChildNode = null)
    {
#warning 未完成。
        throw new NotImplementedException();
    }

    public static bool AreEquivalent(SyntaxToken oldToken, SyntaxToken newToken)
    {
#warning 未完成。
        throw new NotImplementedException();
    }

    public static bool AreEquivalent(SyntaxTokenList oldList, SyntaxTokenList newList)
    {
#warning 未完成。
        throw new NotImplementedException();
    }

    public static bool AreEquivalent<TNode>(SyntaxList<TNode> oldList, SyntaxList<TNode> newList, bool topLevel)
        where TNode : ThisSyntaxNode
    {
#warning 未完成。
        throw new NotImplementedException();
    }

    public static bool AreEquivalent<TNode>(SyntaxList<TNode> oldList, SyntaxList<TNode> newList, Func<SyntaxKind, bool>? ignoreChildNode = null)
        where TNode : ThisSyntaxNode
    {
#warning 未完成。
        throw new NotImplementedException();
    }

    public static bool AreEquivalent<TNode>(SeparatedSyntaxList<TNode> oldList, SeparatedSyntaxList<TNode> newList, bool topLevel)
        where TNode : ThisSyntaxNode
    {
#warning 未完成。
        throw new NotImplementedException();
    }

    public static bool AreEquivalent<TNode>(SeparatedSyntaxList<TNode> oldList, SeparatedSyntaxList<TNode> newList, Func<SyntaxKind, bool>? ignoreChildNode = null)
        where TNode : ThisSyntaxNode
    {
#warning 未完成。
        throw new NotImplementedException();
    }
    #endregion
}
