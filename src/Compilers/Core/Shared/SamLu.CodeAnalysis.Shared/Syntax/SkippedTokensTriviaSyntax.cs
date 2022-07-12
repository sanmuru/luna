using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax;
#endif

public sealed partial class SkippedTokensTriviaSyntax : ISkippedTokensTriviaSyntax { }
