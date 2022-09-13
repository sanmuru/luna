using System.Diagnostics;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax.MoonScriptSyntaxNode;
#endif

internal abstract partial class
#if LANG_LUA
    LuaSyntaxTraverser
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxTraverser
# endif
{
    protected readonly bool VisitIntoStructuredTrivia;

    public
#if LANG_LUA
    LuaSyntaxTraverser
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxTraverser
#endif
    (bool visitIntoStructuredTrivia = false) => this.VisitIntoStructuredTrivia = visitIntoStructuredTrivia;

    public override void Visit(ThisInternalSyntaxNode? node)
    {
        if (node is null) return;

        // 是否进入表示结构语法琐碎内容的语法节点进行访问。
        if (node.IsStructuredTrivia && !this.VisitIntoStructuredTrivia) return;

        base.Visit(node);
    }

    public void VisitList<TNode>(SyntaxList<TNode> list) where TNode : ThisInternalSyntaxNode
    {
        for (int i = 0, n = list.Count; i < n; i++)
        {
            var item = list[i];
            this.Visit(item);
        }
    }

    public void VisitList<TNode>(SeparatedSyntaxList<TNode> list) where TNode : ThisInternalSyntaxNode
    {
        var withSeps = (SyntaxList<ThisInternalSyntaxNode>)list.GetWithSeparators();
        this.VisitList(withSeps);
    }
}
