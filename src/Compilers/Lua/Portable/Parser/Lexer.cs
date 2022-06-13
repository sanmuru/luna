using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

[Flags]
internal enum LexerMode
{
    None = 0,

    Syntax = 0x0001,
    DebuggerSyntax = 0x0002,

    MaskLexMode = 0xFFFF
}

/// <summary>
/// 针对Lua语言特定的词法解析器。
/// </summary>
internal partial class Lexer : AbstractLexer
{
    private const int IdentifierBufferInitialCapacity = 32;
    private const int TriviaListInitialCapacity = 8;

    private readonly LuaParseOptions _options;

    private LexerMode _mode;
    private readonly StringBuilder _builder;
    private char[] _identifierBuffer;
    private int _identifierLength;
    private readonly LexerCache _cache;
    private int _badTokenCount; // 产生的坏标识符的累计数量。

    public LuaParseOptions Options => this._options;

    /// <summary>
    /// 存放语法标识的必要信息。
    /// </summary>
    internal struct TokenInfo
    {
        /// <summary>
        /// 直接语法类别。
        /// </summary>
        internal SyntaxKind Kind;
        /// <summary>
        /// 上下文语法类别。
        /// </summary>
        internal SyntaxKind ContextualKind;
        /// <summary>
        /// 语法标识的文本表示。
        /// </summary>
        internal string? Text;
        /// <summary>
        /// 语法标识的值类别。
        /// </summary>
        internal SpecialType ValueKind;
        internal bool HasIdentifierEscapeSequence;
        /// <summary>
        /// 语法标识的字符串类型值。
        /// </summary>
        internal string? StringValue;
        /// <summary>
        /// 语法标识的32位整数类型值。
        /// </summary>
        internal int IntValue;
        /// <summary>
        /// 语法标识的64位整数类型值。
        /// </summary>
        internal long LongValue;
        /// <summary>
        /// 语法标识的32位单精度浮点数类型值。
        /// </summary>
        internal float FloatValue;
        /// <summary>
        /// 语法标识的64位双精度浮点数类型值。
        /// </summary>
        internal double DoubleValue;
        /// <summary>
        /// 语法标识是否为逐字。
        /// </summary>
        internal bool IsVerbatim;
    }

    public Lexer(SourceText text, LuaParseOptions options) : base(text)
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

    public SyntaxToken Lex(LexerMode mode)
    {
        this._mode = mode;

        switch (this._mode)
        {
            case LexerMode.Syntax:
            case LexerMode.DebuggerSyntax:
                return this.QuickScanSyntaxToken() ?? this.LexSyntaxToken();
        }

        switch (Lexer.ModeOf(this._mode))
        {
            default:
                throw ExceptionUtilities.UnexpectedValue(ModeOf(_mode));
        }
    }

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

    private SyntaxToken LexSyntaxToken();

    internal SyntaxTriviaList LexSyntaxLeadingTrivia();

    internal SyntaxTriviaList LexSyntaxTrailingTrivia();

