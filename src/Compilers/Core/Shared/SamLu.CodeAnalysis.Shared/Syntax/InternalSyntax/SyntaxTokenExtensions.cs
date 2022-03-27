using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax.InternalSyntax;

internal partial class SyntaxToken
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void InitializeWithTrivia(
        SyntaxToken self,
        ref GreenNode? _leading, ref GreenNode? _trailing,
        GreenNode? leading = null, GreenNode? trailing = null
    )
    {
        if (leading is not null)
        {
            self.AdjustFlagsAndWidth(leading);
            _leading = leading;
        }
        if (trailing is not null)
        {
            self.AdjustFlagsAndWidth(trailing);
            _trailing = trailing;
        }
    }
}
