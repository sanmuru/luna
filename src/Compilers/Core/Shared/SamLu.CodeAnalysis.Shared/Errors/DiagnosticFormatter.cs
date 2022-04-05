namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    ;

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
