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
    /// <summary>
    /// 指定的Unicode字符是否是十六进制数字的数位。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns>若<paramref name="c"/>的值是十六进制数字的数位（0-9、A-F、a-f）则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    internal static bool IsHexDigit(char c) =>
        c switch
        {
            >= '0' and <= '9' => true,
            >= 'A' and <= 'F' => true,
            >= 'a' and <= 'f' => true,
            _ => false
        };

    /// <summary>
    /// 指定的Unicode字符是否是二进制数字的数位。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns>若<paramref name="c"/>的值是二进制数字的数位（0或1）则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    internal static bool IsBinaryDigit(char c) =>
        c == '0' || c == '1';

    /// <summary>
    /// 指定的Unicode字符是否是十进制数字的数位。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns>若<paramref name="c"/>的值是十进制数字的数位（0-9）则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    internal static bool IsDecDigit(char c) =>
        c >= '0' && c <= '9';

    /// <summary>
    /// 获取指定的Unicode字符表示的十六进制数字的数位的值。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns><paramref name="c"/>表示的十六进制数字的数位的值。</returns>
    internal static int HexValue(char c)
    {
        Debug.Assert(SyntaxFacts.IsHexDigit(c));
        return c switch
        {
            >= '0' and <= '9' => c - '0',
            _ => (c & 0xdf) - 'A' + 10
        };
    }

    /// <summary>
    /// 获取指定的Unicode字符表示的二进制数字的数位的值。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns><paramref name="c"/>表示的二进制数字的数位的值。</returns>
    internal static int BinaryValue(char c)
    {
        Debug.Assert(SyntaxFacts.IsBinaryDigit(c));
        return c - '0';
    }

    /// <summary>
    /// 获取指定的Unicode字符表示的十进制数字的数位的值。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns><paramref name="c"/>表示的十进制数字的数位的值。</returns>
    internal static int DecValue(char c)
    {
        Debug.Assert(SyntaxFacts.IsDecDigit(c));
        return c - '0';
    }
}
