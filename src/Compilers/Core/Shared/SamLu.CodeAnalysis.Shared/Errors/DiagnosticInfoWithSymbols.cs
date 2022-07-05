using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;
#endif

using ThisMessageProvider = MessageProvider;

internal class DiagnosticInfoWithSymbols : DiagnosticInfo
{
    internal readonly ImmutableArray<Symbol> Symbols;

    internal DiagnosticInfoWithSymbols(ErrorCode code, object[] arguments, ImmutableArray<Symbol> symbols) : base(ThisMessageProvider.Instance, (int)code, arguments) => this.Symbols = symbols;

    internal DiagnosticInfoWithSymbols(bool isWarningAsError, ErrorCode code, object[] arguments, ImmutableArray<Symbol> symbols) : base(ThisMessageProvider.Instance, isWarningAsError, (int)code, arguments) => this.Symbols = symbols;
}
