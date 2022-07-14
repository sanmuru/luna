using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = MoonScriptSyntaxNode;
#endif

internal abstract partial class StructuredTriviaSyntax : ThisInternalSyntaxNode
{
    internal StructuredTriviaSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null) : base(kind, diagnostics, annotations) => this.Initialize();

    internal StructuredTriviaSyntax(ObjectReader reader) : base(reader) => this.Initialize();

    private void Initialize()
    {
        this.SetFlags(NodeFlags.ContainsStructuredTrivia);
    }
}
