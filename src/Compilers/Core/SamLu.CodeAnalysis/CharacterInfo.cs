using System.Diagnostics;

namespace SamLu.CodeAnalysis;

internal static class CharacterInfo
{
    /// <summary>
    /// 指定的Unicode字符是否表示新行。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns>若<paramref name="c"/>的值表示新行则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    public static bool IsNewLine(this char c) =>
        c switch
        {
            '\r' or           // 回车符（U+000D）
            '\n' or           // 换行符（U+000A）
            '\u0085' or       // 新行符（U+0085）
            '\u2028' or       // 分行负（U+2028）
            '\u2029' => true, // 分段符（U+2029）
            _ => false
        };

    /// <summary>
    /// 指定的Unicode字符是否是十六进制数字的数位。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns>若<paramref name="c"/>的值是十六进制数字的数位（0-9、A-F、a-f）则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    public static bool IsHexDigit(this char c) =>
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
    public static bool IsBinaryDigit(this char c) =>
        c == '0' || c == '1';

    /// <summary>
    /// 指定的Unicode字符是否是十进制数字的数位。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns>若<paramref name="c"/>的值是十进制数字的数位（0-9）则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    public static bool IsDecDigit(this char c) =>
        c >= '0' && c <= '9';

    /// <summary>
    /// 获取指定的Unicode字符表示的十六进制数字的数位的值。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns><paramref name="c"/>表示的十六进制数字的数位的值。</returns>
    public static int HexValue(this char c)
    {
        Debug.Assert(c.IsHexDigit());
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
    internal static int BinaryValue(this char c)
    {
        Debug.Assert(c.IsBinaryDigit());
        return c - '0';
    }

    /// <summary>
    /// 获取指定的Unicode字符表示的十进制数字的数位的值。
    /// </summary>
    /// <param name="c">一个Unicode字符。</param>
    /// <returns><paramref name="c"/>表示的十进制数字的数位的值。</returns>
    internal static int DecValue(this char c)
    {
        Debug.Assert(c.IsDecDigit());
        return c - '0';
    }
}
