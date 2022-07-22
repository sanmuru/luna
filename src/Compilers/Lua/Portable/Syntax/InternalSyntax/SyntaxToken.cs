using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

internal partial class SyntaxToken
{
    internal static SyntaxToken StringLiteral(string text) => new SyntaxTokenWithValue<string>(SyntaxKind.StringLiteralToken, text, text);

    internal static SyntaxToken StringLiteral(LuaSyntaxNode leading, string text, LuaSyntaxNode trailing) => new SyntaxTokenWithValueAndTrivia<string>(SyntaxKind.StringLiteralToken, text, text, leading, trailing);

    internal const SyntaxKind FirstTokenWithWellKnownText = SyntaxKind.PlusToken;
    internal const SyntaxKind LastTokenWithWellKnownText = SyntaxKind.MultiLineCommentTrivia;
}
