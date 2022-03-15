namespace SamLu.CodeAnalysis.Lua;

public abstract partial class LuaSyntaxNode
{
    /// <summary>
    /// 获取内部绿树节点。
    /// </summary>
    internal Syntax.InternalSyntax.LuaSyntaxNode LuaGreen => (Syntax.InternalSyntax.LuaSyntaxNode)this.Green;
}
