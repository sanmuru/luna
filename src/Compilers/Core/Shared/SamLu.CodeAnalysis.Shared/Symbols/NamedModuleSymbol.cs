using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Cci;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Symbols;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Symbols;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Symbols;
#endif

using Symbols;

partial class NamedModuleSymbol : INamedTypeSymbolInternal
{
    public abstract INamedTypeSymbolInternal EnumUnderlyingType { get; }
    public abstract TypeKind TypeKind { get; }
    public abstract SpecialType SpecialType { get; }
    public abstract bool IsReferenceType { get; }
    public abstract bool IsValueType { get; }

    public abstract ITypeSymbol GetITypeSymbol();
}
