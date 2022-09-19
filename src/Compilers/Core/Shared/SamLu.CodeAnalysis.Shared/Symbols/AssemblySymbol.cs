﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Symbols;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Symbols;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Symbols;
#endif

partial class AssemblySymbol : Symbol, IAssemblySymbolInternal
{
    /// <inheritdoc cref="Symbol.Symbol()"/>
    internal AssemblySymbol() { }

    /// <summary>
    /// 获取此程序集符号的名称。
    /// </summary>
    /// <value>
    /// 此程序集符号的名称。
    /// </value>
    public override string Name => this.Identity.Name;

    /// <value>返回<see cref="SymbolKind.NetModule"/>。</value>
    /// <inheritdoc/>
    public sealed override SymbolKind Kind => SymbolKind.Assembly;

    /// <remarks>程序集符号必定不被另一个符号包含。</remarks>
    /// <value>返回<see langword="null"/>。</value>
    /// <inheritdoc/>
    public sealed override Symbol? ContainingSymbol => null;

    /// <remarks>程序集必定不被另一个程序集包含。</remarks>
    /// <value>返回<see langword="null"/>。</value>
    /// <inheritdoc/>
    public sealed override AssemblySymbol? ContainingAssembly => null;

    /// <summary>
    /// 获取此程序集符号的程序集身份。
    /// </summary>
    /// <value>
    /// 此程序集符号的程序集身份。
    /// </value>
    public abstract AssemblyIdentity Identity { get; }

    /// <summary>
    /// 获取此程序集符号的程序集版本模式。
    /// </summary>
    /// <value>
    /// 此程序集符号的程序集版本模式。
    /// <para>当在<see cref="AssemblyVersionAttribute"/>中指定的程序集版本字符串包含“<c>*</c>”时，“<c>*</c>”将被替换为<see cref="ushort.MaxValue"/>；</para>
    /// <para>当程序集版本字符串不包含“<c>*</c>”时返回<see langword="null"/>。</para>
    /// </value>
    /// <example>
    /// <c>AssemblyVersion("1.2.*")</c>将被表示为“1.2.65535.65535”
    /// <c>AssemblyVersion("1.2.3.*")</c>将被表示为“1.2.3.65535”
    /// </example>
    public abstract Version? AssemblyVersionPattern { get; }

    /// <summary>
    /// 获取此程序集符号中的所有模块。
    /// </summary>
    /// <value>
    /// 此程序集符号中的所有模块。
    /// <para>返回值至少包含一个项。</para>
    /// <para>返回值第一项表示此程序集中包含清单的主模块。</para>
    /// </value>
    public abstract ImmutableArray<ModuleSymbol> Modules { get; }

    /// <summary>
    /// 获取此程序集符号的目标机器架构。
    /// </summary>
    /// <value>
    /// 此程序集符号的目标机器架构。
    /// </value>
    internal Machine Machine => this.Modules[0].Machine;

    /// <inheritdoc cref="ModuleSymbol.Bit32Required"/>
    internal bool Bit32Required => this.Modules[0].Bit32Required;

    /// <summary>
    /// 获取一个值，指示此程序集是否缺失。
    /// </summary>
    /// <value>
    /// 若此此程序集缺失，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </value>
    internal abstract bool IsMissing { get; }

    /// <value>返回<see cref="Accessibility.NotApplicable"/>。</value>
    /// <inheritdoc/>
    public sealed override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

    /// <value>返回<see langword="false"/>。</value>
    /// <inheritdoc/>
    public sealed override bool IsAbstract => false;

    /// <value>返回<see langword="false"/>。</value>
    /// <inheritdoc/>
    public sealed override bool IsExtern => false;

    /// <value>返回<see langword="false"/>。</value>
    /// <inheritdoc/>
    public sealed override bool IsOverride => false;
    /// <value>返回<see langword="false"/>。</value>
    /// <inheritdoc/>
    public sealed override bool IsSealed => false;

    /// <value>返回<see langword="false"/>。</value>
    /// <inheritdoc/>
    public sealed override bool IsStatic => false;

    /// <value>返回<see langword="false"/>。</value>
    /// <inheritdoc/>
    public sealed override bool IsVirtual => false;

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

    #region 未实现
#warning 未实现。
    public abstract IAssemblySymbolInternal CorLibrary { get; }
    #endregion
}
