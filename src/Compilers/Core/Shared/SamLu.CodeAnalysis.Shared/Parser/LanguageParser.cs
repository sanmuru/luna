using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisSyntaxNode = SamLu.CodeAnalysis.Lua.LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisSyntaxNode = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxNode;
#endif

internal partial class LanguageParser : SyntaxParser
{
    private readonly SyntaxListPool _pool = new();

    private readonly SyntaxFactoryContext _syntaxFactoryContext;
    private readonly ContextAwareSyntax _syntaxFactory;

    private int _recursionDepth; // 递归深度。
    private TerminatorState _terminatorState;

    internal LanguageParser(
        Lexer lexer,
        ThisSyntaxNode? oldTree,
        IEnumerable<TextChangeRange>? changes,
        LexerMode lexerMode = LexerMode.Syntax,
        CancellationToken cancellationToken = default
    ) : base(
        lexer,
        lexerMode,
        oldTree,
        changes,
        allowModeReset: false,
        preLexIfNotIncremental: true,
        cancellationToken: cancellationToken
    )
    {
        this._syntaxFactoryContext = new();
        this._syntaxFactory = new(this._syntaxFactoryContext);
    }
}
