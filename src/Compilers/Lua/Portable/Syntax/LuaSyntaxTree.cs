using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SamLu.CodeAnalysis.Lua;

/// <summary>
/// 一份Lua源代码文件的解析后表示。
/// </summary>
public abstract partial class LuaSyntaxTree : SyntaxTree
{
    internal static readonly SyntaxTree Dummy = new DummySyntaxTree();

    /// <summary>
    /// 为导出语法树的解析器使用的选项。
    /// </summary>
    public new abstract LuaParseOptions Options { get; }

    protected T CloneNodeAsRoot<T>(T node)
        where T : LuaSyntaxNode =>
        LuaSyntaxNode.CloneNodeAsRoot(node, this);

    /// <summary>
    /// 获取语法树的根节点。
    /// </summary>
    /// <param name="cancellationToken">获取过程的取消标记。</param>
    /// <returns>语法树的根节点。</returns>
    public new abstract LuaSyntaxNode GetRoot(CancellationToken cancellationToken = default);

    /// <summary>
    /// 尝试获取语法树的根节点，并返回一个值指示其是否存在。
    /// </summary>
    /// <param name="root">语法树的根节点</param>
    /// <returns>一个值，指示语法树的根节点是否存在。</returns>
    public abstract bool TryGetRoot([NotNullWhen(true)] out LuaSyntaxNode? root);

    /// <summary>
    /// 异步获取语法树的根节点。
    /// </summary>
    /// <param name="cancellationToken">获取过程的取消标记。</param>
    /// <returns>获取过程的任务。</returns>
    /// <remarks>
    /// 默认情况下获取工作将在当前线程中立即执行。
    /// 若希望进行其他安排的实现，则应该重写 <see cref="GetRootAsync(CancellationToken)"/>。
    /// </remarks>
    public new virtual Task<LuaSyntaxNode> GetRootAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(this.TryGetRoot(out LuaSyntaxNode? node) ? node : this.GetRoot(cancellationToken));

    public override bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false) =>
        SyntaxFactory.AreEquivalent(this, tree, topLevel);

    #region Factories
    public static SyntaxTree Create(LuaSyntaxNode root!!, LuaParseOptions? options = null, string? path = "", Encoding? encoding = null) =>
        new ParsedSyntaxTree(
            text: null,
            encoding: null,
            checksumAlgorithm: SourceHashAlgorithm.Sha1,
            path: path,
            options: options ?? LuaParseOptions.Default,
            root: root,
            cloneRoot: true);

    internal static SyntaxTree CreateForDebugger(LuaSyntaxNode root!!, SourceText text, LuaParseOptions options) => new DebuggerSyntaxTree(root, text, options);

    internal static SyntaxTree CreateWithoutClone(LuaSyntaxNode root!!) =>
        new ParsedSyntaxTree(
            text: null,
            encoding: null,
            checksumAlgorithm: SourceHashAlgorithm.Sha1,
            path: "",
            options: LuaParseOptions.Default,
            root: root,
            cloneRoot: false);

    internal static SyntaxTree ParseTextLazy(SourceText text, LuaParseOptions? options = null, string path = "") =>
        new LazySyntaxTree(text, options ?? LuaParseOptions.Default, path);

    public static SyntaxTree ParseText(
        string text,
        LuaParseOptions? options = null,
        string path = "",
        Encoding? encoding = null,
        CancellationToken cancellationToken = default) => LuaSyntaxTree.ParseText(
            text: SourceText.From(text, encoding),
            options: options,
            path: path,
            cancellationToken: cancellationToken);

    public static SyntaxTree ParseText(
        SourceText text!!,
        LuaParseOptions? options = null,
        string path = "",
        CancellationToken cancellationToken = default)
    {
        options = options ?? LuaParseOptions.Default;

        using var lexer = new InternalSyntax.Lexer(text, options);
        using var parser = new InternalSyntax.LanguageParser(lexer, oldTree: null, changes: null, cancellationToken: cancellationToken);
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
        if (this.TryGetRoot(out LuaSyntaxNode? node))
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
