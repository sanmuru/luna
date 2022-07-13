using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;
#endif

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
