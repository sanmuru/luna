﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.LuaSyntaxNode;
using ThisParseOptions = SamLu.CodeAnalysis.Lua.LuaParseOptions;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

using ThisInternalSyntaxNode = SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax.MoonScriptSyntaxNode;
using ThisParseOptions = SamLu.CodeAnalysis.MoonScript.MoonScriptParseOptions;
#endif

internal partial class Lexer : AbstractLexer
{
    /// <summary>标识符缓冲数组初始容量（表示标识符字符长度的初始值）。</summary>
    private const int IdentifierBufferInitialCapacity = 32;
    /// <summary>语法琐碎内容列表初始容量（表示连续的语法琐碎内容中标志数量的初始值）。</summary>
    private const int TriviaListInitialCapacity = 8;

    /// <summary>与解析相关的选项。</summary>
    private readonly ThisParseOptions _options;

    /// <summary>词法分析器的当前分析模式。</summary>
    private LexerMode _mode;
    private readonly StringBuilder _builder;
    /// <summary>标识符缓冲数组。</summary>
    private char[] _identifierBuffer;
    /// <summary>标识符字符长度。</summary>
    private int _identifierLength;
    /// <summary>词法分析器缓存。</summary>
    private readonly LexerCache _cache;
    /// <summary>产生的坏标志的累计数量。</summary>
    private int _badTokenCount;

    /// <summary>
    /// 获取与解析相关的选项。
    /// </summary>
    /// <value>
    /// 与解析相关的选项。
    /// </value>
    public ThisParseOptions Options => this._options;

    /// <summary>
    /// 创建词法分析器的新实例。
    /// </summary>
    /// <param name="options">与解析相关的选项。</param>
    /// <inheritdoc/>
    public Lexer(SourceText text, ThisParseOptions options) : base(text)
    {
        this._options = options;
        this._builder = new();
        this._identifierBuffer = new char[Lexer.IdentifierBufferInitialCapacity];
        this._cache = new();
        this._createQuickTokenFunction = this.CreateQuickToken;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        this._cache.Free();

        base.Dispose();
    }

    private void AddTrivia(ThisInternalSyntaxNode trivia, [NotNull] ref SyntaxListBuilder? list)
    {
        if (this.HasErrors)
            trivia = trivia.WithDiagnosticsGreen(this.GetErrors(leadingTriviaWidth: 0));

        if (list is null)
            list = new(Lexer.TriviaListInitialCapacity);

        list.Add(trivia);
    }

    /// <summary>
    /// 重置词法分析器当前的字符偏移量到指定的位置
    /// </summary>
    /// <param name="position">要重置到的位置。</param>
    public void Reset(int position) => this.TextWindow.Reset(position);

    /// <summary>
    /// 获取指定词法分析器模式枚举值中的模式部分。
    /// </summary>
    /// <param name="mode">要获取模式部分的词法分析器模式枚举值。</param>
    /// <returns><paramref name="mode"/>的模式部分的词法分析器模式枚举值。</returns>
    private static partial LexerMode ModeOf(LexerMode mode);

    /// <summary>
    /// 词法分析器的当前模式的枚举值中的模式部分是否与指定词法分析器模式枚举值相等。
    /// </summary>
    /// <param name="mode">要与词法分析器的当前模式的枚举值中的模式部分相比较的词法分析器模式枚举值。</param>
    /// <returns>若相等，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    private bool ModeIs(LexerMode mode) => Lexer.ModeOf(this._mode) == mode;

    /// <summary>
    /// 使用指定的词法分析器模式分析一个语法标志，并将它调整为分析后的词法分析器的当前模式。
    /// </summary>
    /// <param name="mode">词法分析器的模式。</param>
    /// <returns>分析得到的语法标志。</returns>
    public SyntaxToken Lex(ref LexerMode mode)
    {
        var result = this.Lex(mode);
        mode = this._mode;
        return result;
    }

    /// <summary>
    /// 使用指定的词法分析器模式分析一个语法标志。
    /// </summary>
    /// <param name="mode">词法分析器的模式。</param>
    /// <returns>分析得到的语法标志。</returns>
    public partial SyntaxToken Lex(LexerMode mode);

    /// <summary>前方语法琐碎内容缓存。</summary>
    private SyntaxListBuilder _leadingTriviaCache = new(10);
    /// <summary>后方语法琐碎内容缓存。</summary>
    private SyntaxListBuilder _trailingTriviaCache = new(10);

    /// <summary>
    /// 获取语法琐碎内容缓存的总宽度。
    /// </summary>
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

