using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax;

using ThisSyntaxNode = LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax;

using ThisSyntaxNode = MoonScriptSyntaxNode;
#endif

internal static class SyntaxNodeRemover
{
    internal static TRoot? RemoveNodes<TRoot>(
        TRoot root,
        IEnumerable<ThisSyntaxNode>? nodes,
        SyntaxRemoveOptions options)
        where TRoot : ThisSyntaxNode
    {
#warning 未实现。
        throw new NotImplementedException();
    }
}
