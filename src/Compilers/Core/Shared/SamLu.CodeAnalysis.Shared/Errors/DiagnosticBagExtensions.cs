using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;

using ThisDiagnostic = LuaDiagnostic;
using ThisDiagnosticInfo = LuaDiagnosticInfo;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;

using ThisDiagnostic = MoonScriptDiagnostic;
using ThisDiagnosticInfo = MoonScriptDiagnosticInfo;
#endif

internal static class DiagnosticBagExtensions
{
    internal static ThisDiagnosticInfo Add(this DiagnosticBag diagnostics, ErrorCode code, Location location)
    {
        var info = new ThisDiagnosticInfo(code);
        var diag = new ThisDiagnostic(info, location);
        diagnostics.Add(diag);
        return info;
    }

    internal static ThisDiagnosticInfo Add(this DiagnosticBag diagnostics, ErrorCode code, Location location, params object[] args)
    {
        var info = new ThisDiagnosticInfo(code, args);
        var diag = new ThisDiagnostic(info, location);
        diagnostics.Add(diag);
        return info;
    }

    internal static ThisDiagnosticInfo Add(this DiagnosticBag diagnostics, ErrorCode code, Location location, ImmutableArray<Symbol> symbols, params object[] args)
    {
        var info = new ThisDiagnosticInfo(code, args, symbols, ImmutableArray<Location>.Empty);
        var diag = new ThisDiagnostic(info, location);
        diagnostics.Add(diag);
        return info;
    }

    internal static void Add(this DiagnosticBag diagnostics, DiagnosticInfo info, Location location)
    {
        var diag = new ThisDiagnostic(info, location);
        diagnostics.Add(diag);
    }

    internal static bool Add(this DiagnosticBag diagnostic, SyntaxNode node, HashSet<DiagnosticInfo> useSiteDiagnostics) =>
        !useSiteDiagnostics.IsNullOrEmpty() && diagnostic.Add(node.Location, useSiteDiagnostics);

    internal static bool Add(this DiagnosticBag diagnostic, SyntaxToken token, HashSet<DiagnosticInfo> useSiteDiagnostics) =>
        !useSiteDiagnostics.IsNullOrEmpty() && diagnostic.Add(token.GetLocation(), useSiteDiagnostics);

    internal static bool Add(this DiagnosticBag diagnostic, Location location, IReadOnlyCollection<DiagnosticInfo> useSiteDiagnostics)
    {
        if (useSiteDiagnostics.IsNullOrEmpty()) return false;

        bool haveErrors = false;

        foreach (var info in useSiteDiagnostics)
        {
            if (info.Severity == DiagnosticSeverity.Error)
                haveErrors = true;

            diagnostic.Add(new ThisDiagnostic(info, location));
        }

        return haveErrors;
    }
}
