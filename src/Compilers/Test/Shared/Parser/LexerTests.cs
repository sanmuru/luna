using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

#if LANG_LUA
using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
using ThisParseOptions = SamLu.CodeAnalysis.Lua.LuaParseOptions;

namespace SamLu.CodeAnalysis.Lua.Parser.UnitTests;
#elif LANG_MOONSCRIPT
using SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;
using ThisParseOptions = SamLu.CodeAnalysis.MoonScript.MoonScriptParseOptions;

namespace SamLu.CodeAnalysis.MoonScript.Parser.UnitTests;
#endif

using Utilities;

partial class LexerTests
{
    internal static Lexer CreateLexer(string source, ThisParseOptions? options = null) => new(SourceText.From(source), options ?? LexerTests.DefaultParseOptions);

    internal static void LiteralLexTest<T>(string source, T? value, ThisParseOptions? options = null)
    {
        var lexer = LexerTests.CreateLexer(source, options);

        var token = lexer.Lex(LexerMode.Syntax);
        // 忽略可能的负号。
        if (LexerTestUtilities.IsPunctuationCore(token, "-"))
            token = lexer.Lex(LexerMode.Syntax);
        Assert.That.IsLiteral(token, value);
    }
}