    /// <summary>
    /// 分析一个语法标志。
    /// </summary>
    /// <returns>分析得到的语法标志。</returns>
    private SyntaxToken LexSyntaxToken()
    {
        // 分析前方语法琐碎内容。
        this.LexSyntaxLeadingTriviaCore();
        var leading = this._leadingTriviaCache;

        // 分析语法标志，并获得标志信息。
        TokenInfo tokenInfo = default;
        this.Start();
        this.ScanSyntaxToken(ref tokenInfo);
        var errors = this.GetErrors(Lexer.GetFullWidth(leading));

        // 分析后方语法琐碎内容。
        this.LexSyntaxTrailingTriviaCore();
        var trailing = this._trailingTriviaCache;

        return this.Create(in tokenInfo, leading, trailing, errors);
    }

    /// <summary>
    /// 分析一个前方语法琐碎内容。
    /// </summary>
    /// <returns>分析得到的前方语法琐碎内容。</returns>
    internal SyntaxTriviaList LexSyntaxLeadingTrivia()
    {
        this.LexSyntaxLeadingTriviaCore();
        return new(
            default,
            this._leadingTriviaCache.ToListNode(),
            position: 0,
            index: 0);
    }

    /// <summary>
    /// 分析一个后方语法琐碎内容。
    /// </summary>
    /// <returns>分析得到的后方语法琐碎内容。</returns>
    internal SyntaxTriviaList LexSyntaxTrailingTrivia()
    {
        this.LexSyntaxTrailingTriviaCore();
        return new(default,
            this._trailingTriviaCache.ToListNode(), position: 0, index: 0);
    }

    /// <summary>
    /// 分析一个前方语法琐碎内容并存入<see cref="Lexer._leadingTriviaCache"/>。
    /// </summary>
    private void LexSyntaxLeadingTriviaCore()
    {
        this._leadingTriviaCache.Clear();
        this.LexSyntaxTrivia(
            afterFirstToken: this.TextWindow.Position > 0,
            isTrailing: false,
            triviaList: ref this._leadingTriviaCache);
    }

    /// <summary>
    /// 分析一个后方语法琐碎内容并存入<see cref="Lexer._trailingTriviaCache"/>。
    /// </summary>
    private void LexSyntaxTrailingTriviaCore()
    {
        this._trailingTriviaCache.Clear();
        this.LexSyntaxTrivia(
            afterFirstToken: true,
            isTrailing: true,
            triviaList: ref this._trailingTriviaCache);
    }

    /// <summary>
    /// 创建一个语法标志。
    /// </summary>
    /// <param name="info">语法标志的相关信息。</param>
    /// <param name="leading">起始的语法列表构造器。</param>
    /// <param name="trailing">结尾的语法列表构造器。</param>
    /// <param name="errors">语法诊断消息数组。</param>
    /// <returns>新的语法标志。</returns>
    /// <remarks>
    /// <paramref name="info"/>应表示标识符，或其字符串值不为<see langword="null"/>。
    /// </remarks>
    private partial SyntaxToken Create(
        in TokenInfo info,
        SyntaxListBuilder? leading,
        SyntaxListBuilder? trailing,
        SyntaxDiagnosticInfo[]? errors);

    /// <summary>
    /// 扫描一个完整的语法标志。
    /// </summary>
    /// <param name="info">指定的标志信息，它将在扫描过程中被修改。</param>
    private partial void ScanSyntaxToken(ref TokenInfo info);

    /// <summary>
    /// 扫描一个整形数字字面量。
    /// </summary>
    /// <param name="info">指定的标志信息，它将在扫描过程中被修改。</param>
    /// <returns>
    /// 若扫描成功，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </returns>
    private partial bool ScanNumericLiteral(ref TokenInfo info);

    /// <summary>
    /// 扫描一个单行字符串字面量。
    /// </summary>
    /// <param name="info">指定的标志信息，它将在扫描过程中被修改。</param>
    /// <returns>
    /// 若扫描成功，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </returns>
    private partial bool ScanSingleLineStringLiteral(ref TokenInfo info);

    /// <summary>
    /// 扫描一个多行字符串字面量。
    /// </summary>
    /// <param name="info">指定的标志信息，它将在扫描过程中被修改。</param>
    /// <returns>
    /// 若扫描成功，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </returns>
    private partial bool ScanMultiLineStringLiteral(ref TokenInfo info, int level = -1);

    /// <summary>
    /// 扫描一个标识符或关键字。
    /// </summary>
    /// <param name="info">指定的标志信息，它将在扫描过程中被修改。</param>
    /// <returns>
    /// 若扫描成功，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </returns>
    private partial bool ScanIdentifierOrKeyword(ref TokenInfo info);

