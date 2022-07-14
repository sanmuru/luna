using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Symbols;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Symbols;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Symbols;
#endif

internal abstract partial class NamespaceSymbol : NamespaceOrTypeSymbol, INamespaceSymbolInternal
{
    public virtual ImmutableArray<NamespaceSymbol> ConstituentNamespaces => ImmutableArray.Create(this);

    #region 未实现
#warning 未实现。
    internal abstract NamespaceExtent Extent { get; }

    public abstract override AssemblySymbol ContainingAssembly { get; }

    public abstract bool IsGlobalNamespace { get; }
    #endregion
}
