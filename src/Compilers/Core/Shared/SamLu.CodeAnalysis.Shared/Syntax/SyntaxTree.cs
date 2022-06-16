using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using System.Diagnostics;

#if LANG_LUA
using ThisSyntaxTree = SamLu.CodeAnalysis.Lua.LuaSyntaxTree;
using ThisSyntaxNode = SamLu.CodeAnalysis.Lua.LuaSyntaxNode;
using ThisParseOptions = SamLu.CodeAnalysis.Lua.LuaParseOptions;

namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
using ThisSyntaxTree = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxTree;
using ThisSyntaxNode = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxNode;
using ThisParseOptions = SamLu.CodeAnalysis.MoonScript.MoonScriptParseOptions;

namespace SamLu.CodeAnalysis.MoonScript;
#endif

#if LANG_LUA
/// <summary>一份Lua源代码文件的解析后表示。</summary>
#elif LANG_MOONSCRIPT
/// <summary>一份MoonScript源代码文件的解析后表示。</summary>
#endif
public abstract partial class
#if LANG_LUA
    LuaSyntaxTree
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxTree
#endif
    : SyntaxTree
{
    internal static readonly SyntaxTree Dummy = new DummySyntaxTree();

    /// <summary>
    /// 为导出语法树的解析器使用的选项。
    /// </summary>
    public new abstract ThisParseOptions Options { get; }

    /// <inheritdoc cref="ThisSyntaxNode.CloneNodeAsRoot{T}(T, SyntaxTree)"/>
    /// <seealso cref="ThisSyntaxNode.CloneNodeAsRoot{T}(T, SyntaxTree)"/>
    protected T CloneNodeAsRoot<T>(T node)
        where T : ThisSyntaxNode =>
        ThisSyntaxNode.CloneNodeAsRoot(node, this);

    /// <summary>
    /// 获取语法树的根节点。
    /// </summary>
    /// <param name="cancellationToken">获取过程的取消标记。</param>
    /// <returns>语法树的根节点。</returns>
    public new abstract ThisSyntaxNode GetRoot(CancellationToken cancellationToken = default);

    /// <summary>
    /// 尝试获取语法树的根节点，并返回一个值指示其是否存在。
    /// </summary>
    /// <param name="root">语法树的根节点</param>
    /// <returns>一个值，指示语法树的根节点是否存在。</returns>
    public abstract bool TryGetRoot([NotNullWhen(true)] out ThisSyntaxNode? root);

    /// <summary>
    /// 异步获取语法树的根节点。
    /// </summary>
    /// <param name="cancellationToken">获取过程的取消标记。</param>
    /// <returns>获取过程的任务。</returns>
    /// <remarks>
    /// 默认情况下获取工作将在当前线程中立即执行。
    /// 若希望进行其他安排的实现，则应该重写 <see cref="GetRootAsync(CancellationToken)"/>。
    /// </remarks>
    public new virtual Task<ThisSyntaxNode> GetRootAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(this.TryGetRoot(out ThisSyntaxNode? node) ? node : this.GetRoot(cancellationToken));

    public override bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false) =>
        SyntaxFactory.AreEquivalent(this, tree, topLevel);

    #region 工厂方法
    public static SyntaxTree Create(ThisSyntaxNode root!!, ThisParseOptions? options = null, string? path = "", Encoding? encoding = null) =>
        new ParsedSyntaxTree(
            text: null,
            encoding: null,
            checksumAlgorithm: SourceHashAlgorithm.Sha1,
            path: path,
            options: options ?? ThisParseOptions.Default,
            root: root,
            cloneRoot: true);

    internal static SyntaxTree CreateForDebugger(ThisSyntaxNode root!!, SourceText text, ThisParseOptions options) => new DebuggerSyntaxTree(options, root, text);

    internal static SyntaxTree CreateWithoutClone(ThisSyntaxNode root!!) =>
        new ParsedSyntaxTree(
            text: null,
            encoding: null,
            checksumAlgorithm: SourceHashAlgorithm.Sha1,
            path: "",
            options: ThisParseOptions.Default,
            root: root,
            cloneRoot: false);

    internal static SyntaxTree ParseTextLazy(SourceText text, ThisParseOptions? options = null, string path = "") =>
        new LazySyntaxTree(text, options ?? ThisParseOptions.Default, path);

    /// <inheritdoc cref="ThisSyntaxTree.ParseText(SourceText, ThisParseOptions?, string, CancellationToken)"/>
    public static SyntaxTree ParseText(
        string text,
        ThisParseOptions? options = null,
        string path = "",
        Encoding? encoding = null,
        CancellationToken cancellationToken = default) => ThisSyntaxTree.ParseText(
            text: SourceText.From(text, encoding),
            options: options,
            path: path,
            cancellationToken: cancellationToken);

    /// <summary>
    /// 解析代码文本并产生语法树。
    /// </summary>
    /// <param name="text">要解析的代码文本。</param>
    /// <param name="options">解析选项。</param>
    /// <param name="path">代码文本的路径。</param>
    /// <param name="cancellationToken">解析操作的取消标志。</param>
    /// <returns>表示<paramref name="text"/>的所有信息的语法树。</returns>
    public static SyntaxTree ParseText(
        SourceText text!!,
        ThisParseOptions? options = null,
        string path = "",
        CancellationToken cancellationToken = default)
    {
        options ??= ThisParseOptions.Default;

        // 创建词法分析器。
        using var lexer = new Syntax.InternalSyntax.Lexer(text, options);
        // 创建语言解析器。
        using var parser = new Syntax.InternalSyntax.LanguageParser(lexer, oldTree: null, changes: null, cancellationToken: cancellationToken);
        // 解析并生成编译单元节点（绿树节点）对应的红树节点。
        var block = (BlockSyntax)parser.ParseBlock().CreateRed();

        // 创建已解析的语法树，使用编译单元节点作为根节点。
        var tree = new ParsedSyntaxTree(
            text,
            text.Encoding,
            text.ChecksumAlgorithm,
            path,
            options,
            block,
            cloneRoot: true);
        // 验证语法树的所有节点均匹配代码文本。
        tree.VerifySource();

        return tree;
    }
    #endregion

    #region 更改
    public override SyntaxTree WithChangedText(SourceText newText)
    {
        if (this.TryGetText(out SourceText? oldText))
        {
            var changes = newText.GetChangeRanges(oldText);

            if (changes.Count == 0 && newText == oldText) return this;
            
            return this.WithChanges(newText, changes);
        }

        return this.WithChanges(newText, new[] { new TextChangeRange(new TextSpan(0, this.Length), newText.Length) });
    }

    private SyntaxTree WithChanges(SourceText newText, IReadOnlyList<TextChangeRange> changes!!)
    {
        var workingChanges = changes;
        var oldTree = this;

        // 如果全文都发生更改，则重新进行全文解析。
        if (workingChanges.Count == 1 && workingChanges[0].Span == new TextSpan(0, this.Length) && workingChanges[0].NewLength == newText.Length)
        {
            workingChanges = null;
            oldTree = null;
        }

        using var lexer = new Syntax.InternalSyntax.Lexer(newText, this.Options);
        using var parser = new Syntax.InternalSyntax.LanguageParser(lexer, oldTree?.GetRoot(), workingChanges);

        var compilationUnit = (BlockSyntax)parser.ParseBlock().CreateRed();
        return new ParsedSyntaxTree(
            newText,
            newText.Encoding,
            newText.ChecksumAlgorithm,
            this.FilePath,
            this.Options,
            compilationUnit,
            cloneRoot: true
        );
    }

    public override IList<TextSpan> GetChangedSpans(SyntaxTree oldTree) => SyntaxDiffer.GetPossiblyDifferentTextSpans(oldTree, this);

    public override IList<TextChange> GetChanges(SyntaxTree oldTree) => SyntaxDiffer.GetTextChanges(oldTree, this);
    #endregion

    #region 行位置和定位
    /// <summary>
    /// 获取指定的字符位置对应的行位置。
    /// </summary>
    /// <param name="position">指定的字符位置。</param>
    /// <param name="cancellationToken">取消操作的标识符。</param>
    /// <returns>指定的字符位置对应的行位置。</returns>
    private LinePosition GetLinePosition(int position, CancellationToken cancellationToken) => this.GetText(cancellationToken).Lines.GetLinePosition(position);

    /// <summary>
    /// 获取指定的文本范围所在的位置信息。
    /// </summary>
    /// <param name="span">一段文本的范围。</param>
    /// <returns>这段文本所在的位置信息。</returns>
    public override Location GetLocation(TextSpan span) => new SourceLocation(this, span);

    public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default) =>
        new(this.FilePath, this.GetLinePosition(span.Start, cancellationToken), this.GetLinePosition(span.End, cancellationToken));

    /// <summary>
    /// 获取重新映射后的行位置范围信息。因为基于Lua的语言不支持重新映射行位置，所以此方法等同于<see cref="GetLineSpan(TextSpan, CancellationToken)"/>。
    /// </summary>
    /// <returns>重新映射后的行位置范围信息。</returns>
    /// <inheritdoc cref="GetLineSpan(TextSpan, CancellationToken)"/>
    public sealed override FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = default) => this.GetLineSpan(span, cancellationToken);

    internal sealed override FileLinePositionSpan GetMappedLineSpanAndVisibility(TextSpan span, out bool isHiddenPosition) => base.GetMappedLineSpanAndVisibility(span, out isHiddenPosition);

    public sealed override LineVisibility GetLineVisibility(int position, CancellationToken cancellationToken = default) => LineVisibility.Visible;

    public sealed override IEnumerable<LineMapping> GetLineMappings(CancellationToken cancellationToken = default) => Array.Empty<LineMapping>();

    public sealed override bool HasHiddenRegions() => false;
    #endregion

    #region 诊断
    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node) => this.GetDiagnostics(node.Green, node.Position);

    private IEnumerable<Diagnostic> GetDiagnostics(GreenNode node, int position)
    {
        if (node.ContainsDiagnostics)
            return this.EnumerateDiagnostics(node, position);
        else
            return SpecializedCollections.EmptyEnumerable<Diagnostic>();
    }

    private IEnumerable<Diagnostic> EnumerateDiagnostics(GreenNode node, int position) =>
        new SyntaxTreeDiagnosticEnumerator(this, node, position).GetEnumerable();

    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token)
    {
        Debug.Assert(token.Node is not null);
        return this.GetDiagnostics(token.Node, token.Position);
    }

    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia)
    {
        Debug.Assert(trivia.UnderlyingNode is not null);
        return this.GetDiagnostics(trivia.UnderlyingNode, trivia.Position);
    }

    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNodeOrToken nodeOrToken)
    {
        Debug.Assert(nodeOrToken.UnderlyingNode is not null);
        return this.GetDiagnostics(nodeOrToken.UnderlyingNode, nodeOrToken.Position);
    }

    public override IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default) => this.GetDiagnostics(this.GetRoot(cancellationToken));
    #endregion

    #region SyntaxTree
    /// <inheritdoc/>
    protected sealed override ParseOptions OptionsCore => this.Options;

    /// <inheritdoc/>
    protected sealed override SyntaxNode GetRootCore(CancellationToken cancellationToken) => this.GetRoot(cancellationToken);

    /// <inheritdoc/>
    protected sealed override async Task<SyntaxNode> GetRootAsyncCore(CancellationToken cancellationToken) => await this.GetRootAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    protected sealed override bool TryGetRootCore([NotNullWhen(true)] out SyntaxNode? root)
    {
        if (this.TryGetRoot(out ThisSyntaxNode? node))
        {
            root = node;
            return true;
        }
        else
        {
            root = null;
            return false;
        }
    }
    #endregion
}
