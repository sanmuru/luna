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

    protected static partial void InitializeTokensWithWellKnownText()
    {
        for (var kind = SyntaxToken.FirstTokenWithWellKnownText; kind <= SyntaxToken.LastTokenWithWellKnownText; kind++)
        {
            SyntaxToken.s_tokensWithNoTrivia[(int)kind].Value = new SyntaxToken(kind);
            SyntaxToken.s_tokensWithElasticTrivia[(int)kind].Value = new SyntaxTokenWithTrivia(kind, SyntaxFactory.ElasticZeroSpace, SyntaxFactory.ElasticZeroSpace);
            SyntaxToken.s_tokensWithSingleTrailingSpace[(int)kind].Value = new SyntaxTokenWithTrivia(kind, null, SyntaxFactory.Space);
            SyntaxToken.s_tokensWithSingleTrailingCRLF[(int)kind].Value = new SyntaxTokenWithTrivia(kind, null, SyntaxFactory.CarriageReturnLineFeed);
        }
    }

    internal static partial IEnumerable<SyntaxToken> GetWellKnownTokens()
    {
        foreach (var token in SyntaxToken.s_tokensWithNoTrivia)
        {
            if (token.Value is not null) yield return token.Value;
        }

        foreach (var token in SyntaxToken.s_tokensWithElasticTrivia)
        {
            if (token.Value is not null) yield return token.Value;
        }

        foreach (var token in SyntaxToken.s_tokensWithSingleTrailingSpace)
        {
            if (token.Value is not null) yield return token.Value;
        }

        foreach (var token in SyntaxToken.s_tokensWithSingleTrailingCRLF)
        {
            if (token.Value is not null) yield return token.Value;
        }
    }
}
