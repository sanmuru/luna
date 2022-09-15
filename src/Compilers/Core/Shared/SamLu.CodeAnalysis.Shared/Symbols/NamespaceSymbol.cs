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

    public override void Accept(LuaSymbolVisitor visitor)
    {
        throw new NotImplementedException();
    }

    public override TResult? Accept<TResult>(LuaSymbolVisitor<TResult> visitor) where TResult : default
    {
        throw new NotImplementedException();
    }

    internal override TResult? Accept<TArgument, TResult>(LuaSymbolVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
    {
        return base.Accept(visitor, argument);
    }
    #endregion
}
