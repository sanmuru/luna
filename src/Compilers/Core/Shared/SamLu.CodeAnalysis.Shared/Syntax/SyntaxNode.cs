using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;
using System.Diagnostics.CodeAnalysis;

#if LANG_LUA
using ThisSyntaxNode = SamLu.CodeAnalysis.Lua.LuaSyntaxNode;
using ThisSyntaxTree = SamLu.CodeAnalysis.Lua.LuaSyntaxTree;
using InternalSyntaxNode = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.LuaSyntaxNode;

namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
using ThisSyntaxNode = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxNode;
using ThisSyntaxTree = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxTree;
using InternalSyntaxNode = SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax.MoonScriptSyntaxNode;

namespace SamLu.CodeAnalysis.MoonScript;
#endif

#if LANG_LUA
/// <summary>
/// 表示语法树中的非终结节点。此节点类仅针对于Lua语言构造。
/// </summary>
#elif LANG_MOONSCRIPT
/// <summary>
/// 表示语法树中的非终结节点。此节点类仅针对于MoonScript语言构造。
/// </summary>
#endif
public abstract partial class
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
          : SyntaxNode, IFormattable
{
    /// <summary>
    /// 获取此语法节点所在的语法树。
    /// </summary>
    internal new SyntaxTree SyntaxTree =>
        this._syntaxTree ?? ThisSyntaxNode.ComputeSyntaxTree(this);

    /// <summary>
    /// 获取此语法节点的父节点。若父节点不存在，则返回<see langword="null"/>。
    /// </summary>
    internal new ThisSyntaxNode? Parent => (ThisSyntaxNode?)base.Parent;

    /// <summary>
    /// 获取此语法节点的父节点，或此节点作为结构化语法琐碎内容根节点时的父节点。
    /// </summary>
    internal new ThisSyntaxNode? ParentOrStructuredTriviaParent => (ThisSyntaxNode?)base.ParentOrStructuredTriviaParent;

    /// <summary>
    /// 获取此语法节点描述的语言名称。
    /// </summary>
    public override string Language => this.Green.Language;

    /// <summary>
    /// 获取节点的语法类型。
    /// </summary>
    /// <returns>节点的语法类型。</returns>
    public SyntaxKind Kind() => (SyntaxKind)this.Green.RawKind;

    /// <summary>
    /// 实例化一个语法节点，指定节点的父节点。
    /// </summary>
    /// <param name="green">内部绿树节点。</param>
    /// <param name="parent">节点的父节点。</param>
    /// <param name="position">节点所在位置。</param>
    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
          (InternalSyntaxNode green, ThisSyntaxNode? parent, int position) : base(green, parent, position) { }

    /// <summary>
    /// 实例化一个语法节点，仅用于语法琐碎内容，因为它们不会作为节点的子级，即父节点为<see langword="null"/>，因此实例化时需要明确指明所在的语法树。
    /// </summary>
    /// <param name="green">内部绿树节点。</param>
    /// <param name="position">节点所在位置。</param>
    /// <param name="syntaxTree">节点所在的语法树。</param>
    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
          (InternalSyntaxNode green, int position, SyntaxTree? syntaxTree) : base(green, position, syntaxTree) { }

    private static SyntaxTree ComputeSyntaxTree(ThisSyntaxNode node)
    {
        ArrayBuilder<ThisSyntaxNode>? nodes = null;
        SyntaxTree? tree;

        // 查找最近的具有非空语法树的父节点。
        while (true)
        {
            tree = node._syntaxTree;
            if (tree is not null) break; // 节点自身的语法树非空。

            var parent = node.Parent;
            if (parent is null) // 节点自身即为根节点。
            {
                // 原子操作设置语法树到根节点。
                Interlocked.Exchange(ref node._syntaxTree, ThisSyntaxTree.CreateWithoutClone(node));
                tree = node._syntaxTree;
                break;
            }

            tree = parent._syntaxTree;
            if (tree is not null)
            {
                // 将父节点的语法树设置到节点自身上。
                node._syntaxTree = tree;
                break;
            }

            (nodes ??= ArrayBuilder<ThisSyntaxNode>.GetInstance()).Add(node);
            node = parent;
        }

        // 自上而下传递语法树。
        if (nodes is not null)
        {
            foreach (var n in nodes)
            {
                var existingTree = n._syntaxTree;
                if (existingTree is not null)
                {
                    Debug.Assert(existingTree == tree, "至少有一个节点位于其他语法树。");
                    break;
                }

                n._syntaxTree = tree;
            }

            nodes.Free();
        }

        return tree;
    }

    /// <summary>
    /// 获取此节点在源代码中的位置。
    /// </summary>
    /// <returns>此节点在源代码中的位置。</returns>
    public new Location GetLocation() => new SourceLocation(this);

    [return:NotNullIfNotNull("visitor")]
    public abstract TResult? Accept<TResult>(
#if LANG_LUA
        LuaSyntaxVisitor<TResult>
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxVisitor<TResult>
#endif
        visitor);

    public abstract void Accept(
#if LANG_LUA
        LuaSyntaxVisitor
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxVisitor
#endif
         visitor);

    #region 序列化
    /// <summary>
    /// 从字节流中反序列化语法节点。
    /// </summary>
    /// <param name="stream">从中读取数据的流。</param>
    /// <param name="cancellationToken">取消操作的标识。</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/>的值为<see langword="null"/>。</exception>
    /// <exception cref="InvalidOperationException"><paramref name="stream"/>流不可读。</exception>
    /// <exception cref="ArgumentException"><paramref name="stream"/>流中含有不合法的数据。</exception>
    public static SyntaxNode DeserializeFrom(Stream stream!!, CancellationToken cancellationToken = default)
    {
        if (!stream.CanRead)
            throw new InvalidOperationException(CodeAnalysisResources.TheStreamCannotBeReadFrom);

        using var reader = ObjectReader.TryGetReader(stream, leaveOpen: true, cancellationToken);

        if (reader is null)
            throw new ArgumentException(CodeAnalysisResources.Stream_contains_invalid_data, nameof(stream));

        var root = (InternalSyntaxNode)reader.ReadValue();
        return root.CreateRed();
    }
    #endregion

    #region 查找标识
    /// <summary>
    /// 获取以此节点为根节点的语法树的第一个标识。
    /// </summary>
    /// <param name="predicate">筛选符合条件的标识的方法。若要允许所有标识，则传入<see langword="null"/>。</param>
    /// <param name="stepInto">若值不是<see langword="null"/>时深入语法琐碎内容。仅此委托返回<see langword="true"/>时语法琐碎内容才会被包含在内。</param>
    /// <returns>以此节点为根节点的语法树的第一个标识。</returns>
    internal SyntaxToken GetFirstToken(Func<SyntaxToken, bool>? predicate, Func<SyntaxTrivia, bool>? stepInto = null) =>
        SyntaxNavigator.Instance.GetFirstToken(this, predicate, stepInto);

    /// <summary>
    /// 获取以此节点为根节点的语法树的最后一个标识。
    /// </summary>
    /// <param name="predicate">筛选符合条件的标识的方法。</param>
    /// <param name="stepInto">若值不是<see langword="null"/>时深入语法琐碎内容。仅此委托返回<see langword="true"/>时语法琐碎内容才会被包含在内。</param>
    /// <returns>以此节点为根节点的语法树的最后一个标识。</returns>
    internal SyntaxToken GetLastToken(Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool>? stepInto = null) =>
        SyntaxNavigator.Instance.GetLastToken(this, predicate, stepInto);
    #endregion

    #region SyntaxNode
    /// <summary>
    /// 获取此语法节点所在的语法树。若节点不再任何一颗语法树上，则自动生成一颗语法树。
    /// </summary>
    protected override SyntaxTree SyntaxTreeCore => this.SyntaxTree;

    /// <summary>
    /// 确定此节点是否与另一个节点在结构上相等。
    /// </summary>
    /// <param name="other">相比较的另一个节点。</param>
    /// <returns>若两个节点在结构上相等，则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    /// <exception cref="ExceptionUtilities.Unreachable">默认抛出的异常。</exception>
    protected override bool EquivalentToCore(SyntaxNode other) => throw ExceptionUtilities.Unreachable;

    protected internal override SyntaxNode ReplaceCore<TNode>(
        IEnumerable<TNode>? nodes = null,
        Func<TNode, TNode, SyntaxNode>? computeReplacementNode = null,
        IEnumerable<SyntaxToken>? tokens = null,
        Func<SyntaxToken, SyntaxToken, SyntaxToken>? computeReplacementToken = null,
        IEnumerable<SyntaxTrivia>? trivia = null,
        Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? computeReplacementTrivia = null)
    {
        Trace.Assert(typeof(ThisSyntaxNode).IsAssignableFrom(typeof(TNode)), $"TNode必须继承自{typeof(ThisSyntaxNode).FullName}。");

        return Syntax.SyntaxReplacer.Replace(this,
            nodes.Cast<ThisSyntaxNode>(), computeReplacementNode is null ? null : (node, rewritten) => (ThisSyntaxNode)computeReplacementNode((node as TNode)!, (rewritten as TNode)!),
            tokens, computeReplacementToken,
            trivia, computeReplacementTrivia
        ).AsRootOfNewTreeWithOptionsFrom(this.SyntaxTree);
    }
        

    protected internal override SyntaxNode ReplaceNodeInListCore(
        SyntaxNode originalNode,
        IEnumerable<SyntaxNode> replacementNodes) =>
        Syntax.SyntaxReplacer.ReplaceNodeInList(this,
            (ThisSyntaxNode)originalNode,
            replacementNodes.Cast<ThisSyntaxNode>()
        ).AsRootOfNewTreeWithOptionsFrom(this.SyntaxTree);

    protected internal override SyntaxNode InsertNodesInListCore(
        SyntaxNode nodeInList,
        IEnumerable<SyntaxNode> nodesToInsert,
        bool insertBefore) =>
        Syntax.SyntaxReplacer.InsertNodeInList(
            this,
            (ThisSyntaxNode)nodeInList,
            nodesToInsert.Cast<ThisSyntaxNode>(),
            insertBefore
        ).AsRootOfNewTreeWithOptionsFrom(this.SyntaxTree);

    protected internal override SyntaxNode ReplaceTokenInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens) =>
        Syntax.SyntaxReplacer.ReplaceTokenInList(this, originalToken, newTokens).AsRootOfNewTreeWithOptionsFrom(this.SyntaxTree);

    protected internal override SyntaxNode InsertTokensInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens, bool insertBefore) =>
        Syntax.SyntaxReplacer.InsertTokenList(this, originalToken, newTokens, insertBefore).AsRootOfNewTreeWithOptionsFrom(this.SyntaxTree);

    protected internal override SyntaxNode ReplaceTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia) =>
        Syntax.SyntaxReplacer.ReplaceTriviaInList(this, originalTrivia, newTrivia).AsRootOfNewTreeWithOptionsFrom(this.SyntaxTree);

    protected internal override SyntaxNode InsertTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia, bool insertBefore) =>
        Syntax.SyntaxReplacer.InsertTriviaInList(this, originalTrivia, newTrivia, insertBefore).AsRootOfNewTreeWithOptionsFrom(this.SyntaxTree);

    protected internal override SyntaxNode? RemoveNodesCore(IEnumerable<SyntaxNode> nodes, SyntaxRemoveOptions options) =>
        Syntax.SyntaxNodeRemover.RemoveNodes(this,
            nodes.Cast<ThisSyntaxNode>(),
            options
        ).AsRootOfNewTreeWithOptionsFrom(this.SyntaxTree);

    protected internal override SyntaxNode NormalizeWhitespaceCore(string indentation, string eol, bool elasticTrivia) =>
        Syntax.SyntaxNormalizer.Normalize(this, indentation, eol, elasticTrivia).AsRootOfNewTreeWithOptionsFrom(this.SyntaxTree);

    protected override bool IsEquivalentToCore(SyntaxNode node, bool topLevel = false) =>
        Syntax.SyntaxFactory.AreEquivalent(this, (ThisSyntaxNode)node, topLevel);

    internal override bool ShouldCreateWeakList()
    {
        return base.ShouldCreateWeakList();
    }
    #endregion

    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => this.ToString();
}
