#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;
#endif

public class
#if LANG_LUA
    LuaDiagnosticFormatter
#elif LANG_MOONSCRIPT
    MoonScriptDiagnosticFormatter
#endif
    : Microsoft.CodeAnalysis.DiagnosticFormatter
{
    internal
#if LANG_LUA
    LuaDiagnosticFormatter
#elif LANG_MOONSCRIPT
    MoonScriptDiagnosticFormatter
#endif
        ()
    { }

    public static new
#if LANG_LUA
    LuaDiagnosticFormatter
#elif LANG_MOONSCRIPT
    MoonScriptDiagnosticFormatter
#endif
        Instance
    { get; } = new();
}
