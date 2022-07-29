using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SamLu.CodeAnalysis.Lua;

namespace Luna.Compilers.Simulators;

[LexerSimulator("Lua")]
public sealed class SyntaxLexerSimulator : ILexerSimulator
{
    public TokenKind GetTokenKind(SyntaxKind kind) => kind switch
    {

    };

    public IEnumerable<SyntaxToken> LexToEnd(SourceText text)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<SyntaxToken> LexToEndAsync(SourceText text, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    TokenKind ILexerSimulator.GetTokenKind(int rawKind) => this.GetTokenKind((SyntaxKind)rawKind);
}
