
#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;
#endif

partial class Lexer
{
    internal const int MaxCachedTokenSize = 42;

    private readonly Func<SyntaxToken> _createQuickTokenFunction;

    private SyntaxToken CreateQuickToken()
    {
#warning 未实现。
        throw new NotImplementedException();
    }

    private SyntaxToken? QuickScanSyntaxToken()
    {
#warning 未实现。
        return null;
        throw new NotImplementedException();
    }
}
