namespace SamLu.CodeAnalysis.Lua;

public abstract partial class LuaSyntaxNode
{
    internal Syntax.InternalSyntax.LuaSyntaxNode LuaGreen => (Syntax.InternalSyntax.LuaSyntaxNode)this.Green;

    public SyntaxKind Kind() => this.LuaGreen.Kind;
}
