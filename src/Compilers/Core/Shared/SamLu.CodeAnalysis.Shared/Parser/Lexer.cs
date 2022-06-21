using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisParseOptions = SamLu.CodeAnalysis.Lua.LuaParseOptions;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisParseOptions = SamLu.CodeAnalysis.MoonScript.MoonScriptParseOptions;
#endif

internal partial class Lexer : AbstractLexer
{
    private const int IdentifierBufferInitialCapacity = 32;
    private const int TriviaListInitialCapacity = 8;

    private readonly ThisParseOptions _options;

    private LexerMode _mode;
    private readonly StringBuilder _builder;
    private char[] _identifierBuffer;
    private int _identifierLength;
    private readonly LexerCache _cache;
    private int _badTokenCount; // 产生的坏标志的累计数量。

    public ThisParseOptions Options => this._options;

    public Lexer(SourceText text, ThisParseOptions options) : base(text)
    {
        this._options = options;
        this._builder = new StringBuilder();
        this._identifierBuffer = new char[Lexer.IdentifierBufferInitialCapacity];
        this._cache = new();
        this._createQuickTokenFunction = this.CreateQuickToken;
    }

    public override void Dispose()
    {
        this._cache.Free();

        base.Dispose();
    }

    public void Reset(int position) => this.TextWindow.Reset(position);

    private static LexerMode ModeOf(LexerMode mode) => mode & LexerMode.MaskLexMode;

    private bool ModeIs(LexerMode mode) => Lexer.ModeOf(this._mode) == mode;

    public SyntaxToken Lex(ref LexerMode mode)
    {
        var result = this.Lex(mode);
        mode = this._mode;
        return result;
    }

    public partial SyntaxToken Lex(LexerMode mode);

    private SyntaxListBuilder _leadingTriviaCache = new(10);
    private SyntaxListBuilder _trailingTriviaCache = new(10);

    private static int GetFullWidth(SyntaxListBuilder? builder)
    {
        if (builder is null) return 0;

        int width = 0;
        for (int i = 0; i < builder.Count; i++)
        {
            var node = builder[i];
            Debug.Assert(node is not null);
            width += node.FullWidth;
        }

        return width;
    }

    private SyntaxToken LexSyntaxToken()
    {
        this._leadingTriviaCache.Clear();
        this.LexSyntaxTrivia(afterFirstToken: this.TextWindow.Position > 0, isTrailing: false, triviaList: ref this._leadingTriviaCache);
        var leading = this._leadingTriviaCache;

        TokenInfo tokenInfo = default;

        this.Start();
        this.ScanSyntaxToken(ref tokenInfo);
        var errors = this.GetErrors(Lexer.GetFullWidth(leading));

        this._trailingTriviaCache.Clear();
        this.LexSyntaxTrivia(afterFirstToken: true, isTrailing: true, triviaList: ref this._trailingTriviaCache);
        var trailing = this._trailingTriviaCache;

        return this.Create(in tokenInfo, leading, trailing, errors);
    }

    internal SyntaxTriviaList LexSyntaxLeadingTrivia()
    {
        this._leadingTriviaCache.Clear();
        this.LexSyntaxTrivia(afterFirstToken: this.TextWindow.Position > 0, isTrailing: false, triviaList: ref this._leadingTriviaCache);
        return new(default,
            this._leadingTriviaCache.ToListNode(), position: 0, index: 0);
    }

    internal SyntaxTriviaList LexSyntaxTrailingTrivia()
    {
        this._trailingTriviaCache.Clear();
        this.LexSyntaxTrivia(afterFirstToken: true, isTrailing: true, triviaList: ref this._trailingTriviaCache);
        return new(default,
            this._trailingTriviaCache.ToListNode(), position: 0, index: 0);
    }

    /// <summary>
    /// 创建一个语法标志。
    /// </summary>
    /// <param name="info">语法标志的相关信息。</param>
    /// <param name="leading">起始的语法列表构造器。</param>
    /// <param name="trailing">结尾的语法列表构造器。</param>
    /// <param name="errors">语法诊断消息数组。</param>
    /// <returns>新的语法标志。</returns>
    private partial SyntaxToken Create(
        in TokenInfo info,
        SyntaxListBuilder? leading,
        SyntaxListBuilder? trailing,
        SyntaxDiagnosticInfo[]? errors);

    private partial void ScanSyntaxToken(ref TokenInfo info);

    private partial void LexSyntaxTrivia(bool afterFirstToken, bool isTrailing, ref SyntaxListBuilder triviaList);

#error 未完成
}
