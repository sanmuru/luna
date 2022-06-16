using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisParseOptions = SamLu.CodeAnalysis.Lua.LuaParseOptions;
using ThisSyntaxNode = SamLu.CodeAnalysis.Lua.LuaSyntaxNode;
using ThisInternalSyntaxNode = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisParseOptions = SamLu.CodeAnalysis.MoonScript.MoonScriptParseOptions;
using ThisSyntaxNode = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxNode;
using ThisInternalSyntaxNode = SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax.MoonScriptSyntaxNode;
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

    protected ResetPoint GetResetPoint()
    {
        var pos = this.CurrentTokenPosition;
        if (this._resetCount == 0)
            this._resetCount = pos;

        this._resetCount++;
        return new(this._resetCount, this._mode, pos, this._prevTokenTrailingTrivia);
    }

    protected void Reset(ref ResetPoint point)
    {
        var offset = point.Position - this._firstToken;
        Debug.Assert(offset >= 0);

        if (offset >= this._tokenCount)
        {
            this.PeekToken(offset - this._tokenOffset);

            offset = point.Position - this._firstToken;
        }

        this._mode = point.Mode;
        Debug.Assert(offset >= 0 && offset < this._tokenCount);
        this._tokenOffset = offset;
        this.CurrentToken = null;
        this.CurrentNode = default;
        this._prevTokenTrailingTrivia = point.PrevTokenTrailingTrivia;
        if (this.IsBlending())
        {
            for (int i = this._tokenOffset; i < this._tokenCount; i++)
            {
                if (this._blendedTokens[i].Token is null)
                {
                    this._tokenCount = i;
                    if (this._tokenCount == this)
                        this.FetchCurrentToken();
                    break;
                }
            }
        }
    }

    protected void Release(ref ResetPoint point)
    {
        Debug.Assert(this._resetCount == point.ResetCount);
        this._resetCount--;
        if (this._resetCount == 0)
            this._resetCount = -1;
    }

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

    protected SyntaxToken EatTokenAsKind(SyntaxKind expected)
    {
        Debug.Assert(SyntaxFactory.IsAnyToken(expected));

        var token = this.CurrentToken;
        if (token.Kind == expected)
        {
            this.MoveToNextToken();
            return token;
        }

        var replacement = this.CreateMissingToken(expected, this.CurrentToken.Kind, reportError: true);
        return this.AddTrailingSkippedSyntax(replacement, this.EatToken());
    }

    private SyntaxToken CreateMissingToken(SyntaxKind expected, SyntaxKind actual, bool reportError)
    {
        var token = SyntaxFactory.MissingToken(expected);
        if (reportError)
        {
            token = this.WithAdditionalDiagnostics(token, this.GetExpectedTokenError(expected, actual1))
        }

        return token;
    }

    private SyntaxToken CreateMissingToken(SyntaxKind expected, ErrorCode code, bool reportError)
    {
        var token = SyntaxFactory.MissingToken(expected);
        if (reportError)
            token = this.AddError(token, code);
    }

    protected SyntaxToken EatTokenWithPrejudice
        (SyntaxKind kind)
    {
        var token = this.CurrentToken;
        Debug.Assert(SyntaxFactory.IsAnyToken(kind));
        if (token.Kind != kind)
            token = this.WithAdditionalDiagnostics(token, this.GetExpectedTokenError(kind, token.Kind));

        this.MoveToNextToken();
        return token;
    }

    protected SyntaxToken EatTokenWithPrejuice(ErrorCode code, params object[] args)
    {
        var token = this.EatToken();
        token = this.WithAdditionalDiagnostics(token, this.MakeError(token.GetLeadingTriviaWidth(), token.Width(), token.Width, code, args));
        return token;
    }

    protected SyntaxToken EatContextualToken(SyntaxKind kind, ErrorCode code, bool reportError = true)
    {
        Debug.Assert(SyntaxFacts.IsAnyTokne(kind));

        if (this.CurrentToken.ContextualKind != kind)
            return this.CreateMissingToken(kind, code, reportError);
        else
            return SyntaxParser.ConvertToKeyword(this.EatToken());
    }

    protected SyntaxToken EatContextualToken(SyntaxKind kind, bool reportError = true)
    {
        Debug.Assert(SyntaxFacts.IsAnyToken(kind));

        var contextualKind = this.CurrentToken.ContextualKind;
        if (contextualKind != kind)
            return this.CreateMissingToken(kind, contextualKind, reportError);
        else
            return SyntaxParser.ConvertToKeyword(this.EatToken());
    }

    protected virtual partial SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual, int offset, int width);

    protected virtual SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual)
    {
        this.GetDiagnosticSpanForMissingToken(out int offset, out int width);

        return this.GetExpectedTokenError(expected, actual, offset, width);
    }

    protected void GetDiagnosticSpanForMissingToken(out int offset, out int width)
    {
        var trivia = this._prevTokenTrailingTrivia;
        if (trivia is not null)
        {
            var triviaList = new SyntaxList<ThisInternalSyntaxNode>(trivia);
            bool prevTokenHasEndOfLineTrivia = triviaList.Any((int)SyntaxKind.EndOfLineTrivia);
            if (prevTokenHasEndOfLineTrivia)
            {
                offset = -trivia.FullWidth;
                width = 0;
                return;
            }
        }

        SyntaxToken token = this.CurrentToken;
        offset = token.GetLeadingTriviaWidth();
        width = token.Width;
    }

    protected virtual TNode WithAdditionalDiagnostics<TNode>(TNode node, params DiagnosticInfo[] diagnostics)
        where TNode : GreenNode
    {
        var existingDiagnostics = node.GetDiagnostics();
        int existingLength = existingDiagnostics.Length;
        if (existingLength == 0)
            return node.WithDiagnosticsGreen(diagnostics);
        else
        {
            var result = new DiagnosticInfo[existingDiagnostics.Length + diagnostics.Length];
            existingDiagnostics.CopyTo(result, 0);
            diagnostics.CopyTo(result, existingLength);
            return node.WithDiagnosticsGreen(result);
        }
    }

    protected TNode AddError<TNode>(TNode node, ErrorCode code) where TNode : GreenNode =>
        this.AddError(node, node, Array.Empty<object>());

    protected TNode AddError<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : GreenNode
    {
        if (!node.IsMissing)
            return this.WithAdditionalDiagnostics(node, this.MakeError(node, code, args));

        int offset, width;

        var token = node as SyntaxToken;
        if (token is not null && token.ContainsSkippedText)
        {
            offset = token.GetLeadingTriviaWidth();
            Debug.Assert(offset == 0);

            width = 0;
            bool seenSkipped = false;
            foreach (var trivia in token.TrailingTrivia)
            {
                if (trivia.Kind == SyntaxKind.SkippedTokensTrivia)
                {
                    seenSkipped = true;
                    width += trivia.Width;
                }
                else if (seenSkipped)
                    break;
                else
                    offset += trivia.Width;
            }
        }
        else
            this.GetDiagnosticSpanForMissingToken(out offset, out width);

        return this.WithAdditionalDiagnostics(node, this.MakeError(offset, width, code, args));
    }

    protected TNode AddError<TNode>(TNode node, ThisInternalSyntaxNode location, ErrorCode code, params object[] args) where TNode : ThisInternalSyntaxNode
    {
        this.FindOffset(node, location, out int offset);
        return this.WithAdditionalDiagnostics(node, this.MakeError(offset, location.Width, code, args));
    }

    protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code) where TNode : ThisInternalSyntaxNode
    {
        var firstToken = node.GetFirstToken();
        return this.WithAdditionalDiagnostics(node, this.MakeError(firstToken.GetLeadingTrivia(), firstToken.Width, code));
    }

    protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : ThisInternalSyntaxNode
    {
        var firstToken = node.GetFirstToken();
        return this.WithAdditionalDiagnostics(node, this.MakeError(firstToken.GetLeadingTrivia(), firstToken.Width, code, args));
    }

    protected TNode AddErrorToLastToken<TNode>(TNode node, ErrorCode code) where TNode : ThisInternalSyntaxNode
    {
        this.GetOffsetAndWidthForLastToken(node, out int offset, out int width);
        return this.WithAdditionalDiagnostics(node, this.MakeError(offset, width, code));
    }

    protected TNode AddErrorToLastToken<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : ThisInternalSyntaxNode
    {
        SyntaxParser.GetOffsetAndWidthForLastToken(node, out int offset, out int width);
        return this.WithAdditionalDiagnostics(node, this.MakeError(offset, width, code, args));
    }

    private static void GetOffsetAndWidthForLastToken<TNode>(TNode node, out int offset, out int width) where TNode : ThisInternalSyntaxNode
    {
        var lastToken = node.GetLastNonmissingToken();
        offset = node.FullWidth;
        width = 0;
        if (lastToken is not null)
        {
            offset -= lastToken.FullWidth;
            offset += lastToken.GetLeadingTriviaWidth();
            width += lastToken.Width;
        }
    }

