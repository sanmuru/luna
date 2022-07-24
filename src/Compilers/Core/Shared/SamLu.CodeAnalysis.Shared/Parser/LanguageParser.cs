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
    protected readonly SyntaxListPool pool = new();

    protected readonly SyntaxFactoryContext syntaxFactoryContext;
    protected readonly ContextAwareSyntax syntaxFactory;

    protected int recursionDepth; // 递归深度。
    protected TerminatorState terminatorState;

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
        this.syntaxFactoryContext = new();
        this.syntaxFactory = new(this.syntaxFactoryContext);
    }

    private protected bool IsIncrementalAndFactoryContextMatches
    {
        get
        {
            if (!base.IsIncremental) return false;

            var node = this.CurrentNode;
            return node is not null && this.MatchFactoryContext(node.Green, this.syntaxFactoryContext);
        }
    }

    private protected partial bool MatchFactoryContext(GreenNode green, SyntaxFactoryContext context);

    private protected bool IsTerminal()
    {
        if (this.CurrentTokenKind == SyntaxKind.EndOfFileToken) return true;

        for (int i = 1; i < LanguageParser.LastTerminatorState; i <<= 1)
        {
            var state = (TerminatorState)i;
            if (IsTerminalCore(state)) return true;
        }

        return false;
    }

    private protected virtual partial bool IsTerminalCore(TerminatorState state);
}
