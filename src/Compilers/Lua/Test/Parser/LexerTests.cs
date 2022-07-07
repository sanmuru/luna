using Microsoft.CodeAnalysis.Text;
using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace SamLu.CodeAnalysis.Lua.Parser.UnitTests;

using Utilities;

[TestClass]
public partial class LexerTests
{
    internal static Lexer CreateLexer(string source, LuaParseOptions? options = null) => new(SourceText.From(source), options ?? LuaParseOptions.Default);

    [TestMethod]
    public void SimpleLexTest()
    {
        string source = @"local i = 1";
        Lexer lexer = LexerTests.CreateLexer(source);

        SyntaxToken token;
        LexerMode mode = LexerMode.Syntax;

        token = lexer.Lex(mode);
        Assert.That.IsKeyword(token, "local");

        token = lexer.Lex(mode);
        Assert.That.IsIdentifier(token, "i");

        token = lexer.Lex(mode);
        Assert.That.IsPunctuation(token, "=");

        token = lexer.Lex(mode);
        Assert.That.IsLiteral(token);
        Assert.AreEqual(token.GetValue(), 1L);

        token = lexer.Lex(mode);
        Assert.That.IsEndOfFile(token);
    }

    #region 正向测试
    internal static void LiteralLexTest<T>(string source, T? value, LuaParseOptions? options = null)
    {
        var lexer = LexerTests.CreateLexer(source, options);
        try
        {
            var token = lexer.Lex(LexerMode.Syntax);
            // 忽略可能的负号。
            if (LexerTestUtilities.IsPunctuationCore(token, "-"))
                token = lexer.Lex(LexerMode.Syntax);
            Assert.That.IsLiteral(token);

            var tokenValue = token.GetValue();
            Assert.IsInstanceOfType(value, typeof(T));
            Assert.AreEqual((T?)tokenValue, value);
        }
        catch (AssertFailedException)
        {
            throw;
        }
    }

    [TestMethod]
    public void NumericLiteralLexTests()
    {
        LiteralLexTest(long.MaxValue.ToString(), long.MaxValue);
        LiteralLexTest(long.MinValue.ToString(), 0x8000000000000000UL); // 由于long.MinValue取负后的数字超过long.MinValue，因此返回的类型是ulong。
        LiteralLexTest(double.MaxValue.ToString("G17"), double.MaxValue); // 由于精度影响，浮点数转换为字符串时会四舍五入，因此可能会导致从字符串转型回浮点数时不相等，甚至超出最大/最小值导致抛出错误。
        Assert.AreEqual(double.MinValue.ToString("G17").Substring(1), double.MaxValue.ToString("G17"));
        LiteralLexTest(double.MinValue.ToString("G17"), double.MaxValue);

        double value = 31.4568156151E-45;
        string hexValue = value.ToHexString();
        LiteralLexTest(hexValue, value); // 十六进制浮点数。

        LiteralLexTest("31415", 31415L); // 正常整数，long类型。
        LiteralLexTest("31.415", 31.415D); // 正常十进制浮点数，double类型。
        LiteralLexTest(".314", 0.314D); // 整数部分缺失。
        LiteralLexTest("314.", 314.0D); // 小数部分缺失。
    }

    [TestMethod]
    public void StringLiteralLexTests()
    {
    }
    #endregion
}
