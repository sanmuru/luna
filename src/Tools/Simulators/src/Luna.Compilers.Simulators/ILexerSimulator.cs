using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Luna.Compilers.Simulators;

public interface ILexerSimulator
{
    IEnumerable<SyntaxToken> LexToEnd(SourceText text);

    TokenKind GetTokenKind(int rawKind);
}