#error 未完成

    /// <summary>
    /// 将非关键字标志转换为包含相同信息的关键字标志。
    /// </summary>
    /// <param name="token">要转化的非关键字标志。</param>
    /// <returns>一个关键字标志，包含非关键字标志<paramref name="token"/>的所有信息。</returns>
    protected static SyntaxToken ConvertToKeyword(SyntaxToken token)
    {
        if (token.Kind != token.ContextualKind)
        {
            // 分两种情况：是否为缺失标志。
            var kw = token.IsMissing
                ? SyntaxFactory.MissingToken(
                    token.LeadingTrivia.Node,
                    token.ContextualKind,
                    token.TrailingTrivia.Node
                )
                : SyntaxFactory.Token(
                    token.LeadingTrivia.Node,
                    token.ContextualKind,
                    token.TrailingTrivia.Node
                );
            var d = token.GetDiagnostics();
            // 如果有诊断信息，则附加到标志上。
            if (d is not null && d.Length > 0)
                kw = kw.WithDiagnosticsGreen(d);

            return kw;
        }

        return token;
    }

    /// <summary>
    /// 将非标识符标志转换为包含相同信息的标识符标志。
    /// </summary>
    /// <param name="token">要转化的非标识符标志。</param>
    /// <returns>一个标识符标志，包含非标识符标志<paramref name="token"/>的所有信息。</returns>
    protected static SyntaxToken ConvertToIdentifier(SyntaxToken token)
    {
        Debug.Assert(!token.IsMissing);
        return SyntaxToken.Identifier(
            token.Kind,
            token.LeadingTrivia.Node,
            token.Text,
            token.ValueText,
            token.TrailingTrivia.Node
        );
    }

    /// <summary>
    /// 检查特性是否可用，不可用时为语法节点附加错误信息。
    /// </summary>
    /// <typeparam name="TNode">语法节点的类型。</typeparam>
    /// <param name="node">作为载体的语法节点。</param>
    /// <param name="feature">要检查的特性。</param>
    /// <param name="forceWarning">是否强制视为警告。</param>
    /// <returns>检查处理后的语法节点。</returns>
    protected partial TNode CheckFeatureAvailability<TNode>(TNode node, MessageID feature, bool forceWarning = false)
        where TNode : GreenNode;

    /// <inheritdoc cref="ThisParseOptions.IsFeatureEnabled(MessageID)"/>
    protected bool IsFeatureEnabled(MessageID feature) => this.Options.IsFeatureEnabled(feature);

    /// <summary>获取当前解析的标志的位置。</summary>
    private int CurrentTokenPosition => this._firstToken + this._tokenOffset;

    /// <summary>
    /// 当解析进入循环流程中时，为防止因意外的错误导致解析器无法向后分析而出现死循环，此方法应作为保险措施而非实现功能的方式。
    /// </summary>
    /// <param name="lastTokenPosition">上一次更新的标志位置。</param>
    /// <param name="assertIfFalse">当解析器无法向后分析时是否使用断言中断。</param>
    /// <returns>若为<see langword="true"/>时，表示解析器正常向后分析；若为<see langword="false"/>时，表示解析器无法向后分析。</returns>
    protected bool IsMakingProgress(ref int lastTokenPosition, bool assertIfFalse = true)
    {
        var pos = this.CurrentTokenPosition;
        if (pos > lastTokenPosition)
        {
            lastTokenPosition = pos;
            return true;
        }

        Debug.Assert(!assertIfFalse);
        return false;
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
