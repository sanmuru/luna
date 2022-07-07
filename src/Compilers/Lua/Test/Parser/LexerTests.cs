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
        LiteralLexTest("31415", 31415L);
        LiteralLexTest(long.MaxValue.ToString(), long.MaxValue);
        LiteralLexTest(long.MinValue.ToString(), 0x8000000000000000); // 由于long.MinValue取负后的数字超过long.MinValue，因此返回的类型是ulong。
        LiteralLexTest(double.MaxValue.ToString(), double.MaxValue);
    }

    [TestMethod]
    public void StringLiteralLexTests()
    {
    }
    #endregion
}
