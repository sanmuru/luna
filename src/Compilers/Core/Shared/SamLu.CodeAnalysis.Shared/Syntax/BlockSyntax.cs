using Microsoft.CodeAnalysis;

#if LANG_LUA
using ThisSyntaxNode = SamLu.CodeAnalysis.Lua.LuaSyntaxNode;

namespace SamLu.CodeAnalysis.Lua.Syntax;
#elif LANG_MOONSCRIPT
using ThisSyntaxNode = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxNode;

namespace SamLu.CodeAnalysis.MoonScript.Syntax;
#endif

public sealed partial class BlockSyntax : ThisSyntaxNode, ICompilationUnitSyntax
{
}
