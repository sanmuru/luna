using System.Diagnostics;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;
#endif

/// <summary>
/// 定义一系列用于编译器决定如何处理Unicode字符的方法。
/// </summary>
public static partial class SyntaxFacts
{
    /// <inheritdoc cref="CharacterInfo.IsNewLine(char)"/>
    internal static bool IsNewLine(char c) => c.IsNewLine();

    /// <inheritdoc cref="CharacterInfo.IsHexDigit(char)"/>
    internal static bool IsHexDigit(char c) => c.IsHexDigit();

    /// <inheritdoc cref="CharacterInfo.IsBinaryDigit(char)"/>
    internal static bool IsBinaryDigit(char c) => c.IsBinaryDigit();

    /// <inheritdoc cref="CharacterInfo.IsDecDigit(char)"/>
    internal static bool IsDecDigit(char c) => c.IsDecDigit();

    /// <inheritdoc cref="CharacterInfo.HexValue(char)"/>
    internal static int HexValue(char c) => c.HexValue();

    /// <inheritdoc cref="CharacterInfo.BinaryValue(char)"/>
    internal static int BinaryValue(char c) => c.BinaryValue();

    /// <inheritdoc cref="CharacterInfo.DecValue(char)"/>
    internal static int DecValue(char c) => c.DecValue();
}
