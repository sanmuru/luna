using System;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax.InternalSyntax;

internal partial class SyntaxToken
{
    internal class MissingTokenWithTrivia : SyntaxTokenWithTrivia
    {
        public sealed override string Text => string.Empty;

        static MissingTokenWithTrivia()
        {
            ObjectBinder.RegisterTypeReader(typeof(MissingTokenWithTrivia), r => new MissingTokenWithTrivia(r));
        }

        internal MissingTokenWithTrivia(SyntaxKind kind, GreenNode? leading, GreenNode? trailing) : base(kind, leading, trailing)
        {
            this.ClearIsNotMissingFlag();
        }

        internal MissingTokenWithTrivia(SyntaxKind kind, GreenNode? leading, GreenNode? trailing, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations) : base(kind, leading, trailing, diagnostics, annotations)
        {
            this.ClearIsNotMissingFlag();
        }

        internal MissingTokenWithTrivia(ObjectReader reader) : base(reader)
        {
            this.ClearIsNotMissingFlag();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearIsNotMissingFlag() => this.ClearFlags(NodeFlags.IsNotMissing);

        internal override void WriteTo(ObjectWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteValue(this.LeadingField);
            writer.WriteValue(this.TrailingField);
        }

        internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics) =>
            new MissingTokenWithTrivia(this.Kind, this.GetLeadingTrivia(), this.GetTrailingTrivia(), diagnostics, this.GetAnnotations());

        internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations) =>
            new MissingTokenWithTrivia(this.Kind, this.GetLeadingTrivia(), this.GetTrailingTrivia(), this.GetDiagnostics(), annotations);
    }
}
