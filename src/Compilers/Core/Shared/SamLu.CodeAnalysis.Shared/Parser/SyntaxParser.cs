using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisParseOptions = SamLu.CodeAnalysis.Lua.LuaParseOptions;
using ThisSyntaxNode = SamLu.CodeAnalysis.Lua.LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisParseOptions = SamLu.CodeAnalysis.MoonScript.MoonScriptParseOptions;
using ThisSyntaxNode = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxNode;
#endif

using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

internal abstract partial class SyntaxParser : IDisposable
{
    protected readonly Lexer lexer;
    private readonly bool _isIncremental;
    private readonly bool _allowModeReset;
    protected readonly CancellationToken cancellationToken;

    private LexerMode _mode;
    private Blender _firstBlender;
    private BlendedNode _currentNode;
    private SyntaxToken? _currentToken;
    private ArrayElement<SyntaxToken>[]? _lexedTokens;
    private GreenNode? _prevTokenTrailingTrivia;
    /// <summary>
    /// <see cref="_lexedTokens"/>或<see cref="_blendedTokens"/>的第一项的位置。
    /// </summary>
    private int _firstToken;
    /// <summary>
    /// 当前处理的标志在<see cref="_lexedTokens"/>或<see cref="_blendedTokens"/>的索引位置。
    /// </summary>
    private int _tokenOffset;
    private int _tokenCount;
    private int _resetCount;
    private int _resetStart;

    private static readonly ObjectPool<BlendedNode[]> s_blendedNodesPool = new(() => new BlendedNode[32], 2);

    private BlendedNode[]? _blendedTokens;

    protected bool IsIncremental => this._isIncremental;

    public ThisParseOptions Options => this.lexer.Options;

    public bool IsScript => this.Options.Kind == SourceCodeKind.Script;

    protected LexerMode Mode
    {
        get => this._mode;
        set
        {
            if (this._mode != value)
            {
                Debug.Assert(this._allowModeReset);

                this._mode = value;
                this._currentToken = null;
                this._currentNode = default;
                this._tokenCount = this._tokenOffset;
            }
        }
    }

    protected SyntaxParser(
        Lexer lexer,
        LexerMode mode,
        ThisSyntaxNode? oldTree,
        IEnumerable<TextChangeRange>? changes,
        bool allowModeReset,
        bool preLexIfNotIncremental = false,
        CancellationToken cancellationToken = default
    )
    {
        this.lexer = lexer;
        this._mode = mode;
        this._allowModeReset = allowModeReset;
        this.cancellationToken = cancellationToken;
        this._currentNode = default;
        this._isIncremental = oldTree is not null;

        if (this.IsIncremental || allowModeReset)
        {
            this._firstBlender = new(lexer, oldTree, changes);
            this._blendedTokens = SyntaxParser.s_blendedNodesPool.Allocate();
        }
        else
        {
            this._firstBlender = default;
            this._lexedTokens = new ArrayElement<SyntaxToken>[32];
        }

        if (preLexIfNotIncremental && !this.IsIncremental && !cancellationToken.CanBeCanceled)
            this.PreLex();
    }

    protected void ReInitialize()
    {
        this._firstToken = 0;
        this._tokenOffset = 0;
        this._tokenCount = 0;
        this._resetCount = 0;
        this._resetStart = 0;
        this._currentToken = null;
        this._prevTokenTrailingTrivia = null;
        if (this.IsIncremental || this._allowModeReset)
            this._firstBlender = new(this.lexer, null, null);
    }

    private void PreLex()
    {
        var size = Math.Min(4096, Math.Max(32, this.lexer.TextWindow.Text.Length / 2));
        this._lexedTokens = new ArrayElement<SyntaxToken>[size];
        var lexer = this.lexer;
        var mode = this._mode;

        for (int i = 0; i < size; i++)
        {
            var token = lexer.Lex(mode);
            this.AddLexedToken(token);
            if (token.Kind == SyntaxKind.EndOfFileToken) break;
        }
    }

    protected ResetPoint GetResetPoint();

    protected void Reset(ref ResetPoint point);

    protected void Release(ref ResetPoint point);

    protected ThisSyntaxNode? CurrentNode
    {
        get
        {
            Debug.Assert(this._blendedTokens is not null);

            var node = this._currentNode.Node;
            if (node is not null) return node;

            this.ReadCurrentNode();
            return this._currentNode.Node;
        }
    }

    protected SyntaxKind CurrentNodeKind => this.CurrentNode?.Kind() ?? SyntaxKind.None;

    private void ReadCurrentNode();

    #region IDisposable
    public void Dispose()
    {
        var blendedTokens = _blendedTokens;
        if (blendedTokens is not null)
        {
            this._blendedTokens = null!;
            if (blendedTokens.Length < 4096)
            {
                Array.Clear(blendedTokens, 0, blendedTokens.Length);
                SyntaxParser.s_blendedNodesPool.Free(blendedTokens);
            }
            else
            {
                SyntaxParser.s_blendedNodesPool.ForgetTrackedObject(blendedTokens);
            }
        }
    }
    #endregion
}
