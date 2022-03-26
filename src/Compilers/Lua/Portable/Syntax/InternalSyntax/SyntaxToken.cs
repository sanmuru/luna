using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

internal partial class SyntaxToken
{
    internal const SyntaxKind FirstTokenWithWellKnownText = SyntaxKind.PlusToken;
    internal const SyntaxKind LastTokenWithWellKnownText = SyntaxKind.MultiLineCommentTrivia;

    private static readonly ArrayElement<SyntaxToken>[] s_tokensWithNoTrivia = new ArrayElement<SyntaxToken>[(int)SyntaxToken.LastTokenWithWellKnownText + 1];
    private static readonly ArrayElement<SyntaxToken>[] s_tokensWithElasticTrivia = new ArrayElement<SyntaxToken>[(int)SyntaxToken.LastTokenWithWellKnownText + 1];
    private static readonly ArrayElement<SyntaxToken>[] s_tokensWithSingleTrailingSpace = new ArrayElement<SyntaxToken>[(int)SyntaxToken.LastTokenWithWellKnownText + 1];
    private static readonly ArrayElement<SyntaxToken>[] s_tokensWithSingleTrailingCRLF = new ArrayElement<SyntaxToken>[(int)SyntaxToken.LastTokenWithWellKnownText + 1];

}
