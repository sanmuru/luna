using System.Text;
using Microsoft.CodeAnalysis.Text;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;

using ThisSyntaxNode = LuaSyntaxNode;
using ThisParseOptions = LuaParseOptions;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;

using ThisSyntaxNode = MoonScriptSyntaxNode;
using ThisParseOptions = MoonScriptParseOptions;
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
