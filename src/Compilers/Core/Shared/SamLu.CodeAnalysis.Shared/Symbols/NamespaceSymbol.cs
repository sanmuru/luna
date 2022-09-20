using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Symbols;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Symbols;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Symbols;
#endif

partial class NamespaceSymbol : ModuleSymbol, INamespaceSymbolInternal
{
    /// <inheritdoc cref="ModuleSymbol()"/>
    internal NamespaceSymbol() { }

    /// <value>返回<see cref="SymbolKind.Namespace"/>。</value>
    /// <inheritdoc/>
    public sealed override SymbolKind Kind => SymbolKind.Namespace;

    #region 包含关系
    /// <remarks>命名空间符号必定不被另一个名称类型符号包含。</remarks>
    /// <value>返回<see langword="null"/>。</value>
    /// <inheritdoc/>
    public override NamedTypeSymbol? ContainingType => null;

    public abstract override AssemblySymbol ContainingAssembly { get; }
    #endregion

    /// <value>返回<see langword="false"/>。</value>
    /// <inheritdoc/>
    public sealed override bool IsAbstract => false;

    /// <value>返回<see langword="false"/>。</value>
    /// <inheritdoc/>
    public sealed override bool IsSealed => false;

    /// <value>返回<see langword="true"/>。</value>
    /// <inheritdoc/>
    public sealed override bool IsStatic => true;

    public virtual bool IsGlobalNamespace => this.ContainingModule is null;

    public sealed override bool IsImplicitlyDeclared => this.IsGlobalNamespace;

    /// <remarks>命名空间必定是公共的。</remarks>
    /// <value>返回<see cref="Accessibility.Public"/>。</value>
    /// <inheritdoc/>
    public sealed override Accessibility DeclaredAccessibility => Accessibility.Public;

    public IEnumerable<NamespaceSymbol> GetNamespaceMembers() => this.GetMembers().OfType<NamespaceSymbol>();
}
