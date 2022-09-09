﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
namespace SamLu.CodeAnalysis.Lua.Parser.UnitTests;

using Utilities;
using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

[TestClass]
public partial class LexerTests
{
    internal static readonly LuaParseOptions DefaultParseOptions = LuaParseOptions.Default;

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

    [TestMethod]
    public void TestFilesLexTests()
    {
        foreach (var path in Directory.GetFiles("tests"))
        {
            var lexer = new Lexer(SourceText.From(File.OpenRead(path)), LuaParseOptions.Default);
            var mode = LexerMode.Syntax;

            int position = 0;
            SyntaxToken token;
            Stack<SyntaxToken> stack = new();
            do
            {
                token = lexer.Lex(mode);
                stack.Push(token);
                position += token.FullWidth;
            }
            while (token.Kind != SyntaxKind.EndOfFileToken);
        }
    }

    /// <summary>
    /// 精准抽样测试，因为发现某些语法标志被错误处理。
    /// </summary>
    [TestMethod]
    public void SampleLexTests()
    {
        { // 输入中第一个'deg'被分析成'de'和'g'，后续的正常
            var lexer = LexerTests.CreateLexer("a = to(\"tocfunction\", math.deg)deg deg");
            lexer.LexSyntaxToken();
            lexer.LexSyntaxToken();
            lexer.LexSyntaxToken();
            lexer.LexSyntaxToken();
            lexer.LexSyntaxToken();
            lexer.LexSyntaxToken();
            Assert.AreEqual("math", lexer.LexSyntaxToken().Text);
            Assert.AreEqual(".", lexer.LexSyntaxToken().Text);
            Assert.AreEqual("deg", lexer.LexSyntaxToken().Text);
            lexer.LexSyntaxToken();
            Assert.AreEqual("deg", lexer.LexSyntaxToken().Text);
            Assert.AreEqual("deg", lexer.LexSyntaxToken().Text);
        }
    }

    #region 正向测试
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

        LiteralLexTest("31415", 31415L); // 正常十进制整数，long类型。
        LiteralLexTest("0x31415ABCD", 0x31415ABCD); // 正常十六进制整数，long类型。
        LiteralLexTest("31.415", 31.415D); // 正常十进制浮点数，double类型。
        LiteralLexTest(".314", 0.314D); // 整数部分缺失。
        LiteralLexTest("314.", 314.0D); // 小数部分缺失。
    }

    [TestMethod]
    public void StringLiteralLexTests()
    {
        LiteralLexTest("""   '\a\b\f\n\r\t\v\\\"\''   """, "\a\b\f\n\r\t\v\\\"\'"); // 基本转义字符。

        LiteralLexTest("""   "as',\",\'df"   """, "as',\",'df"); // 双引号内包含的单引号可以不转义，但双引号必须转义。
        LiteralLexTest("""   'as",\',\"df'   """, "as\",',\"df"); // 单引号内包含的双引号可以不转义，但单引号必须转义。

        LiteralLexTest("""
            'first line\
            \
            third line'
            """, "first line\n\nthird line"); // 转义字面换行。
        LiteralLexTest("""
            'abso\z
                 lutely fun\z  ny!\z
            '
            """, "absolutely funny!"); // 跳过空白字符和新行字符。
        LiteralLexTest("""
            '\97o\10\049t23\043456'
            """, "ao\n1t23+456"); // 转义十进制Ascii编码。
        LiteralLexTest("""
            '\x61\x6F\n\x312\xe5\xad\xa63'
            """, "ao\n12学3"); // 转义十六进制UTF-8编码序列。
        LiteralLexTest("""
            '\u{61}o\u{A}\u{0031}t23+\u{00000000000000000000000000005B57}456'
            """, "ao\n1t23+字456"); // 转义十进制Unicode码点。

        LiteralLexTest("""
            [[first line
            second line]]
            """, "first line\nsecond line"); // 多行原始字符串。
        LiteralLexTest("""
            [===[a,[b],[[c]],[=[d]=],[==[e]==],[====[f]====],g]===]
            """, "a,[b],[[c]],[=[d]=],[==[e]==],[====[f]====],g"); // 多行原始字符串。
        LiteralLexTest("""
            [===[
            first line
            second line
            ]===]
            """, "first line\nsecond line\n"); // 字面多行，如果第一行没有字符则忽略这行。
    }

    [TestMethod]
    public void CommentLexTests()
    {
        string source = """
            --     a single line comment.       
            --[=a=[also a single line comment because of character 'a'.]=a=]
            -- [==[another single line comment because of the space before long bracket.]==]
            --[==[a multiline comment
            that,
            though contains other level of [=====[long brackets]=====], can
            cross
            many lines.
            ]==]     --last single line comment.
            """;
        Lexer lexer = LexerTests.CreateLexer(source);
        LexerMode mode = LexerMode.Syntax;
        SyntaxToken token;

        token = lexer.Lex(mode);
        var list = token.LeadingTrivia;
        Assert.IsTrue(list.Count == 9);

        Assert.IsTrue(list[0]!.Kind == SyntaxKind.SingleLineCommentTrivia);
        Assert.AreEqual(((SyntaxTrivia)list[0]!).Text, "--     a single line comment.       ");

        Assert.IsTrue(list[2]!.Kind == SyntaxKind.SingleLineCommentTrivia);
        Assert.AreEqual(((SyntaxTrivia)list[2]!).Text, "--[=a=[also a single line comment because of character 'a'.]=a=]");

        Assert.IsTrue(list[4]!.Kind == SyntaxKind.SingleLineCommentTrivia);
        Assert.AreEqual(((SyntaxTrivia)list[4]!).Text, "-- [==[another single line comment because of the space before long bracket.]==]");

        Assert.IsTrue(list[6]!.Kind == SyntaxKind.MultiLineCommentTrivia);
        Assert.AreEqual(((SyntaxTrivia)list[6]!).Text, """
            --[==[a multiline comment
            that,
            though contains other level of [=====[long brackets]=====], can
            cross
            many lines.
            ]==]
            """);

        Assert.IsTrue(list[8]!.Kind == SyntaxKind.SingleLineCommentTrivia);
        Assert.AreEqual(((SyntaxTrivia)list[8]!).Text, "--last single line comment.");
    }
    #endregion
}