    /// <summary>
    /// 创建一个语法标识。
    /// </summary>
    /// <param name="info">语法标识的相关信息。</param>
    /// <param name="leading">起始的语法列表构造器。</param>
    /// <param name="trailing">结尾的语法列表构造器。</param>
    /// <param name="errors">语法消息数组。</param>
    /// <returns>新的语法标识。</returns>
    private SyntaxToken Create(
        ref TokenInfo info,
        SyntaxListBuilder? leading,
        SyntaxListBuilder? trailing,
        SyntaxDiagnosticInfo[]? errors)
    {
        Debug.Assert(info.Kind != SyntaxKind.IdentifierToken || info.StringValue is not null);

        GreenNode? leadingNode = leading?.ToListNode();
        GreenNode? trailingNode = trailing?.ToListNode();

        SyntaxToken token = info.Kind switch
        {
            // 标识符标识
            SyntaxKind.IdentifierToken => SyntaxFactory.Identifier(info.ContextualKind, leadingNode, info.Text!, info.StringValue!, trailingNode),

            // 数字字面量标识
            SyntaxKind.NumericLiteralToken =>
                info.ValueKind switch
                {
                    // 32位整数
                    SpecialType.System_Int32 => SyntaxFactory.Literal(leadingNode, info.Text!, info.IntValue, trailingNode),
                    // 64为整数
                    SpecialType.System_Int64 => SyntaxFactory.Literal(leadingNode, info.Text!, info.LongValue, trailingNode),
                    // 32位单精度浮点数
                    SpecialType.System_Single => SyntaxFactory.Literal(leadingNode, info.Text!, info.FloatValue, trailingNode),
                    // 64为双精度浮点数
                    SpecialType.System_Double => SyntaxFactory.Literal(leadingNode, info.Text!, info.DoubleValue, trailingNode),
                    _ => throw ExceptionUtilities.UnexpectedValue(info.ValueKind),
                },

            // 字符串字面量标识
            SyntaxKind.StringLiteralToken or
            // 单行原始字符串字面量标识
            SyntaxKind.SingleLineRawStringLiteralToken or
            // 多行原始字符串字面量标识
            SyntaxKind.MultiLineRawStringLiteralToken => SyntaxFactory.Literal(leadingNode, info.Text!, info.Kind, info.StringValue!, trailingNode),

            SyntaxKind.EndOfFileToken => SyntaxFactory.Token(leadingNode, SyntaxKind.EndOfFileToken, trailingNode),

            SyntaxKind.None => SyntaxFactory.BadToken(leadingNode, info.Text!, trailingNode),

            _ => SyntaxFactory.Token(leadingNode, info.Kind, trailingNode)
        };

        // 为标识添加诊断。
        if (errors is not null && this._options.DocumentationMode >= DocumentationMode.Diagnose)
            token = token.WithDiagnosticsGreen(errors);

        return token;
    }

