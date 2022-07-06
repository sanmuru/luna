using System.Globalization;
using System.Numerics;

namespace SamLu.CodeAnalysis;

internal static class IntegerParser
{
    public static bool TryParseHexadecimalInt64(string s, out long l)
    {
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(2);
        return long.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out l);
    }

    public static bool TryParseDecimalInt64(string s, out BigInteger bigInteger)
    {
        if (BigInteger.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out bigInteger))
            return bigInteger >= long.MinValue && bigInteger <= long.MaxValue;
        else
            return false;
    }
}
