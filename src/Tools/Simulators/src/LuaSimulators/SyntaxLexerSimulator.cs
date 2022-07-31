using Microsoft.CodeAnalysis.Text;
using SamLu.CodeAnalysis.Lua;
using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace Luna.Compilers.Simulators;

[LexerSimulator("Lua")]
public sealed partial class SyntaxLexerSimulator : ILexerSimulator
{
    private partial Lexer CreateLexer(SourceText text) => new(text, LuaParseOptions.Default);
}
