using System.Numerics;

namespace SamLu.CodeAnalysis.UnitTests;

[TestClass]
public sealed class IntegerParserTests
{
    private static readonly Random s_random = new();

    internal static BigInteger RandomBigInteger()
    {
        int sign = s_random.Next(2) * 2 - 1;
        int bits = s_random.Next(2, 310); // 稍微比double.MaxValue大一些。
        char[] chars = new char[bits];
        for (int i = 0; i < chars.Length; i++)
            chars[i] = (char)(s_random.Next(10) + '0');
        var result = BigInteger.Parse(new string(chars));

        if (sign == -1)
            return -result;
        else
            return result;
    }

    [TestMethod]
    public void TryParseDecimalInt64Tests()
    {
        Parallel.For(0, 31000, body =>
        {
            var source = IntegerParserTests.RandomBigInteger();
            string decimalStr = source.ToString();

            bool succeed = IntegerParser.TryParseDecimalInt64(decimalStr, out var result);

            Assert.IsTrue(source == result);

            if (source > long.MaxValue || source < long.MinValue)
                Assert.IsFalse(succeed, "大数字超出Int64范围，应当数字溢出导致失败！");
            else
                Assert.IsTrue(succeed, "大数字在Int64范围内，应当返回解析结果！");
        });
    }
}
