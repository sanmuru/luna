﻿using System.Diagnostics;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax.MoonScriptSyntaxNode;
#endif

internal partial class
#if LANG_LUA
    LuaSyntaxRewriter : LuaSyntaxVisitor<ThisInternalSyntaxNode>
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxRewriter : MoonScriptSyntaxVisitor<ThisInternalSyntaxNode>
#endif
{
    protected readonly bool VisitIntoStructuredTrivia;

    public
#if LANG_LUA
    LuaSyntaxRewriter
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxRewriter
#endif
    (bool visitIntoStructuredTrivia = false) => this.VisitIntoStructuredTrivia = visitIntoStructuredTrivia;

    public override ThisInternalSyntaxNode? VisitToken(SyntaxToken token)
    {
        var leading = this.VisitList(token.LeadingTrivia);
        var trailing = this.VisitList(token.TrailingTrivia);

        if (leading != token.LeadingTrivia)
            token = token.TokenWithLeadingTrivia(leading.Node);

        if (trailing != token.TrailingTrivia)
            token = token.TokenWithTrailingTrivia(trailing.Node);

        return token;
    }

    public SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list) where TNode : ThisInternalSyntaxNode
    {
        SyntaxListBuilder? alternate = null;
        for (int i = 0, n = list.Count; i < n; i++)
        {
            var item = list[i];
            var visited = this.Visit(item);
            if (item != visited && alternate is null)
            {
                alternate = new(n);
                alternate.AddRange(list, 0, i);
            }
            else if (alternate is not null)
            {
                Debug.Assert(visited is not null && visited.Kind != SyntaxKind.None, "无法移除节点。");
                alternate.Add(visited);
            }
        }

        if (alternate is not null)
            return alternate.ToList();

        return list;
    }

    public SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> list) where TNode : ThisInternalSyntaxNode
    {
        var withSeps = (SyntaxList<ThisInternalSyntaxNode>)list.GetWithSeparators();
        var result = this.VisitList(withSeps);
        if (result != withSeps)
            return result.AsSeparatedList<TNode>();

        return list;
    }
}
