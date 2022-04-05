using System.Text;
using Microsoft.CodeAnalysis.Text;

#if LANG_LUA
using ThisSyntaxNode = SamLu.CodeAnalysis.Lua.LuaSyntaxNode;
using ThisParseOptions = SamLu.CodeAnalysis.Lua.LuaParseOptions;

namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
using ThisSyntaxNode = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxNode;
using ThisParseOptions = SamLu.CodeAnalysis.MoonScript.MoonScriptParseOptions;

namespace SamLu.CodeAnalysis.MoonScript;
#endif

public partial class
#if LANG_LUA
    LuaSyntaxTree
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxTree
#endif
{
    private sealed class DebuggerSyntaxTree : ParsedSyntaxTree
    {
        internal override bool SupportsLocations => true;

        internal DebuggerSyntaxTree(
            ThisSyntaxNode root,
            SourceText text,
            ThisParseOptions options
        ) : base(
            text,
            text.Encoding,
            text.ChecksumAlgorithm,
            path: string.Empty,
            options: options,
            root: root,
            cloneRoot: true)
        {
        }
    }
}