    /// <summary>
    /// 分析一个语法琐碎内容。
    /// </summary>
    /// <param name="afterFirstToken"></param>
    /// <param name="isTrailing">若要分析的是前方语法琐碎内容，则传入<see langword="false"/>；若要分析的是后方语法琐碎内容，则传入<see langword="true"/>。</param>
    /// <param name="triviaList">语法琐碎内容缓存。</param>
    private partial void LexSyntaxTrivia(
        bool afterFirstToken,
        bool isTrailing,
        ref SyntaxListBuilder triviaList);

    /// <summary>
    /// 扫描一个表示新行的字符序列，并返回对应的绿树节点。
    /// </summary>
    /// <returns>表示新行的绿树节点。</returns>
    private ThisInternalSyntaxNode? ScanEndOfLine()
    {
        this._builder.Clear();

        char c = this.TextWindow.PeekChar();
        if (SyntaxFacts.IsNewLine(c))
        {
            while (true)
            {
                this._builder.Append(this.TextWindow.NextChar());
                c = this.TextWindow.PeekChar();

                char firstChar = this._builder[0];
                char[] restChars = new char[this._builder.Length];
                int length = restChars.Length - 1;
                this._builder.CopyTo(1, restChars, 0, length);
                restChars[length] = c;

                if (!SyntaxFacts.IsNewLine(firstChar, restChars))
                    break;
            }

            string newLine = this._builder.ToString();
            return newLine switch
            {
                "\r\n" => SyntaxFactory.CarriageReturnLineFeed,
                "\n" => SyntaxFactory.LineFeed,
                "\r" => SyntaxFactory.CarriageReturn,
                _ => SyntaxFactory.EndOfLine(newLine)
            };
        }

        return null;
    }

    /// <summary>
    /// 扫描一个表示注释的字符序列，并返回对应的语法琐碎内容。
    /// </summary>
    /// <returns>表示注释的语法琐碎内容。</returns>
    private partial SyntaxTrivia ScanComment();

    /// <summary>
    /// 扫描到一行的末尾。
    /// </summary>
    private void ScanToEndOfLine()
    {
        for (
            char c = this.TextWindow.PeekChar();
            !SyntaxFacts.IsNewLine(c) && (c != SlidingTextWindow.InvalidCharacter || !this.TextWindow.IsReallyAtEnd());
            c = this.TextWindow.PeekChar()
        )
            this.TextWindow.AdvanceChar();
    }

    /// <summary>
    /// 扫描长方括号结构（多行注释或字符串常量）。
    /// </summary>
    /// <param name="isTerminal">长方括号结构是否闭合。</param>
    /// <param name="level">如果指定了这个参数，则表示预先匹配到的长方括号结构的级数。</param>
    private bool ScanLongBrackets(out bool isTerminal, int level = -1)
    {
        this._builder.Clear();

        /* 匹配长方括号的开始部分。
         * 开始部分要符合格式：\[=*\[
         */
        if (this.TextWindow.PeekChar() != '[') // 不符合格式。
        {
            isTerminal = default;
            return false;
        }

        // 匹配长方括号的复数等号部分，同时收集级数（等号字符个数）信息，为之后匹配长方括号的结束部分做准备。
        if (level < 0)
        {
            while (true)
            {
                char c = this.TextWindow.PeekChar(level + 1);
                if (c == '=') // 优先扫描等号字符。
                {
                    level++;
                    continue;
                }
                else if (c == '[') // 然后扫描左方括号字符。
                    break;
                else // 不符合格式。
                {
                    isTerminal = default;
                    return false;
                }
            }
        }
        this.TextWindow.AdvanceChar(level + 2);

        /* 接下来一边扫描正常字符，一边匹配相同级数的结束长方括号。
         * 除了非法字符外，扫描到的每一个字符都将被视为注释的一部分。
         * 如果没有匹配到相同级数的结束长方括号，方法仍然返回true表示执行成功，但通过isTerminal参数传出false表示长方括号未闭合。
         */
        while (true)
        {
            char c = this.TextWindow.NextChar();
            if (c == SlidingTextWindow.InvalidCharacter && this.TextWindow.IsReallyAtEnd()) break;

            this._builder.Append(c);
            if (c != ']') continue; // 不进入匹配结束长方括号的代码区域。

            bool isPairedLevel = true;
            for (int i = 0; i < level; i++)
            {
                if (this.TextWindow.PeekChar() == '=')
                    this._builder.Append(this.TextWindow.NextChar());
                else
                {
                    isPairedLevel = false;
                    break;
                }
            }

            if (isPairedLevel && this.TextWindow.PeekChar() == ']') // 长方括号结构完全配对。
            {
                this.TextWindow.AdvanceChar();
                // 由于this._builder的末尾处有结束长方括号结构，因此需要删除这段内容。
                this._builder.Length -= level + 1;

                isTerminal = true;
                return true;
            }
        }

        // 代码执行到这里的情况都是格式不符的情况。
        isTerminal = default;
        return false;
    }
}
