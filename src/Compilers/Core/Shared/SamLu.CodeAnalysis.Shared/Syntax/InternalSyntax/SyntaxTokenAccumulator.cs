#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using System.Collections.Generic;
using ThisInternalSyntaxAccumulator = LuaSyntaxAccumulator<SyntaxToken>;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisInternalSyntaxAccumulator = MoonScriptSyntaxAccumulator<SyntaxToken>;
#endif

internal class SyntaxTokenAccumulator : ThisInternalSyntaxAccumulator
{
    public SyntaxTokenAccumulator(bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia) { }

    public override IEnumerable<SyntaxToken> VisitToken(SyntaxToken token)
    {
        this.VisitList(token.LeadingTrivia);
        yield return token;
        this.VisitList(token.TrailingTrivia);
    }
}
