using System;
using System.Collections.Generic;
using System.Text;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = MoonScriptSyntaxNode;
#endif

partial class
#if LANG_LUA
    LuaSyntaxNode
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxNode
#endif
{
    internal class MockNode : ThisInternalSyntaxNode
    {
        public MockNode() : base(0) { }
    }
}
