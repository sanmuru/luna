using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

partial class Lexer
{
    private sealed class InterpolationToken : SyntaxToken.SyntaxTokenWithValueAndTrivia<Interpolation>
    {
        static InterpolationToken() => ObjectBinder.RegisterTypeReader(typeof(InterpolationToken), r => new InterpolationToken(r));

        internal InterpolationToken(ObjectReader reader) : base(reader) { }

        internal InterpolationToken(string text, in Interpolation value, GreenNode? leading, GreenNode? trailing) : base(SyntaxKind.InterpolationToken, text, value, leading, trailing) { }

        internal InterpolationToken(string text, in Interpolation value, GreenNode? leading, GreenNode? trailing, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations) : base(SyntaxKind.InterpolationToken, text, value, leading, trailing, diagnostics, annotations) { }

        public override SyntaxToken TokenWithLeadingTrivia(GreenNode? trivia) =>
            new InterpolationToken(this._text, this._value, trivia, this.GetTrailingTrivia(), this.GetDiagnostics(), this.GetAnnotations());

        public override SyntaxToken TokenWithTrailingTrivia(GreenNode? trivia) =>
            new InterpolationToken(this._text, this._value, this.GetLeadingTrivia(), trivia, this.GetDiagnostics(), this.GetAnnotations());

        internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics) =>
            new InterpolationToken(this._text, this._value, this.GetLeadingTrivia(), this.GetTrailingTrivia(), diagnostics, this.GetAnnotations());

        internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations) =>
             new InterpolationToken(this._text, this._value, this.GetLeadingTrivia(), this.GetTrailingTrivia(), this.GetDiagnostics(), annotations);
    }
}
