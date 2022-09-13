using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua;

using InternalSyntax = Syntax.InternalSyntax;

static partial class SyntaxEquivalence
{
    private static partial bool AreTokensEquivalentCore(GreenNode before, GreenNode after, SyntaxKind kind) =>
        kind switch
        {
            SyntaxKind.IdentifierToken => ((InternalSyntax.SyntaxToken)before).ValueText == ((InternalSyntax.SyntaxToken)after).ValueText,

            SyntaxKind.NumericLiteralToken or
            SyntaxKind.StringLiteralToken or
            SyntaxKind.MultiLineRawStringLiteralToken => ((InternalSyntax.SyntaxToken)before).Text == ((InternalSyntax.SyntaxToken)after).Text,

            _ => true
        };

    private static partial bool TryAreTopLevelEquivalent(GreenNode before, GreenNode after, SyntaxKind kind, ref Func<SyntaxKind, bool>? ignoreChildNode, out bool equivalence)
    {
        equivalence = false;
        return false;
    }
}
