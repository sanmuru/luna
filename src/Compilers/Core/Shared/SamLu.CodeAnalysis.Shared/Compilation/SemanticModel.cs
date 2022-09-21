using System.Diagnostics;
using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;

using ThisSyntaxNode = LuaSyntaxNode;
using ThisSyntaxTree = LuaSyntaxTree;
using ThisCompilation = LuaCompilation;
using ThisSemanticModel = LuaSemanticModel;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;

using ThisSyntaxNode = MoonScriptSyntaxNode;
using ThisSyntaxTree = MoonScriptSyntaxTree;
using ThisCompilation = MoonScriptCompilation;
using ThisSemanticModel = MoonScriptSemanticModel;
#endif

internal abstract partial class
#if LANG_LUA
    LuaSemanticModel
#elif LANG_MOONSCRIPT
    MoonScriptSemanticModel
#endif
    : SemanticModel
{
    /// <summary>
    /// 获取获得此语义模型的来源编译内容。
    /// </summary>
    /// <value>
    /// 编译内容的实例，其<see cref="ThisCompilation.GetSemanticModel(SyntaxTree, bool)"/>返回此语义模型。
    /// </value>
    public new abstract ThisCompilation Compilation { get; }

    /// <summary>
    /// 获取此语义模型的父语义模型。
    /// </summary>
    /// <value>
    /// 若此语义模型是推测式的，则返回其父语义模型；否则返回<see langword="null"/>。
    /// </value>
    public new abstract ThisSemanticModel? ParentModel { get; }

    /// <summary>
    /// 获取与此语义模型相关联的语法树。
    /// </summary>
    /// <value>
    /// 与此语义模型相关联的语法树。
    /// </value>
    public new abstract ThisSyntaxTree SyntaxTree { get; }

    /// <summary>
    /// 绑定基于的语法树的根节点。
    /// </summary>
    internal new abstract ThisSyntaxNode Root { get; }

    public partial ISymbol GetDeclaredSymbol(ThisSyntaxNode node, CancellationToken cancellationToken = default);

    #region 帮助方法
    /// <summary>
    /// 获取一个值，指示指定语法节点是否能被查询函数查询。
    /// </summary>
    /// <param name="node">要被查询的语法节点。</param>
    /// <param name="allowNamedArgumentName">是否允许查询有名实参的名称。</param>
    /// <param name="isSpeculative">是否是推测式查询。若传入<see langword="true"/>，则不检查<paramref name="node"/>的<see cref="ThisSyntaxNode.Parent"/>。</param>
    /// <returns>若指定语法节点能被查询函数查询，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    internal static partial bool CanGetSemanticInfo(ThisSyntaxNode node, bool allowNamedArgumentName = false, bool isSpeculative = false);

    /// <summary>
    /// 获取一个值，指示指定语法节点是否位于
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    internal bool IsInTree(ThisSyntaxNode node) => node.SyntaxTree == this.SyntaxTree;

    /// <summary>
    /// A convenience method that determines a position from a node.  If the node is missing,
    /// then its position will be adjusted using CheckAndAdjustPosition.
    /// </summary>
    protected int GetAdjustedNodePosition(ThisSyntaxNode node)
    {
        Debug.Assert(this.IsInTree(node));

        var fullSpan = this.Root.FullSpan;
        var position = node.SpanStart;

        // 跳过零宽语法标志，但不跳过语法节点末尾。
        var firstToken = node.GetFirstToken(includeZeroWidth: false);
        if (firstToken.Node is not null)
        {
            int betterPosition = firstToken.SpanStart;
            if (betterPosition < node.Span.End)
                position = betterPosition;
        }

        if (fullSpan.IsEmpty)
        {
            Debug.Assert(position == fullSpan.Start);
            return position;
        }
        else if (position == fullSpan.End)
        {
            Debug.Assert(node.Width == 0);
            // For zero-width node at the end of the full span,
            // check and adjust the preceding position.
            return CheckAndAdjustPosition(position - 1);
        }
        else if (node.IsMissing || node.HasErrors || node.Width == 0 || node.IsPartOfStructuredTrivia())
        {
            return CheckAndAdjustPosition(position);
        }
        else
        {
            // No need to adjust position.
            return position;
        }
    }

    [Conditional("DEBUG")]
    protected void AssertPositionAdjusted(int position)
    {
        Debug.Assert(position == this.CheckAndAdjustPosition(position), "Expected adjusted position");
    }

    protected int CheckAndAdjustPosition(int position) => this.CheckAndAdjustPosition(position, out _);

    protected int CheckAndAdjustPosition(int position, out SyntaxToken token)
    {
        int fullStart = this.Root.Position;
        int fullEnd = this.Root.FullSpan.End;
        bool atEOF = position == fullEnd && position == this.SyntaxTree.GetRoot().FullSpan.End;

        if ((fullStart <= position && position < fullEnd) || atEOF)
        {
            token = this.FindTokenAtPosition(
                atEOF ? this.SyntaxTree.GetRoot() : this.Root,
                position,
                atEOF);

            if (position < token.SpanStart)
            {
                // 若此时已经是第一个语法标志，则返回SyntaxToken的默认值。
                token = token.GetPreviousToken();
            }

            // 根节点可能缺失第一个语法标志，因此需防止越界。
            return Math.Max(token.SpanStart, fullStart);
        }
        else if (fullStart == fullEnd && position == fullEnd)
        {
            // 根节点的文本区域为空且不是完整的编译单元。
            token = default;
            return fullStart;
        }

        throw new ArgumentOutOfRangeException(nameof(position), position,
            string.Format(LunaResources.PositionIsNotWithinSyntax, Root.FullSpan));
    }

    private partial SyntaxToken FindTokenAtPosition(ThisSyntaxNode root, int position, bool atEOF);

    protected void CheckSyntaxNode(ThisSyntaxNode node)
    {
        if (!IsInTree(node))
            throw new ArgumentException(LunaResources.SyntaxNodeIsNotWithinSyntaxTree);
    }

    // This method ensures that the given syntax node to speculate is non-null and doesn't belong to a SyntaxTree of any model in the chain.
    private void CheckModelAndSyntaxNodeToSpeculate(ThisSyntaxNode node)
    {
        if (this.IsSpeculativeSemanticModel)
            throw new InvalidOperationException(LunaResources.ChainingSpeculativeModelIsNotSupported);

        if (this.Compilation.ContainsSyntaxTree(node.SyntaxTree))
            throw new ArgumentException(LunaResources.SpeculatedSyntaxNodeCannotBelongToCurrentCompilation);
    }

    #endregion

    #region SemanticModel
    protected sealed override Compilation CompilationCore => this.Compilation;

    protected sealed override SemanticModel? ParentModelCore => this.ParentModel;

    protected sealed override SyntaxTree SyntaxTreeCore => this.SyntaxTree;

    protected sealed override SyntaxNode RootCore => this.Root;
    #endregion
}