    private void ScanSyntaxToken(ref TokenInfo info)
    {
        // 初始化以准备新的标识扫描。
        info.Kind = SyntaxKind.None;
        info.ContextualKind = SyntaxKind.None;
        info.Text = null;
        char c;
        char surrogateCharacter = SlidingTextWindow.InvalidCharacter;
        bool isEscaped = false;
        int startingPosition = this.TextWindow.Position;

        // 开始扫描标识。
        c = this.TextWindow.PeekChar();
        switch (c)
        {
            case '+':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.PlusToken;
                break;

            case '-':
                this.TextWindow.AdvanceChar();
                if (this.TextWindow.PeekChar() == '-')
                {
                    this.TextWindow.AdvanceChar();
                    this.ScanComment(ref info);
                }
                else
                    info.Kind = SyntaxKind.MinusToken; break;

            case '*':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.AsteriskToken;
                break;

            case '/':
                this.TextWindow.AdvanceChar();
                if (this.TextWindow.PeekChar() == '/')
                {
                    this.TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.SlashSlashToken;
                }
                else
                    info.Kind = SyntaxKind.SlashToken;
                break;

            case '%':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.PersentToken;
                break;

            case '#':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.HashToken;
                break;

            case '&':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.AmpersandToken;
                break;

            case '~':
                this.TextWindow.AdvanceChar();
                if (this.TextWindow.PeekChar() == '=')
                {
                    this.TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.TildeEqualsToken;
                }
                else
                    info.Kind = SyntaxKind.TildeToken;
                break;

            case '|':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.BarToken;
                break;

            case '<':
                this.TextWindow.AdvanceChar();
                switch (this.TextWindow.PeekChar())
                {
                    case '=':
                        this.TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.LessThanEqualsToken;
                        break;

                    case '<':
                        this.TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.LessThanLessThenToken;
                        break;

                    default:
                        info.Kind = SyntaxKind.LessThanToken;
                        break;
                }
                break;

            case '>':
                this.TextWindow.AdvanceChar();
                switch (this.TextWindow.PeekChar())
                {
                    case '=':
                        this.TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.GreaterThanEqualsToken;
                        break;
                    case '<':
                        this.TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.GreaterThanGreaterThenToken;
                        break;

                    default:
                        info.Kind = SyntaxKind.GreaterThenToken;
                        break;
                }
                break;

            case '=':
                this.TextWindow.AdvanceChar();
                if (this.TextWindow.PeekChar() == '=')
                {
                    this.TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.EqualsEqualsToken;
                }
                else
                    info.Kind = SyntaxKind.EqualsToken;
                break;

            case '(':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.OpenParenToken;
                break;

            case ')':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.CloseParenToken;
                break;

            case '{':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.OpenBraceToken;
                break;

            case '}':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.CloseBraceToken;
                break;

            case '[':
                switch (this.TextWindow.PeekChar(2))
                {
                    case '[':
                        this.ScanMultiLineStringLiteral(ref info);
                        break;

                    case '=':
                        for (int i = 3; ; i++)
                        {
                            char nextChar = this.TextWindow.PeekChar(i);
                            if (nextChar == '=') continue;
                            else if (nextChar == '[')
                                this.ScanMultiLineStringLiteral(ref info, i - 2);
                            else goto default;
                        }
                        if (info.Kind == SyntaxKind.None) goto default; // 未匹配到完整的多行原始字符字面量的起始语法。
                        break;

                    default:
                        this.TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.OpenBracketToken;
                        break;
                }
                break;

            case ']':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.CloseBracketToken;
                break;

            case ':':
                this.TextWindow.AdvanceChar();
                if (this.TextWindow.PeekChar() == ':')
                {
                    this.TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.ColonColonToken;
                }
                else
                    info.Kind = SyntaxKind.ColonToken;
                break;

            case ';':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.SemicolonToken;
                break;

            case ',':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.CommanToken;
                break;

            case '.':
                this.TextWindow.AdvanceChar();
                if (this.TextWindow.PeekChar() == '.')
                {
                    this.TextWindow.AdvanceChar();
                    if (this.TextWindow.PeekChar() == '.')
                    {
                        this.TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.DotDotDotToken;
                    }
                    else
                        info.Kind = SyntaxKind.DotDotToken;
                }
                else
                    info.Kind = SyntaxKind.DotToken;
                break;

            // 字符串字面量
            case '\"':
            case '\'':
                this.ScanSingleStringLiteral(ref info);
                break;

            case 'a':
            case 'b':
            case 'c':
            case 'd':
            case 'e':
            case 'f':
            case 'g':
            case 'h':
            case 'i':
            case 'j':
            case 'k':
            case 'l':
            case 'm':
            case 'n':
            case 'o':
            case 'p':
            case 'q':
            case 'r':
            case 's':
            case 't':
            case 'u':
            case 'v':
            case 'w':
            case 'x':
            case 'y':
            case 'z':
            case 'A':
            case 'B':
            case 'C':
            case 'D':
            case 'E':
            case 'F':
            case 'G':
            case 'H':
            case 'I':
            case 'J':
            case 'K':
            case 'L':
            case 'M':
            case 'N':
            case 'O':
            case 'P':
            case 'Q':
            case 'R':
            case 'S':
            case 'T':
            case 'U':
            case 'V':
            case 'W':
            case 'X':
            case 'Y':
            case 'Z':
            case '_':
                this.ScanIdentifierOrKeyword(ref info);
                break;

            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                this.ScanNumericLiteral(ref info);
                break;

            case SlidingTextWindow.InvalidCharacter:
                if (!this.TextWindow.IsReallyAtEnd())
                    goto default;
                else
                    info.Kind = SyntaxKind.EndOfFileToken;
                break;

            default:
                if (SyntaxFacts.IsIdentifierStartCharacter(this.TextWindow.PeekChar()))
                    goto case 'a';

                this.TextWindow.AdvanceChar();

                if (this._badTokenCount++ > 200)
                {
                    //当遇到大量无法决定的字符时，将剩下的输出也合并入。
                    int end = this.TextWindow.Text.Length;
                    int width = end - startingPosition;
                    info.Text = this.TextWindow.Text.ToString(new(startingPosition, width));
                    this.TextWindow.Reset(end);
                }
                else
                    info.Text = this.TextWindow.GetText(intern: true);

                this.AddError(ErrorCode.ERR_UnexpectedCharacter, info);
                break;
        }
    }

#warning 未完成
}
