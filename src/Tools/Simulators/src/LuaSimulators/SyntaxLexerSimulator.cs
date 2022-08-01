using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SamLu.CodeAnalysis.Lua;
using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
using SyntaxToken = Microsoft.CodeAnalysis.SyntaxToken;
using LuaSyntaxToken = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.SyntaxToken;
using LuaSyntaxKind = SamLu.CodeAnalysis.Lua.SyntaxKind;
using LuaSyntaxFactory = SamLu.CodeAnalysis.Lua.SyntaxFactory;
using System.Diagnostics;

namespace Luna.Compilers.Simulators;

[LexerSimulator("Lua")]
public sealed partial class SyntaxLexerSimulator : ILexerSimulator
{
    private partial Lexer CreateLuaLexer(SourceText text) => new(text, LuaParseOptions.Default);

    private partial LuaSyntaxToken LexNode(Lexer lexer) => lexer.Lex(LexerMode.Syntax);

    private partial IEnumerable<SyntaxToken> DescendTokens(LuaSyntaxToken node) => new[] { LuaSyntaxFactory.Token(node) };

    private TokenKind GetTokenKind(LuaSyntaxKind kind)
    {
        Debug.Assert(SyntaxFacts.IsAnyToken(kind));

        if (SyntaxFacts.IsKeywordKind(kind)) return TokenKind.Keyword;
        else if (SyntaxFacts.IsUnaryExpressionOperatorToken(kind) || SyntaxFacts.IsBinaryExpressionOperatorToken(kind)) return TokenKind.Operator;
        else if (SyntaxFacts.IsPunctuation(kind)) return TokenKind.Punctuation;
        else return kind switch
        {
            LuaSyntaxKind.NumericLiteralToken => TokenKind.NumericLiteral,
            LuaSyntaxKind.StringLiteralToken or
            LuaSyntaxKind.MultiLineRawStringLiteralToken => TokenKind.StringLiteral,
            LuaSyntaxKind.WhiteSpaceTrivia => TokenKind.WhiteSpace,
            LuaSyntaxKind.SingleLineCommentTrivia or
            LuaSyntaxKind.MultiLineCommentTrivia => TokenKind.Comment,
            LuaSyntaxKind.SkippedTokensTrivia => TokenKind.Skipped,
            _ => TokenKind.None
        };
    }
}
