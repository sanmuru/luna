using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace SamLu.CodeAnalysis.Lua.Parser.UnitTests.Utilities;

public static class LexerTestUtilities
{
    internal static void IsKeyword(this Assert assert, SyntaxToken token) =>
        Assert.IsTrue(LexerTestUtilities.IsKeywordCore(token));

    internal static void IsKeyword(this Assert assert, SyntaxToken token, string message) =>
        Assert.IsTrue(LexerTestUtilities.IsKeywordCore(token), message);

    internal static void IsKeyword(this Assert assert, SyntaxToken token, string message, params object[] parameters) =>
        Assert.IsTrue(LexerTestUtilities.IsKeywordCore(token), message, parameters);

    internal static bool IsKeywordCore(SyntaxToken token) => SyntaxFacts.IsKeywordKind(token.Kind);
}
