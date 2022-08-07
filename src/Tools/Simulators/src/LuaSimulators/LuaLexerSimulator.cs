using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SamLu.CodeAnalysis.Lua;
using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
using SyntaxToken = Microsoft.CodeAnalysis.SyntaxToken;
using LuaInternalSyntaxNode = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.LuaSyntaxNode;
using LuaInternalSyntaxToken = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.SyntaxToken;
using LuaSyntaxKind = SamLu.CodeAnalysis.Lua.SyntaxKind;
using LuaSyntaxFactory = SamLu.CodeAnalysis.Lua.SyntaxFactory;
using LuaInternalSyntaxFactory = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.SyntaxFactory;
using System.Diagnostics;

namespace Luna.Compilers.Simulators;

[LexerSimulator("Lua")]
public sealed partial class LuaLexerSimulator : ILexerSimulator
{
    private partial Lexer CreateLuaLexer(SourceText text) => new(text, LuaParseOptions.Default);

    private partial LuaInternalSyntaxToken LexNode(Lexer lexer) => lexer.Lex(LexerMode.Syntax);

    private LuaInternalSyntaxNode root = LuaInternalSyntaxFactory.Mock();
    private int position = 0;
    private int index = 0;
    private partial IEnumerable<SyntaxToken> DescendTokens(LuaInternalSyntaxToken node) => new[] { LuaSyntaxFactory.Token(this.root, node, 0, 0) };

    private TokenKind GetTokenKind(LuaSyntaxKind kind)
    {
        if (SyntaxFacts.IsKeywordKind(kind)) return TokenKind.Keyword;
        else if (SyntaxFacts.IsUnaryExpressionOperatorToken(kind) || SyntaxFacts.IsBinaryExpressionOperatorToken(kind)) return TokenKind.Operator;
        else if (SyntaxFacts.IsPunctuation(kind)) return TokenKind.Punctuation;
        else return kind switch
        {
            LuaSyntaxKind.IdentifierToken => TokenKind.Identifier,
            LuaSyntaxKind.NumericLiteralToken => TokenKind.NumericLiteral,
            LuaSyntaxKind.StringLiteralToken or
            LuaSyntaxKind.MultiLineRawStringLiteralToken => TokenKind.StringLiteral,
            LuaSyntaxKind.WhiteSpaceTrivia => TokenKind.WhiteSpace,
            LuaSyntaxKind.EndOfLineTrivia => TokenKind.NewLine,
            LuaSyntaxKind.SingleLineCommentTrivia or
            LuaSyntaxKind.MultiLineCommentTrivia => TokenKind.Comment,
            LuaSyntaxKind.SkippedTokensTrivia => TokenKind.Skipped,
            _ => TokenKind.None
        };
    }
}
