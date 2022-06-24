using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;

namespace SamLu.CodeAnalysis.Lua;

internal static class ObjectDisplay
{
    /// <summary>
    /// 获取<see langword="nil"/>的字符串表示。
    /// </summary>
    internal static string NilLiteral => "nil";

    /// <summary>
    /// 格式化布尔值字面量。
    /// </summary>
    /// <param name="value">要格式化的字面量。</param>
    /// <returns><paramref name="value"/>的字符串表示。</returns>
    internal static string FormatLiteral(bool value) =>
        value ? "true" : "false";

    /// <summary>
    /// 格式化整数字面量。
    /// </summary>
    /// <param name="value">要格式化的字面量。</param>
    /// <returns><paramref name="value"/>的字符串表示。</returns>
    /// <remarks>
    /// 当<paramref name="options"/>包含<see cref="ObjectDisplayOptions.UseHexadecimalNumbers"/>时，返回十六进制格式。
    /// </remarks>
    internal static string FormatLiteral(long value, ObjectDisplayOptions options, CultureInfo? cultureInfo = null)
    {
        var pooledBuilder = PooledStringBuilder.GetInstance();
        var sb = pooledBuilder.Builder;

        if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
        {
            sb.Append("0x");
            sb.Append(value.ToString("x16"));
        }
        else
        {
            sb.Append(value.ToString(ObjectDisplay.GetFormatCulture(cultureInfo)));
        }

        return pooledBuilder.ToStringAndFree();
    }

    /// <summary>
    /// 格式化浮点数字面量。
    /// </summary>
    /// <param name="value">要格式化的字面量。</param>
    /// <returns><paramref name="value"/>的字符串表示。</returns>
    /// <remarks>
    /// 当<paramref name="options"/>包含<see cref="ObjectDisplayOptions.UseHexadecimalNumbers"/>时，返回十六进制格式。
    /// </remarks>
    internal static string FormatLiteral(double value, ObjectDisplayOptions options, CultureInfo? cultureInfo = null)
    {
        var pooledBuilder = PooledStringBuilder.GetInstance();
        var sb = pooledBuilder.Builder;

        if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
        {
            sb.Append("0x");
            sb.Append(value.ToHexString());
        }
        else
        {
            sb.Append(value.ToString(ObjectDisplay.GetFormatCulture(cultureInfo)));
        }

        return pooledBuilder.ToStringAndFree();
    }

    /// <summary>
    /// 将双精度浮点数转化为十六进制字符串格式。
    /// </summary>
    public static string ToHexString(this double value)
    {
        var pooledBuilder = PooledStringBuilder.GetInstance();
        var sb = pooledBuilder.Builder;

        if (double.IsNaN(value))
        {
            sb.Append("nan");
        }
        else
        {
            if (value < 0)
            {
                sb.Append('-');
                value = -value;
            }

            if (double.IsInfinity(value))
            {
                sb.Append("inf");
            }
            else
            {
                sb.Append("0x");

                var pooledBuilder2 = PooledStringBuilder.GetInstance();
                var sb2 = pooledBuilder2.Builder;

                #region 整数部分
                double trunc = Math.Floor(value);
                value -= trunc;
                while (trunc >= 16)
                {
                    byte reminder = (byte)(trunc % 16);
                    sb2.Append(reminder.ToString("X"));
                    trunc = Math.Floor(trunc / 16);
                }
                if ((int)trunc != 0)
                    sb2.Append(Convert.ToString((int)trunc, 16));

                char[] buff = new char[sb2.Length];
                sb2.CopyTo(0, buff, 0, buff.Length);
                Array.Reverse(buff);
                #endregion

                sb2.Clear();

                #region 小数部分
                byte hexdigit;
                while (value != 0)
                {
                    value *= 16;
                    hexdigit = (byte)value;
                    sb2.Append(hexdigit.ToString("X"));
                    value -= hexdigit;
                }
                char[] buff2 = pooledBuilder2.ToStringAndFree().TrimEnd('0').ToCharArray();
                #endregion

                if (buff.Length == 0)
                    sb.Append('0');
                else
                    sb.Append(buff);

                sb.Append('.');

                if (buff2.Length == 0)
                    sb.Append('0');
                else
                    sb.Append(buff2);
            }
        }

        return pooledBuilder.ToStringAndFree();
    }

    /// <summary>
    /// 获取格式化的文化信息。
    /// </summary>
    /// <remarks>
    /// 默认使用<see cref="CultureInfo.InvariantCulture"/>。
    /// </remarks>
    private static CultureInfo GetFormatCulture(CultureInfo? cultureInfo) => cultureInfo ?? CultureInfo.InvariantCulture;
}
