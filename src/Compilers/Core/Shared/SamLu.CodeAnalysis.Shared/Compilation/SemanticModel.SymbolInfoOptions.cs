#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;
#endif

partial class
#if LANG_LUA
    LuaSemanticModel
#elif LANG_MOONSCRIPT
    MoonScriptSemanticModel
#endif
{
    internal static partial void ValidateSymbolInfoOptions(SymbolInfoOptions options);
}
