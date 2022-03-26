using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax.InternalSyntax;

internal abstract partial class StructuredTriviaSyntax :
#if LANG_LUA
    LuaSyntaxNode
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxNode
#endif
{
    internal StructuredTriviaSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null) : base(kind, diagnostics, annotations) => this.Initialize();

    internal StructuredTriviaSyntax(ObjectReader reader) : base(reader) => this.Initialize();

    private void Initialize()
    {
        this.SetFlags(NodeFlags.ContainsStructuredTrivia);
    }
}
