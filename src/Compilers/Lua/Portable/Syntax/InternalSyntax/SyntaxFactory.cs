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

    internal static partial IEnumerable<SyntaxTrivia> GetWellKnownTrivia()
    {
        yield return SyntaxFactory.CarriageReturnLineFeed;
        yield return SyntaxFactory.LineFeed;
        yield return SyntaxFactory.CarriageReturn;
        yield return SyntaxFactory.Space;
        yield return SyntaxFactory.Tab;

        yield return SyntaxFactory.ElasticCarriageReturnLineFeed;
        yield return SyntaxFactory.ElasticLineFeed;
        yield return SyntaxFactory.ElasticCarriageReturn;
        yield return SyntaxFactory.ElasticSpace;
        yield return SyntaxFactory.ElasticTab;

        yield return SyntaxFactory.ElasticZeroSpace;
    }
}
