using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;

using ThisSyntaxNode = LuaSyntaxNode;
using ThisCompilation = LuaCompilation;
using ThisSemanticModel = LuaSemanticModel;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;

using ThisSyntaxNode = MoonScriptSyntaxNode;
using ThisCompilation = MoonScriptCompilation;
using ThisSemanticModel = MoonScriptSemanticModel;
#endif

using Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract partial class Symbol : ISymbolInternal, IFormattable, IEquatable<Symbol>
{
    private ISymbol? _lazyISymbol;

    #region 名称
    /// <summary>
    /// 获取此符号的名称。
    /// </summary>
    /// <value>
    /// 此符号的名称。
    /// 若此符号没有名称，则返回空字符串，而非<see langword="null"/>。
    /// </value>
    public virtual string Name => string.Empty;

    /// <summary>
    /// 获取此符号在元数据中的名称。
    /// </summary>
    /// <remarks>
    /// 通常情况下，此符号在元数据中的名称与<see cref="Name"/>相等。
    /// </remarks>
    public virtual string MetadataName => this.Name;
    #endregion

    /// <summary>
    /// 获取此符号在元数据中表示的标志。
    /// </summary>
    /// <value>
    /// 此符号在元数据中表示的标志。通常情况下符号并未从元数据中加载，这种情况下元数据标志为<c>0</c>。
    /// </value>
    public virtual int MetadataToken => 0;

    /// <summary>
    /// 获取此符号的类型。
    /// </summary>
    public abstract SymbolKind Kind { get; }

    #region 包含关系
    /// <summary>
    /// 获取逻辑上包含了此符号的符号。
    /// </summary>
    /// <value>
    /// 逻辑上包含了此符号的符号。
    /// 若此符号不属于任何符号，则返回<see langword="null"/>。
    /// </value>
    public abstract Symbol? ContainingSymbol { get; }

    /// <summary>
    /// 获取逻辑上包含了此符号的模块符号。
    /// </summary>
    /// <value>
    /// 逻辑上包含了此符号的模块符号。
    /// 若此符号不属于任何模块符号，则返回<see langword="null"/>。
    /// </value>
    public virtual ModuleSymbol? ContainingModule =>
        // 注：此基类仅提供了基础的递归获取逻辑，子类应尽可能提供更高效率的实现以重写此逻辑。
        this.GetContainingSymbolHelper<ModuleSymbol>();

    /// <summary>
    /// 获取逻辑上包含了此符号的.NET模块符号。
    /// </summary>
    /// <value>
    /// 逻辑上包含了此符号的.NET模块符号。
    /// 若此符号不属于任何.NET模块，或在多个.NET模块间共享，则返回<see langword="null"/>。
    /// </value>
    internal virtual NetModuleSymbol? ContainingNetModule =>
        // 注：此基类仅提供了基础的递归获取逻辑，子类应尽可能提供更高效率的实现以重写此逻辑。
        this.ContainingSymbol?.ContainingNetModule;

    /// <summary>
    /// 获取逻辑上包含了此符号的程序集符号。
    /// </summary>
    /// <value>
    /// 逻辑上包含了此符号的程序集符号。
    /// 若此符号不属于任何程序集，或在多个程序集间共享，则返回<see langword="null"/>。
    /// </value>
    public virtual AssemblySymbol? ContainingAssembly =>
        // 注：此基类仅提供了基础的递归获取逻辑，子类应尽可能提供更高效率的实现以重写此逻辑。
        this.ContainingSymbol?.ContainingAssembly;

    /// <summary>
    /// 递归获取最近的指定符号。
    /// </summary>
    /// <typeparam name="TSymbol">符号的类型。</typeparam>
    private TSymbol? GetContainingSymbolHelper<TSymbol>() where TSymbol : Symbol
    {
        for (var container = this.ContainingSymbol; container is not null; container = container.ContainingSymbol)
        {
            if (container is TSymbol symbol)
                return symbol;
        }
        return null;
    }
    #endregion

    #region 声明关系
    /// <summary>
    /// 获取声明此符号的可访问性。
    /// </summary>
    /// <value>
    /// 声明此符号的可访问性。若此符号不适用可访问性，则返回<see cref="Accessibility.NotApplicable"/>。
    /// </value>
    public abstract Accessibility DeclaredAccessibility { get; }

    /// <summary>
    /// 获取声明此符号的语法引用列表。
    /// </summary>
    /// <value>
    /// <para>声明此符号的语法引用列表。</para>
    /// <para>
    /// 某些符号可能有多个原始定义的位置，因此属性可能返回多个语法引用，前提是它们定义在源代码中，并且不是隐式定义（<see cref="IsImplicitlyDeclared"/>）。
    /// </para>
    /// <para>
    /// 当符号定义在元数据中或隐式定义在源代码中，属性将返回空的只读数组。
    /// </para>
    /// </value>
    /// <remarks>
    /// 反向获取符号（语法节点获取定义的符号）请见<see
    /// cref="ThisSemanticModel.GetDeclaredSymbol(ThisSyntaxNode, CancellationToken)"/>.
    /// </remarks>
    public abstract ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

    /// <summary>
    /// 获取声明指定语法节点的语法位置列表。
    /// </summary>
    /// <remarks>
    /// 此方法是<see cref="DeclaringSyntaxReferences"/>的实现，由子类调用。
    /// </remarks>
    internal static ImmutableArray<SyntaxReference> GetDeclaringSyntaxReferenceHelper<TNode>(ImmutableArray<Location> locations)
        where TNode : ThisSyntaxNode
    {
        if (locations.IsEmpty)
            return ImmutableArray<SyntaxReference>.Empty;

        ArrayBuilder<SyntaxReference> builder = ArrayBuilder<SyntaxReference>.GetInstance();
        foreach (Location location in locations)
        {
            if (!location.IsInSource) continue;

            if (location.SourceSpan.Length != 0)
            {
                SyntaxToken token = location.SourceTree.GetRoot().FindToken(location.SourceSpan.Start);
                if (token.Kind() != SyntaxKind.None)
                {
                    var node = token.Parent?.FirstAncestorOrSelf<TNode>();
                    if (node is not null)
                        builder.Add(node.GetReference());
                }
            }
            else
            {
                SyntaxNode parent = location.SourceTree.GetRoot();
                SyntaxNode? found = null;
                foreach (var descendant in parent.DescendantNodesAndSelf(c => c.Location.SourceSpan.Contains(location.SourceSpan)))
                {
                    if (descendant is TNode && descendant.Location.SourceSpan.Contains(location.SourceSpan))
                        found = descendant;
                }

                if (found is not null)
                    builder.Add(found.GetReference());
            }
        }

        return builder.ToImmutableAndFree();
    }

    /// <summary>
    /// 获取声明此符号的编译内容。
    /// </summary>
    /// <value>
    /// 声明此符号的编译内容。
    /// </value>
    public virtual ThisCompilation? DeclaringCompilation
    {
        get
        {
            if (!this.IsDefinition)
                return this.OriginalDefinition.DeclaringCompilation;

            switch (this.Kind)
            {
                case SymbolKind.ErrorType:
                    return null;
                case SymbolKind.Assembly:
                    Debug.Assert(this is not SourceAssemblySymbol, "SourceAssemblySymbol must override DeclaringCompilation");
                    return null;
                case SymbolKind.NetModule:
                    Debug.Assert(this is not SourceModuleSymbol, "SourceModuleSymbol must override DeclaringCompilation");
                    return null;
            }

            switch (this.ContainingNetModule)
            {
                case SourceModuleSymbol sourceModuleSymbol:
                    return sourceModuleSymbol.DeclaringCompilation;
            }

            return null;
        }
    }
    #endregion

    #region 定义
    /// <summary>
    /// 获取此符号的原始定义。
    /// </summary>
    /// <value>
    /// 此符号的原始定义。
    /// 若此符号是由另一个符号通过类型置换创建的，则返回其最原始的在源代码或元数据中的定义。
    /// </value>
    public Symbol OriginalDefinition => this.OriginalSymbolDefinition;

    /// <summary>
    /// 由子类重写，获取此符号的原始定义。
    /// </summary>
    /// <value>
    /// <see cref="OriginalDefinition"/>使用此返回值。
    /// </value>
    protected virtual Symbol OriginalSymbolDefinition => this;

    /// <summary>
    /// 获取一个值，只是此符号是否为原始定义。
    /// </summary>
    /// <value>
    /// 若此符号为原始定义，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </value>
    public bool IsDefinition => object.ReferenceEquals(this, this.OriginalDefinition);
    #endregion

    /// <summary>
    /// 获取此符号的原始定义的位置。
    /// </summary>
    /// <value>
    /// 此符号的原始定义的位置。可能在源代码或元数据中；某些符号也可能有多个原始定义的位置。
    /// </value>
    public abstract ImmutableArray<Location> Locations { get; }

    /// <summary>
    /// <para>
    /// Get a source location key for sorting. For performance, it's important that this
    /// be able to be returned from a symbol without doing any additional allocations (even
    /// if nothing is cached yet.)
    /// </para>
    /// <para>
    /// Only (original) source symbols and namespaces that can be merged
    /// need implement this function if they want to do so for efficiency.
    /// </para>
    /// </summary>
    internal virtual LexicalSortKey GetLexicalSortKey()
    {
        var locations = this.Locations;
        var declaringCompilation = this.DeclaringCompilation;
        Debug.Assert(declaringCompilation != null); // require that it is a source symbol
        return (locations.Length > 0) ? new LexicalSortKey(locations[0], declaringCompilation) : LexicalSortKey.NotInSource;
    }

    #region 符号完成
    /// <summary>
    /// 获取一个值，指示此符号是否应当被完成。
    /// </summary>
    /// <value>
    /// 当此符号应当被完成时，返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// 直观上来说，（任何编译中的）所有源代码项都返回<see langword="true"/>。
    /// </value>
    internal virtual bool RequiresCompletion => false;

    /// <summary>
    /// 调用以强制完成此符号。
    /// </summary>
    /// <param name="location">此符号的位置。若传入<see langword="null"/>，则表示不指定位置。</param>
    /// <param name="cancellationToken">取消操作的标志。</param>
    internal virtual void ForceComplete(SourceLocation? location, CancellationToken cancellationToken)
    {
        // 源代码符号必须重写此方法。
        Debug.Assert(!this.RequiresCompletion);
    }

    /// <summary>
    /// 此符号是否包含对应的符号完成。
    /// </summary>
    /// <param name="part">要查找的符号完成的类型。</param>
    /// <returns>若此符号是否包含对应的符号完成，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    internal virtual bool HasComplete(CompletionPart part)
    {
        // 源代码符号必须重写此方法。
        Debug.Assert(!this.RequiresCompletion);
        return true;
    }
    #endregion

    /// <summary>
    /// 获取一个值，指示此符号是否为静态的。
    /// </summary>
    /// <value>
    /// 若此符号是静态的，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </value>
    public abstract bool IsStatic { get; }

    /// <summary>
    /// 获取一个值，指示此符号是否为虚拟的。
    /// </summary>
    /// <value>
    /// 若此符号是虚拟的，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </value>
    public abstract bool IsVirtual { get; }

    /// <summary>
    /// 获取一个值，指示此符号是否为重写的。
    /// </summary>
    /// <value>
    /// 若此符号是重写的，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </value>
    public abstract bool IsOverride { get; }

    /// <summary>
    /// 获取一个值，指示此符号是否为抽象的。
    /// </summary>
    /// <value>
    /// 若此符号是抽象的，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </value>
    public abstract bool IsAbstract { get; }

    /// <summary>
    /// 获取一个值，指示此符号是否为封闭的。
    /// </summary>
    /// <value>
    /// 若此符号是封闭的，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </value>
    public abstract bool IsSealed { get; }

    /// <summary>
    /// 获取一个值，指示此符号是否为外部的。
    /// </summary>
    /// <value>
    /// 若此符号是外部的，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </value>
    public abstract bool IsExtern { get; }

    /// <summary>
    /// 获取一个值，指示此符号是否为隐式声明的。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 若此符号是否为隐式声明的，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </para>
    /// <para>
    /// 隐式声明指由编译器生成，且在源代码中没有对应的显式声明。
    /// </para>
    /// </remarks>
    public virtual bool IsImplicitlyDeclared => false;

    #region 相等项
    public static bool operator ==(Symbol? left, Symbol? right) => Symbol.Equals(left, right, SymbolEqualityComparer.Default.CompareKind);

    public static bool operator !=(Symbol? left, Symbol? right) => !(left == right);

    public sealed override bool Equals(object? obj) => this.Equals(obj as Symbol);

    public bool Equals(Symbol? other) => this.Equals(other, SymbolEqualityComparer.Default.CompareKind);

    bool ISymbolInternal.Equals(ISymbolInternal? other, TypeCompareKind compareKind) => this.Equals(other as Symbol, compareKind);

    public virtual bool Equals(Symbol? other, TypeCompareKind compareKind) => object.ReferenceEquals(this, other);

    public static bool Equals(Symbol? first, Symbol? second, TypeCompareKind compareKind)
    {
        if (first is null)
            return second is null;
        else if (object.ReferenceEquals(first, second))
            return true;
        else
            return first.Equals(second, compareKind);
    }

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
    #endregion

    #region 公共符号
    internal ISymbol ISymbol
    {
        get
        {
            if (this._lazyISymbol is null)
                Interlocked.CompareExchange(ref _lazyISymbol, this.CreateISymbol(), null);

            return _lazyISymbol;
        }
    }

    protected abstract ISymbol CreateISymbol();
    #endregion

    #region 访问方法
    public abstract void Accept(
#if LANG_LUA
        LuaSymbolVisitor
#elif LANG_MOONSCRIPT
        MoonScriptSymbolVisitor
#endif
        visitor);

    public abstract TResult? Accept<TResult>(
#if LANG_LUA
        LuaSymbolVisitor
#elif LANG_MOONSCRIPT
        MoonScriptSymbolVisitor
#endif
        <TResult> visitor);

    internal abstract TResult? Accept<TArgument, TResult>(
#if LANG_LUA
        LuaSymbolVisitor
#elif LANG_MOONSCRIPT
        MoonScriptSymbolVisitor
#endif
        <TArgument, TResult> visitor, TArgument argument);
    #endregion

    internal virtual UseSiteInfo<AssemblySymbol> GetUseSiteInfo() => default;

    internal static bool ReportUseSiteDiagnostic(DiagnosticInfo info, DiagnosticBag diagnostics, Location location)
    {
#warning 未实现。
        throw new NotImplementedException();
    }

#region 未实现
#warning 未实现。
    public abstract IReference GetCciAdapter();
    #endregion

    /// <remarks>防止除编译器外创建实例。</remarks>
    internal Symbol() { }

    #region ISymbolInternal
#nullable disable
    ISymbolInternal ISymbolInternal.ContainingSymbol => this.ContainingSymbol;
    INamespaceSymbolInternal ISymbolInternal.ContainingNamespace => null;
    INamedTypeSymbolInternal ISymbolInternal.ContainingType => this.ContainingType;
    IModuleSymbolInternal ISymbolInternal.ContainingModule => this.ContainingNetModule;
    IAssemblySymbolInternal ISymbolInternal.ContainingAssembly => this.ContainingAssembly;
    Compilation ISymbolInternal.DeclaringCompilation => this.DeclaringCompilation;
    ISymbol ISymbolInternal.GetISymbol() => this.ISymbol;
#nullable enable
    #endregion

#region IFormattable
    /*
    public sealed override string ToString() => this.ToDisplayString();

    public string ToDisplayString(SymbolDisplayFormat? format = null) =>
        SymbolDisplay.ToDisplayString(this.ISymbol, format);

    public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat? format = null) =>
        SymbolDisplay.ToDisplayParts(this.ISymbol, format);

    public string ToMinimalDisplayString(
        SemanticModel semanticModel,
        int position,
        SymbolDisplayFormat? format = null) =>
        SymbolDisplay.ToMinimalDisplayString(this.ISymbol, semanticModel, position, format);

    public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(
        SemanticModel semanticModel,
        int position,
        SymbolDisplayFormat? format = null) =>
        SymbolDisplay.ToMinimalDisplayParts(this.ISymbol, semanticModel, position, format);
    */
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => this.ToString();
#endregion
}
