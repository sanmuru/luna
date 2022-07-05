using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Symbols;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Symbols;
#endif

#warning 未实现。
internal readonly struct NamespaceExtent
{
    private readonly NamespaceKind _kind;

    public NamespaceKind Kind => this._kind;
}
