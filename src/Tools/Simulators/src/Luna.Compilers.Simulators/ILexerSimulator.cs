using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Luna.Compilers.Simulators;

public interface ILexerSimulator
{
    IEnumerable<SyntaxToken> LexToEnd(SourceText text);

    IAsyncEnumerable<SyntaxToken> LexToEndAsync(SourceText text, CancellationToken cancellationToken);

    TokenKind GetTokenKind(int rawKind);
}
