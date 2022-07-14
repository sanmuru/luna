using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;

using ThisSyntaxNode = LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;

using ThisSyntaxNode = MoonScriptSyntaxNode;
#endif

internal static class SyntaxNodeExtensions
{
    public static TNode WithAnnotations<TNode>(this TNode node, params SyntaxAnnotation[] annotations) where TNode : ThisSyntaxNode =>
        (TNode)node.Green.SetAnnotations(annotations).CreateRed();
}
