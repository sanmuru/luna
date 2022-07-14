using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Cci;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Symbols;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;
#endif

using Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract partial class Symbol : ISymbolInternal, IFormattable
{
    private ISymbol _lazySymbol;

    internal virtual UseSiteInfo<AssemblySymbol> GetUseSiteInfo() => default;

    internal static bool ReportUseSiteDiagnostic(DiagnosticInfo info, DiagnosticBag diagnostics, Location location)
    {
#warning 未实现。
        throw new NotImplementedException();
    }

    #region 未实现
#warning 未实现。
    public abstract SymbolKind Kind { get; }
    public abstract string Name { get; }
    public abstract string MetadataName { get; }
    public abstract Compilation DeclaringCompilation { get; }
    public abstract ISymbolInternal ContainingSymbol { get; }
    public abstract AssemblySymbol ContainingAssembly { get; }
    public abstract IModuleSymbolInternal ContainingModule { get; }
    public abstract INamedTypeSymbolInternal ContainingType { get; }
    public abstract INamespaceSymbolInternal ContainingNamespace { get; }
    public abstract bool IsDefinition { get; }
    public abstract ImmutableArray<Location> Locations { get; }
    public abstract bool IsImplicitlyDeclared { get; }
    public abstract Accessibility DeclaredAccessibility { get; }
    public abstract bool IsStatic { get; }
    public abstract bool IsVirtual { get; }
    public abstract bool IsOverride { get; }
    public abstract bool IsAbstract { get; }

    public abstract bool Equals(ISymbolInternal? other, TypeCompareKind compareKind);
    public abstract IReference GetCciAdapter();
    public abstract ISymbol GetISymbol();
    #endregion

    #region ISymbolInternal
    IAssemblySymbolInternal ISymbolInternal.ContainingAssembly => this.ContainingAssembly;
    #endregion

    #region IFormattable
    public sealed override string ToString()
    {
#warning 未实现。
        throw new NotImplementedException();
    }

    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => this.ToString();
    #endregion
}
