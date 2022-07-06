using Microsoft.CodeAnalysis.Text;
using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace SamLu.CodeAnalysis.Lua.Parser.UnitTests;

using Utilities;

[TestClass]
public partial class LexerTests
{
    [TestMethod]
    public void SimpleLex()
    {
        string source = "local i = 1";
        SourceText text = SourceText.From(source);
        Lexer lexer = new(text, LuaParseOptions.Default);

        SyntaxToken token;

        token = lexer.Lex(LexerMode.Syntax);
        Assert.That.IsKeyword(token);
    }
}
