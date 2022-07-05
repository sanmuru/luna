using System.Text;
using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;

using ThisParseOptions = LuaParseOptions;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;

using ThisParseOptions = MoonScriptParseOptions;
#endif

using Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;

static partial class SyntaxFactory
{
    private static SourceText MakeSourceText(string text, int offset)
    {
        return SourceText.From(text, Encoding.UTF8).GetSubText(offset);
    }

    private static Lexer MakeLexer(string text, int offset, ThisParseOptions? options = null)
    {
        return new Lexer(
            text: MakeSourceText(text, offset),
            options: options ?? ThisParseOptions.Default);
    }

    private static LanguageParser MakeParser(Lexer lexer)
    {
        return new LanguageParser(lexer, oldTree: null, changes: null);
    }

}
