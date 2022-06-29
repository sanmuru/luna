using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax;

public sealed partial class SkippedTokensTriviaSyntax : StructuredTriviaSyntax, ISkippedTokensTriviaSyntax
{
}
