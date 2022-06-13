#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisSyntaxNode = SamLu.CodeAnalysis.Lua.LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisSyntaxNode = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxNode;
#endif

/// <summary>
/// 表示已协调的节点。
/// </summary>
internal readonly struct BlendedNode
{
    internal readonly ThisSyntaxNode? Node;
    internal readonly SyntaxToken Token;
    internal readonly Blender Blender;

    internal BlendedNode(ThisSyntaxNode? node, SyntaxToken token, Blender blender)
    {
        this.Node = node;
        this.Token = token;
        this.Blender = blender;
    }
}
