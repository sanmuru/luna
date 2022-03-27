using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

internal static partial class SyntaxFactory
{
    internal static SyntaxToken Literal(GreenNode? leading, string text, int value, GreenNode? trailing) => SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);

    internal static SyntaxToken Literal(GreenNode? leading, string text, long value, GreenNode? trailing) => SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);

    internal static SyntaxToken Literal(GreenNode? leading, string text, float value, GreenNode? trailing) => SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);

    internal static SyntaxToken Literal(GreenNode? leading, string text, double value, GreenNode? trailing) => SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);

    internal static SyntaxToken Literal(GreenNode? leading, string text, string value, GreenNode? trailing) => SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);

    internal static SyntaxToken Literal(GreenNode? leading, string text, SyntaxKind kind, string value, GreenNode? trailing) => SyntaxToken.WithValue(kind, leading, text, value, trailing);

}
