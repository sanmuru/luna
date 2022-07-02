﻿using System.Diagnostics.CodeAnalysis;

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
    /// <inheritdoc cref="CharacterInfo.IsWhiteSpace(char)"/>
    public static bool IsWhiteSpace(char c) => c.IsWhiteSpace();

    /// <inheritdoc cref="CharacterInfo.IsNewLine(char)"/>
    public static bool IsNewLine(char c) => c.IsNewLine();

    /// <summary>
    /// 指定的多个字符序列是否表示新行。
    /// </summary>
    /// <param name="firstChar">第一个Unicode字符。</param>
    /// <param name="restChars">后续的Unicode字符序列。</param>
    /// <returns>若<paramref name="firstChar"/>和<paramref name="restChars"/>组成的字符序列表示新行则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    public static partial bool IsNewLine(char firstChar, params char[] restChars);

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

    /// <summary>
    /// 指定的Unicode字符是否可以是标识符的第一个字符。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns>若<paramref name="c"/>的值可以是标识符的第一个字符则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    public static partial bool IsIdentifierStartCharacter(char c);

    /// <summary>
    /// 指定的Unicode字符是否可以是标识符的后续字符。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns>若<paramref name="c"/>的值可以是标识符的后续字符则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    public static partial bool IsIdentifierPartCharacter(char c);

    /// <summary>
    /// 指定的名称是否是一个合法的标识符。
    /// </summary>
    /// <param name="name">一个标识符名称。</param>
    /// <returns>若<paramref name="name"/>表示的是一个合法的标识符则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    public static partial bool IsValidIdentifier([NotNullWhen(true)] string? name);
}
