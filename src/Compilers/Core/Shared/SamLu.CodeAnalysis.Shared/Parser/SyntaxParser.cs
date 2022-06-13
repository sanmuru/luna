using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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

/// <summary>
/// 表示语法解析器，提供语法解析器的必要约束和部分实现，此类必须被继承。
/// </summary>
internal abstract partial class SyntaxParser : IDisposable
{
    /// <summary>语法解析器的词法器。</summary>
    protected readonly Lexer lexer;
    /// <summary>是否进行增量语法分析。</summary>
    private readonly bool _isIncremental;
    /// <summary>是否允许重置词法器模式。</summary>
    private readonly bool _allowModeReset;
    /// <summary>语法解析器的各项操作的取消标志。</summary>
    protected readonly CancellationToken cancellationToken;

    /// <summary>词法器模式。</summary>
    private LexerMode _mode;
    private Blender _firstBlender;
    private BlendedNode _currentNode;
    private BlendedNode[]? _blendedTokens;
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

    /// <summary>
    /// 获取一个值，指示语法解析器是否进行增量语法分析。
    /// </summary>
    protected bool IsIncremental => this._isIncremental;

    /// <summary>
    /// 获取语法解析器的设置。
    /// </summary>
    public ThisParseOptions Options => this.lexer.Options;

    /// <summary>
    /// 获取一个值，指示解析的是脚本类代码文本。
    /// </summary>
    public bool IsScript => this.Options.Kind == SourceCodeKind.Script;

    /// <summary>
    /// 获取或设置词法器模式。
    /// </summary>
    protected LexerMode Mode
    {
        get => this._mode;
        set
        {
            if (this._mode != value)
            {
                Debug.Assert(this._allowModeReset);

                // 设置新的词法器模式并重置字段。
                this._mode = value;
                this._currentToken = null;
                this._currentNode = default;
                this._tokenCount = this._tokenOffset;
            }
        }
    }

    /// <summary>
    /// 创建<see cref="SyntaxParser"/>的新实例。
    /// </summary>
    /// <param name="lexer">语法解析器的词法器。</param>
    /// <param name="mode">内部词法器的模式。</param>
    /// <param name="oldTree">旧语法树的根节点。传入不为<see langword="null"/>的值时，将开启增量处理操作。</param>
    /// <param name="changes">被修改的文本范围。</param>
    /// <param name="allowModeReset">是否允许重设词法器的模式。</param>
    /// <param name="preLexIfNotIncremental">当不开启增量处理操作时，是否进行词法预分析。</param>
    /// <param name="cancellationToken">语法解析器的操作的取消标志。</param>
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
        this._isIncremental = oldTree is not null; // 是否开启增量处理操作。

        if (this._isIncremental || allowModeReset) // 协调新旧节点。
        {
            this._firstBlender = new(lexer, oldTree, changes);
            this._blendedTokens = SyntaxParser.s_blendedNodesPool.Allocate();
        }
        else // 均为新节点。
        {
            this._firstBlender = default;
            this._lexedTokens = new ArrayElement<SyntaxToken>[32];
        }

