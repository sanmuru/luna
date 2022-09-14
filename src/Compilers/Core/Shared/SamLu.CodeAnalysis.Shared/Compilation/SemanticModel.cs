using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;

using ThisSyntaxNode = LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript

using ThisSyntaxNode = MoonScriptSyntaxNode;
#endif

internal abstract partial class
#if LANG_LUA
    LuaSemanticModel
#elif LANG_MOONSCRIPT
    MoonScriptSemanticModel
#endif
    : SemanticModel
{
    public partial ISymbol GetDeclaredSymbol(ThisSyntaxNode node, CancellationToken cancellationToken = default);
}
