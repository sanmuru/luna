namespace SamLu.CodeAnalysis.MoonScript;

public abstract partial class MoonScriptSyntaxNode
{
    /// <summary>
    /// 获取内部绿树节点。
    /// </summary>
    internal Syntax.InternalSyntax.MoonScriptSyntaxNode MoonScriptGreen => (Syntax.InternalSyntax.MoonScriptSyntaxNode)this.Green;
}
