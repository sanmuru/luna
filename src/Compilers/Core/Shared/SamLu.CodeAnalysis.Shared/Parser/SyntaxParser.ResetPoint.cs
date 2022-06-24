using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax.InternalSyntax;

internal partial class SyntaxParser
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
