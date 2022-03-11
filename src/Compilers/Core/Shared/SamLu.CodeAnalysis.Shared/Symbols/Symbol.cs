using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Symbols;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    ;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract partial class Symbol : ISymbolInternal, IFormattable
{
    private ISymbol _lazySymbol;
}
