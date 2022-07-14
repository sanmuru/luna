using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = MoonScriptSyntaxNode;
#endif

internal static class
#if LANG_LUA
    LuaSyntaxNodeCache
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxNodeCache
#endif
{
    internal static GreenNode? TryGetNode(int kind, GreenNode? child1, SyntaxFactoryContext context, out int hash) =>
        SyntaxNodeCache.TryGetNode(kind, child1, GetNodeFlags(context), out hash);

    internal static GreenNode? TryGetNode(int kind, GreenNode? child1, GreenNode? child2, SyntaxFactoryContext context, out int hash) =>
        SyntaxNodeCache.TryGetNode(kind, child1, child2, GetNodeFlags(context), out hash);

    internal static GreenNode? TryGetNode(int kind, GreenNode? child1, GreenNode? child2, GreenNode? child3, SyntaxFactoryContext context, out int hash) =>
        SyntaxNodeCache.TryGetNode(kind, child1, child2, child3, GetNodeFlags(context), out hash);

    private static GreenNode.NodeFlags GetNodeFlags(SyntaxFactoryContext context) =>
        ThisInternalSyntaxNode.SetFactoryContext(SyntaxNodeCache.GetDefaultNodeFlags(), context);
}
