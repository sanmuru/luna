
namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax.InternalSyntax;

internal partial class SyntaxToken :
#if LANG_LUA
    LuaSyntaxNode
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxNode
#endif
{
}
