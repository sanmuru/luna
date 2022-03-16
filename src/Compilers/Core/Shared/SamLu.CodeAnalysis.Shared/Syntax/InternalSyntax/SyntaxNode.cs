using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax.InternalSyntax;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class
#if LANG_LUA
    LuaSyntaxNode
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxNode
#endif
    : GreenNode
{
    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind) : base((ushort)kind) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind, int fullWidth) : base((ushort)kind, fullWidth) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind, DiagnosticInfo[] diagnostics) : base((ushort)kind, diagnostics) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind, DiagnosticInfo[] diagnostics, int fullWidth) : base((ushort)kind, diagnostics, fullWidth) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations) : base((ushort)kind, diagnostics, annotations) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, int fullWidth) : base((ushort)kind, diagnostics, annotations, fullWidth) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (ObjectReader reader) : base(reader) { }

    public sealed override string Language =>
#if LANG_LUA
        LanguageNames.Lua;
#elif LANG_MOONSCRIPT
        LanguageNames.MoonScript
#endif

    public SyntaxKind Kind => (SyntaxKind)this.RawKind;

    public override string KindText => this.Kind.ToString();

    public override int RawContextualKind => this.RawKind;

    public override bool IsStructuredTrivia => this is StructuredTriviaSyntax;

    public override bool IsDirective => this is DirectiveTriviaSyntax;

#warning 未完成
}
