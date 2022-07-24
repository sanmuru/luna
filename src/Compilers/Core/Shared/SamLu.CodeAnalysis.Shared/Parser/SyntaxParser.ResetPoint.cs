using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;
#endif

partial class SyntaxParser
{
    protected struct ResetPoint
    {
        internal readonly int ResetCount;
        internal readonly LexerMode Mode;
        internal readonly int Position;
        internal readonly GreenNode? PrevTokenTrailingTrivia;

        internal ResetPoint(
            int resetCount,
            LexerMode mode,
            int position,
            GreenNode? prevTokenTrailingTrivia
        )
        {
            this.ResetCount = resetCount;
            this.Mode = mode;
            this.Position = position;
            this.PrevTokenTrailingTrivia = prevTokenTrailingTrivia;
        }
    }
}
