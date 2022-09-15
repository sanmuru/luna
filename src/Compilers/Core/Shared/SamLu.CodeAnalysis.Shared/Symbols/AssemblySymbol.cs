using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Symbols;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Symbols;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Symbols;
#endif

internal abstract partial class AssemblySymbol : IAssemblySymbolInternal
{
    internal abstract bool IsMissing { get; }

    #region 未实现
#warning 未实现。
    public abstract Version? AssemblyVersionPattern { get; }
    public abstract AssemblyIdentity Identity { get; }
    public abstract IAssemblySymbolInternal CorLibrary { get; }
    #endregion
}
