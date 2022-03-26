using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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

#region Factories
    public static SyntaxTree Create(ThisSyntaxNode root!!, ThisParseOptions? options = null, string? path = "", Encoding? encoding = null) =>
        new ParsedSyntaxTree(
            text: null,
            encoding: null,
            checksumAlgorithm: SourceHashAlgorithm.Sha1,
            path: path,
            options: options ?? ThisParseOptions.Default,
            root: root,
            cloneRoot: true);

    internal static SyntaxTree CreateForDebugger(ThisSyntaxNode root!!, SourceText text, ThisParseOptions options) => new DebuggerSyntaxTree(root, text, options);

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

    public static SyntaxTree ParseText(
        SourceText text!!,
        ThisParseOptions? options = null,
        string path = "",
        CancellationToken cancellationToken = default)
    {
        options = options ?? ThisParseOptions.Default;

        using var lexer = new Syntax.InternalSyntax.Lexer(text, options);
        using var parser = new Syntax.InternalSyntax.LanguageParser(lexer, oldTree: null, changes: null, cancellationToken: cancellationToken);
        var compilationUnit = (CompilationUnitSyntax)parser.ParseCompilationUnit().CreateRead();
        var tree = new ParsedSyntaxTree(
            text,
            text.Encoding,
            text.ChecksumAlgorithm,
            path,
            options,
            compilationUnit,
            cloneRoot: true);
        tree.VerifySource();
        return tree;
    }
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
