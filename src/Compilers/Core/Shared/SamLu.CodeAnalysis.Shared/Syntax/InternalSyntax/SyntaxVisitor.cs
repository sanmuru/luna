#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = MoonScriptSyntaxNode;
#endif

using SamLu.CodeAnalysis.Syntax.InternalSyntax;

internal abstract partial class
#if LANG_LUA
    LuaSyntaxVisitor
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxVisitor
#endif
    <TResult> : CommonSyntaxVisitor<TResult, ThisInternalSyntaxNode>
{
    public override TResult? Visit(ThisInternalSyntaxNode? node)
    {
        if (node is null) return default;

        return node.Accept(this);
    }

    public virtual TResult? VisitToken(SyntaxToken token) => this.DefaultVisit(token);

    public virtual TResult? VisitTrivia(SyntaxTrivia trivia) => this.DefaultVisit(trivia);

    protected override TResult? DefaultVisit(ThisInternalSyntaxNode node) => default;
}

internal abstract partial class
#if LANG_LUA
    LuaSyntaxVisitor
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxVisitor
#endif
    : CommonSyntaxVisitor<ThisInternalSyntaxNode>
{
    public override void Visit(ThisInternalSyntaxNode? node)
    {
        if (node is null) return;

        node.Accept(this);
    }

    public virtual void VisitToken(SyntaxToken token) => this.DefaultVisit(token);

    public virtual void VisitTrivia(SyntaxTrivia trivia) => this.DefaultVisit(trivia);
}
