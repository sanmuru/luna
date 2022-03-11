using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    internal abstract class LuaSyntaxNode : GreenNode
    {
        internal LuaSyntaxNode(SyntaxKind kind) : base((ushort)kind) => GreenStats.NoteGreen(this);

        internal LuaSyntaxNode(SyntaxKind kind, int fullWidth) : base((ushort)kind, fullWidth) => GreenStats.NoteGreen(this);

        internal LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics) : base((ushort)kind, diagnostics) => GreenStats.NoteGreen(this);

        internal LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, int fullWidth) : base((ushort)kind, diagnostics, fullWidth) => GreenStats.NoteGreen(this);

        internal LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations) : base((ushort)kind, diagnostics, annotations) => GreenStats.NoteGreen(this);

        internal LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, int fullWidth) : base((ushort)kind, diagnostics, annotations, fullWidth) => GreenStats.NoteGreen(this);

        internal LuaSyntaxNode(ObjectReader reader) : base(reader) { }

        public override string Language => LanguageNames.Lua;
        public SyntaxKind Kind => (SyntaxKind)this.RawKind;

        public override string KindText => this.Kind.ToString();

#warning 未完成
    }
}
