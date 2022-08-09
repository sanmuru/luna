using Microsoft.CodeAnalysis;
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

    private bool IsIncrementalAndFactoryContextMatches
    {
        get
        {
            if (!base.IsIncremental) return false;

            var node = this.CurrentNode;
            return node is not null && this.MatchFactoryContext(node.Green, this._syntaxFactoryContext);
        }
    }

    private partial bool MatchFactoryContext(GreenNode green, SyntaxFactoryContext context);

#if TESTING
    internal
#else
    private
#endif
        bool IsTerminal()
    {
        if (this.CurrentTokenKind == SyntaxKind.EndOfFileToken) return true;

        for (int i = 1; i < LanguageParser.LastTerminatorState; i <<= 1)
        {
            var state = (TerminatorState)i;
            if (IsTerminalCore(state)) return true;
        }

        return false;
    }

    private partial bool IsTerminalCore(TerminatorState state);
}
