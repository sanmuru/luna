
#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax;

using ThisSyntaxRewriter = LuaSyntaxRewriter;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax;

using ThisSyntaxRewriter = MoonScriptSyntaxRewriter;
#endif

internal class SyntaxNormalizer : ThisSyntaxRewriter
{
    internal static TNode Normalize<TNode>(TNode node, string indentWhiteSpace, string eolWhiteSpace, bool useElasticTrivia = false)
    {
#warning 未实现。
        throw new NotImplementedException();
    }
}
