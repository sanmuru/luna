#if LANG_LUA
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua;

using ThisParseOptions = LuaParseOptions;
using ThisCompilation = LuaCompilation;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;

using ThisParseOptions = MoonScriptParseOptions;
using ThisCompilation = MoonScriptCompilation;
#endif

internal static class
#if LANG_LUA
    LuaCompilationExtensions
#elif LANG_MOONSCRIPT
    MoonScriptCompilationExtensions
#endif
{
    internal static bool IsFeatureEnabled(this ThisCompilation compilation, MessageID feature)
    {
        return ((ThisParseOptions?)compilation.SyntaxTrees.FirstOrDefault()?.Options)?.IsFeatureEnabled(feature) == true;
    }

    internal static bool IsFeatureEnabled(this SyntaxNode? syntax, MessageID feature)
    {
        return ((ThisParseOptions?)syntax?.SyntaxTree.Options)?.IsFeatureEnabled(feature) == true;
    }

#warning 未完成
}
