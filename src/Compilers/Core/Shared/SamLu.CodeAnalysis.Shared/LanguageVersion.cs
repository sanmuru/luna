using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    ;

internal static partial class LanguageVersionExtensionsInternal
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsValid(this LanguageVersion value) => Enum.IsDefined(typeof(LanguageVersion), value);

    internal static partial ErrorCode GetErrorCode(this LanguageVersion version);
}

internal sealed partial class
#if LANG_LUA
    LuaRequiredLanguageVersion
#elif LANG_MOONSCRIPT
    MoonScriptRequiredLanguageVersion
#endif
    : RequiredLanguageVersion
{
    internal LanguageVersion Version { get; init; }

    internal
#if LANG_LUA
        LuaRequiredLanguageVersion
#elif LANG_MOONSCRIPT
        MoonScriptRequiredLanguageVersion
#endif
        (LanguageVersion version) => this.Version = version;

    public override string ToString() => this.Version.ToDisplayString();
}

public static partial class LanguageVersionFacts
{
    /// <summary>
    /// 返回在控制行中（开启/langver开关）显示文本的格式一致的版本数字。
    /// 例如："5"、"5.4"、"latest"。
    /// </summary>
    /// <param name="version">要获取显示文本的语言版本。</param>
    /// <returns>语言版本的显示文本。</returns>
    public static partial string ToDisplayString(this LanguageVersion version);

    /// <summary>
    /// 尝试从字符串输入中分析出<see cref="LanguageVersion"/>，若<paramref name="result"/>为<see langword="null"/>时返回<see cref="LanguageVersion.Default"/>。
    /// </summary>
    /// <param name="version">字符串输入。</param>
    /// <param name="result">分析出的语言版本。</param>
    /// <returns></returns>
    public static partial bool TryParse(string? version, out LanguageVersion result);

    /// <summary>
    /// 将一个特定的语言版本（例如<see cref="LanguageVersion.Default"/>、<see cref="LanguageVersion.Latest"/>）映射到一个具体的版本。
    /// </summary>
    /// <param name="version">要映射的语言版本。</param>
    /// <returns></returns>
    public static partial LanguageVersion MapSpecifiedToEffectiveVersion(this LanguageVersion version);
}
