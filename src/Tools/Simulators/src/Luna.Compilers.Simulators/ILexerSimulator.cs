using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Luna.Compilers.Simulators;

public interface ILexerSimulator
{
    void Initialize(LexerSimulatorContext context);

    IEnumerable<SyntaxToken> LexToEnd(SourceText sourceText);

    TokenKind GetTokenKind(int rawKind);
}