        // 进行词法器预分析，此操作不应被取消。
        // 因为在构造函数中取消操作将会使处置操作变得复杂难懂，所以应排除可取消的情况。
        if (preLexIfNotIncremental && !this._isIncremental && !cancellationToken.CanBeCanceled)
            this.PreLex();
    }

    [MemberNotNullWhen(true, nameof(SyntaxParser._blendedTokens))]
    [MemberNotNullWhen(false, nameof(SyntaxParser._lexedTokens))]
    protected bool IsBlending() => this._isIncremental || this._allowModeReset;

    protected void ReInitialize()
    {
        this._firstToken = 0;
        this._tokenOffset = 0;
        this._tokenCount = 0;
        this._resetCount = 0;
        this._resetStart = 0;
        this._currentToken = null;
        this._prevTokenTrailingTrivia = null;
        if (this._isIncremental || this._allowModeReset)
            this._firstBlender = new(this.lexer, null, null);
    }

    /// <summary>
    /// 词法器预分析。
    /// </summary>
    [MemberNotNull(nameof(SyntaxParser._lexedTokens))]
    private void PreLex()
    {
        // 不应在这个方法内部处理取消标志。
        var size = Math.Min(4096, Math.Max(32, this.lexer.TextWindow.Text.Length / 2));
        this._lexedTokens ??= new ArrayElement<SyntaxToken>[size];
        var lexer = this.lexer;
        var mode = this._mode;

        for (int i = 0; i < size; i++)
        {
            var token = lexer.Lex(mode); // 词法器分析一个标志。
            this.AddLexedToken(token);

            // 遇到文件结尾。
            if (token.Kind == SyntaxKind.EndOfFileToken) break;
        }
    }

    protected ResetPoint GetResetPoint();

    protected void Reset(ref ResetPoint point);

    protected void Release(ref ResetPoint point);

    /// <summary>
    /// 获取当前的已协调节点。
    /// </summary>
    [MemberNotNull(nameof(SyntaxParser._blendedTokens))]
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

    /// <summary>
    /// 获取当前的已协调节点的语法部分类型。
    /// </summary>
    protected SyntaxKind CurrentNodeKind => this.CurrentNode?.Kind() ?? SyntaxKind.None;

    /// <summary>
    /// 读取当前的节点到<see cref="SyntaxParser._currentNode"/>。
    /// </summary>
    [MemberNotNull(nameof(SyntaxParser._blendedTokens))]
    private void ReadCurrentNode()
    {
        Debug.Assert(this._blendedTokens is not null);

        if (this._tokenOffset == 0)
            this._currentNode = this._firstBlender.ReadNode(this._mode);
        else
            this._currentNode = this._blendedTokens[this._tokenOffset - 1].Blender.ReadNode(this._mode);
    }

    [MemberNotNull(nameof(SyntaxParser._blendedTokens))]
    protected GreenNode? EatNode()
    {
        Debug.Assert(this._blendedTokens is not null);

        var result = this.CurrentNode?.Green;

        if (this._tokenOffset >= this._blendedTokens.Length)
            this.AddTokenSlot();

        this._blendedTokens[this._tokenOffset++] = this._currentNode;
        this._tokenCount = this._tokenOffset;

        this._currentNode = default;
        this._currentToken = null;

        return result;
    }

    /// <summary>
    /// 获取当前的已词法分析的标志。
    /// </summary>
    protected SyntaxToken CurrentToken => this._currentToken ??= this.FetchCurrentToken();

    /// <summary>
    /// 取得当前的已词法分析的标志。
    /// </summary>
    private SyntaxToken FetchCurrentToken()
    {
        if (this._tokenOffset >= this._tokenCount)
            this.AddNewToken();

        if (this.IsBlending())
            return this._blendedTokens[this._tokenOffset].Token;
        else
            return this._lexedTokens[this._tokenOffset];
    }

    private void AddNewToken()
    {
        if (this._blendedTokens is null)
            this.AddLexedToken(this.lexer.Lex(this._mode));
        else
        {
            if (this._tokenCount > 0)
                this.AddToken(this._blendedTokens[this._tokenCount - 1].Blender.ReadToken(this._mode));
            else
            {
                if (this._currentNode.Token is not null)
                    this.AddToken(this._currentNode);
                else
                    this.AddToken(this._firstBlender.ReadToken(this._mode));
            }
        }
    }

    [MemberNotNull(nameof(SyntaxParser._blendedTokens))]
    private void AddToken(in BlendedNode tokenResult)
    {
        Debug.Assert(tokenResult.Token is not null);
        Debug.Assert(this._blendedTokens is not null);

        if (this._tokenCount >= this._blendedTokens.Length)
            this.AddTokenSlot();

        this._blendedTokens[this._tokenCount] = tokenResult;
        this._tokenCount++;
    }

    [MemberNotNull(nameof(SyntaxParser._lexedTokens))]
    private void AddLexedToken(SyntaxToken token)
    {
        Debug.Assert(this._lexedTokens is not null);

        if (this._tokenCount >= this._lexedTokens.Length)
            this.AddLexedTokenSlot();

        this._lexedTokens[this._tokenCount].Value = token;
        this._tokenCount++;
    }

    [MemberNotNull(nameof(SyntaxParser._blendedTokens))]
    private void AddTokenSlot()
    {
        Debug.Assert(this._blendedTokens is not null);

        if (this._tokenOffset > (this._blendedTokens.Length >> 1) && (this._resetStart == -1 || this._resetStart > this._firstToken))
        {
            int shiftOffset = (this._resetStart == -1) ? this._tokenOffset : this._resetStart - this._firstToken;
            int shiftCount = this._tokenCount - shiftOffset;
            Debug.Assert(shiftOffset > 0);
            this._firstBlender = this._blendedTokens[shiftOffset - 1].Blender;
            if (shiftCount > 0)
                Array.Copy(this._blendedTokens, shiftOffset, this._blendedTokens, 0, shiftCount);

            this._firstToken += shiftOffset;
            this._tokenCount -= shiftOffset;
            this._tokenOffset -= shiftOffset;
        }
        else
        {
            var old = this._blendedTokens;
            Array.Resize(ref this._blendedTokens, this._blendedTokens.Length * 2);
            SyntaxParser.s_blendedNodesPool.ForgetTrackedObject(old, replacement: this._blendedTokens);
        }
    }

    [MemberNotNull(nameof(SyntaxParser._lexedTokens))]
    private void AddLexedTokenSlot()
    {
        Debug.Assert(this._lexedTokens is not null);

        if (this._tokenOffset > (this._lexedTokens.Length >> 1) && (this._resetStart == -1 || this._resetStart > this._firstToken))
        {
            int shiftOffset = (this._resetStart == -1) ? this._tokenOffset : this._resetStart - this._firstToken;
            int shiftCount = this._tokenCount - shiftOffset;
            Debug.Assert(shiftOffset > 0);
            if (shiftCount > 0)
                Array.Copy(this._lexedTokens, shiftOffset, this._lexedTokens, 0, shiftCount);

            this._firstToken += shiftOffset;
            this._tokenCount -= shiftOffset;
            this._tokenCount -= shiftOffset;
        }
        else
        {
            var old = new ArrayElement<SyntaxToken>[this._lexedTokens.Length * 2];
            Array.Copy(this._lexedTokens, old, this._lexedTokens.Length);
            this._lexedTokens = old;
        }
    }

    protected SyntaxToken PeekToken(int n)
    {
        Debug.Assert(n >= 0);

        while (this._tokenOffset + n >= this._tokenCount)
            this.AddNewToken();

        if (this.IsBlending())
            return this._blendedTokens[this._tokenOffset + n].Token;
        else
            return this._lexedTokens[this._tokenOffset + n];
    }

    protected SyntaxToken EatToken()
    {
        var token = this.CurrentToken;
        this.MoveToNextToken();
        return token;
    }

    protected SyntaxToken EatToken(SyntaxKind kind)
    {
        Debug.Assert(SyntaxFacts.IsAnyToken(kind));

        var token = this.CurrentToken;
        if (token.Kind == kind)
        {
            this.MoveToNextToken();
            return token;
        }

        return this.CreateMissingToken(kind, this.CurrentToken.Kind, reportError: true);
    }

    protected SyntaxToken? TryEatToken(SyntaxKind kind) => this.CurrentToken.Kind == kind ? this.EatToken() : null;

    private void MoveToNextToken()
    {
        this._prevTokenTrailingTrivia = this.CurrentToken.GetTrailingTrivia();

        this._currentToken = null;

        if (this._blendedTokens is not null)
            this._currentNode = default;

        this._tokenOffset++;
    }

    protected void ForceEndOfFile()
    {
        this._currentToken = SyntaxFactory.Token(SyntaxKind.EndOfFileToken);
    }

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
