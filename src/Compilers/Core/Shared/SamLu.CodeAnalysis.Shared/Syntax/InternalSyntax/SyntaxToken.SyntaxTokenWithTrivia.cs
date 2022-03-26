using System;
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
    internal class SyntaxTokenWithTrivia : SyntaxToken
    {
        protected readonly GreenNode? LeadingField;
        protected readonly GreenNode? TrailingField;

        static SyntaxTokenWithTrivia()
        {
            ObjectBinder.RegisterTypeReader(typeof(SyntaxTokenWithTrivia), r => new SyntaxTokenWithTrivia(r));
        }

        internal SyntaxTokenWithTrivia(SyntaxKind kind, GreenNode? leading, GreenNode? trailing) : base(kind)
        {
            if (leading is not null)
            {
                this.AdjustFlagsAndWidth(leading);
                this.LeadingField = leading;
            }
            if (trailing is not null)
            {
                this.AdjustFlagsAndWidth(trailing);
                this.TrailingField = trailing;
            }
        }

        internal SyntaxTokenWithTrivia(SyntaxKind kind, GreenNode? leading, GreenNode? trailing, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations) : base(kind, diagnostics, annotations)
        {
            if (leading is not null)
            {
                this.AdjustFlagsAndWidth(leading);
                this.LeadingField = leading;
            }
            if (trailing is not null)
            {
                this.AdjustFlagsAndWidth(trailing);
                this.TrailingField = trailing;
            }
        }

        internal SyntaxTokenWithTrivia(ObjectReader reader) : base(reader)
        {
            var leading = (GreenNode?)reader.ReadValue();
            if (leading is not null)
            {
                this.AdjustFlagsAndWidth(leading);
                this.LeadingField = leading;
            }
            var trailing = (GreenNode?)reader.ReadValue();
            if (trailing is not null)
            {
                this.AdjustFlagsAndWidth(trailing);
                this.TrailingField = trailing;
            }
        }

        internal override void WriteTo(ObjectWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteValue(this.LeadingField);
            writer.WriteValue(this.TrailingField);
        }

        public sealed override GreenNode? GetLeadingTrivia() => this.LeadingField;

        public sealed override GreenNode? GetTrailingTrivia() => this.TrailingField;

        internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics) =>
            new SyntaxTokenWithTrivia(this.Kind, this.GetLeadingTrivia(), this.GetTrailingTrivia(), diagnostics, this.GetAnnotations());

        internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations) =>
            new SyntaxTokenWithTrivia(this.Kind, this.GetLeadingTrivia(), this.GetTrailingTrivia(), this.GetDiagnostics(), annotations);
    }
}
