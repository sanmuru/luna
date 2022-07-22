using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

partial class Lexer
{
    internal readonly struct Interpolation
    {
        /// <summary>
        /// 插值语法的起始语法（“#{”）的范围。
        /// </summary>
        public readonly Range StartRange;

        public readonly ImmutableArray<SyntaxToken> InnerTokens;

        /// <summary>
        /// 插值语法的结尾语法（“}”）的范围。
        /// </summary>
        public readonly Range EndRange;

        public Interpolation(Range startRange, ImmutableArray<SyntaxToken> innerTokens, Range endRange)
        {
            this.StartRange = startRange;
            this.InnerTokens = innerTokens;
            this.EndRange = endRange;
        }
    }

    [NonCopyable]
    private ref partial struct InterpolatedStringScanner
    {
        private readonly Lexer _lexer;
        private SyntaxDiagnosticInfo? _error = null;

        /// <summary>
        /// 获取或设置扫描过程中搜集到的错误。
        /// 一旦搜集到了一个错误，我们就应在下一个可能的结束位置停下解析，以避免混淆错误提示。
        /// </summary>
        public SyntaxDiagnosticInfo? Error { get => this._error; set => this._error ??= value; }

        public InterpolatedStringScanner(Lexer lexer) => _lexer = lexer;

        private bool IsAtEnd() => this.IsAtEnd(true);

        private bool IsAtEnd(bool allowNewline)
        {
            char c = this._lexer.TextWindow.PeekChar();
            return
                (!allowNewline && SyntaxFacts.IsNewLine(c)) ||
                (c == SlidingTextWindow.InvalidCharacter && _lexer.TextWindow.IsReallyAtEnd());
        }

        public bool ScanInterpolatedStringLiteral(ref TokenInfo info)
        {
            char quote = this._lexer.TextWindow.NextChar();
            Debug.Assert(quote == '"');

            var buffer = ArrayBuilder<BuilderStringLiteralToken>.GetInstance(); // 缓存需要重设缩进量的语法标志。
            var builder = ArrayBuilder<SyntaxToken>.GetInstance();
            /* 按照合法语法，插值字符串字面量由至少两个插值字符串字面量文本标志构成。
             * 因为进行向后扫描，所以仅有最后一个插值字符串字面量文本标志不会检测到存在插值语法。
             */
            var hasInterpolation = false;
            var minIndent = int.MaxValue;
            var isLastTokenAtEndOfLine = false;
            while (true)
            {
                // 扫描一个字符串字面量。
                if (this.ScanInterpolatedStringLiteralText(quote, ref hasInterpolation, out var spanBuilder, ref minIndent))
                {

                    // 扫描到符合字符串字面量格式的标志。
                    SyntaxDiagnosticInfo[]? errors = this.Error is null ? null : new[] { this.Error };
                    if (hasInterpolation) // 存在插值语法，则是插值字符串字面量文本标志。
                    {
                        var builderToken = createBuilderToken(this._lexer, spanBuilder, errors);
                        buffer.Add(builderToken);
                        builder.Add(builderToken);
                        // 若上一个标志位于行尾，则表明需要检查扫描到的字符串字面量的缩进量。
                        if (isLastTokenAtEndOfLine)
                        {
                            minIndent = Math.Min(minIndent, builderToken.GetWhiteSpaceIndent());
                        }
                        isLastTokenAtEndOfLine = builderToken.IsTokenAtEndOfLine();

                        // 添加插值内容的语法标志集。
                        var contentTokens = this.ScanInterpolatedStringContent();
                        foreach (var innerToken in contentTokens)
                        {
                            builder.Add(innerToken);
                            isLastTokenAtEndOfLine = innerToken.IsTokenAtEndOfLine();
                        }
                        contentTokens.Free();
                    }
                    else // 不存在插值语法。
                    {
                        if (builder.Count == 0) // 若是第一个扫描到的标志，则表示普通字符串字面量标志；
                            return false; // 此方法仅处理插值字符串字面量，因此返回失败。

                        // 最后一个也应为插值字符串字面量文本标志。
                        var builderToken = createBuilderToken(this._lexer, spanBuilder, errors);
                        buffer.Add(builderToken);
                        builder.Add(builderToken);
                        // 若上一个标志位于行尾，则表明需要检查扫描到的字符串字面量的缩进量。
                        if (isLastTokenAtEndOfLine)
                        {
                            minIndent = Math.Min(minIndent, builderToken.GetWhiteSpaceIndent());
                        }
                        break;
                    }
                }
            }

            // 传播最小缩进量。
            foreach (var builderToken in buffer)
            {
                builderToken.InnerIndent = minIndent;
            }
            buffer.Free(); // 释放缓存。

            var tokens = builder.ToImmutableOrEmptyAndFree();
            // 复原前后方语法琐碎。
            this._lexer._leadingTriviaCache.Clear();
            this._lexer._leadingTriviaCache.AddRange(tokens[0].LeadingTrivia);
            this._lexer._trailingTriviaCache.Clear();
            this._lexer._trailingTriviaCache.AddRange(tokens[tokens.Length - 1].TrailingTrivia);

            info.Kind = SyntaxKind.InterpolatedStringToken;
            info.Text = this._lexer.TextWindow.GetText(intern: true);
            info.SyntaxTokenArrayValue = tokens;

            return true;

            // 创建一个构建中的插值字符串字面量文本标志。
            static BuilderStringLiteralToken createBuilderToken(Lexer lexer, ArrayBuilder<string?> spanBuilder, SyntaxDiagnosticInfo[]? errors) =>
                new(
                    SyntaxKind.InterpolatedStringTextToken,
                    lexer.TextWindow.GetText(intern: true),
                    spanBuilder,
                    0, // 默认缩进量为0，将在之后更改。
                    lexer._leadingTriviaCache.ToListNode(),
                    lexer._trailingTriviaCache.ToListNode());
        }

        private bool ScanInterpolatedStringLiteralText(char quote, ref bool hasInterpolation, out ArrayBuilder<string?> spanBuilder, ref int minIndent)
        {
            // 仅当第一次进入此方法时hasInterpolation为false，此种情况下前方琐碎内容已分析完毕；
            // 当之后再进入此方法时需要分析前方琐碎内容。
            if (hasInterpolation)
            {
                this._lexer.LexSyntaxLeadingTriviaCore();
            }

            spanBuilder = ArrayBuilder<string?>.GetInstance();
            this._lexer._builder.Clear();

            if (this._lexer.TextWindow.PeekChar() == '}') // 上一个插值的结尾。
            {
                // 若已经扫描到插值，则字符串值中不添加右花括号。
                if (hasInterpolation)
                    this._lexer.TextWindow.AdvanceChar();
                else
                    this._lexer._builder.Append(this._lexer.TextWindow.NextChar());
            }

            while (true)
            {
                char c = this._lexer.TextWindow.PeekChar();
                if (c == quote) // 字符串结尾
                {
                    this._lexer.TextWindow.AdvanceChar();

                    if (this._lexer._builder.Length > 0)
                        spanBuilder.Add(this._lexer._builder.ToString());

                    hasInterpolation = false;
                    break;
                }
                // 字符串中可能包含非正规的Utf-16以外的字符，检查是否真正到达文本结尾来验证这些字符不是由用户代码引入的情况。
                else if (c == SlidingTextWindow.InvalidCharacter && this._lexer.TextWindow.IsReallyAtEnd())
                {
                    Debug.Assert(this._lexer.TextWindow.Width > 0);
                    this.Error = AbstractLexer.MakeError(ErrorCode.ERR_UnterminatedStringLiteral);

                    if (this._lexer._builder.Length > 0)
                        spanBuilder.Add(this._lexer._builder.ToString());

                    hasInterpolation = false;
                    break;
                }
                else if (SyntaxFacts.IsWhiteSpace(c))
                {
                    // 扫描缩进或内容（第一行）状态。
                    this._lexer.TextWindow.AdvanceChar();
                    this._lexer._builder.Append(c);
                }
                else
                {
                    if (spanBuilder.Count % 2 == 1) // 处于扫描缩进状态。
                    {
                        if (this._lexer._builder.Length > 0)
                        {
                            spanBuilder.Add(this._lexer._builder.ToString());
                            this._lexer._builder.Clear();
                        }
                        else
                            spanBuilder.Add(null);
                    }

                    if (c == '\\') // 转义字符前缀
                        this._lexer.ScanEscapeSequence();
                    else if (c == '#' && this._lexer.TextWindow.PeekChar(1) == '{')
                    {
                        if (this._lexer._builder.Length > 0)
                            spanBuilder.Add(this._lexer._builder.ToString());

                        hasInterpolation = true;
                        this._lexer.TextWindow.AdvanceChar(2);
                        break;
                    }
                    else if (SyntaxFacts.IsNewLine(c))
                    {
                        this._lexer.TextWindow.AdvanceChar();
                        if (SyntaxFacts.IsNewLine(c, this._lexer.TextWindow.PeekChar()))
                            this._lexer.TextWindow.AdvanceChar();
                        this._lexer._builder.Append('\n');

                        spanBuilder.Add(this._lexer._builder.ToString());
                        this._lexer._builder.Clear();
                    }
                    else // 普通字符
                    {
                        // 扫描内容状态。
                        this._lexer.TextWindow.AdvanceChar();
                        this._lexer._builder.Append(c);
                    }
                }
            }

            // 找到最小缩进量。
            if (spanBuilder.Count > 1)
            {
                for (int i = 1; i < spanBuilder.Count; i += 2)
                {
                    string? span = spanBuilder[i];
                    if (span is null) // 遇到无缩进量的行，快速退出。
                    {
                        minIndent = 0;
                        break;
                    }
                    minIndent = Math.Min(minIndent, span.Sum(SyntaxFacts.WhiteSpaceIndent));
                }
            }

            return true;
        }

        private ArrayBuilder<SyntaxToken> ScanInterpolatedStringContent()
        {
            var tokens = ArrayBuilder<SyntaxToken>.GetInstance();

            int braceBalance = 0;
            var mode = LexerMode.Syntax;
            while (true)
            {
                int position = this._lexer.TextWindow.Position;

                var token = this._lexer.Lex(mode);
                switch (token.Kind)
                {
                    case SyntaxKind.OpenBraceToken:
                        braceBalance++;
                        break;
                    case SyntaxKind.CloseBraceToken:
                        // 花括号已平衡，且下一个是右花括号标志，终止枚举。
                        if (braceBalance == 0)
                        {
                            this._lexer.Reset(position); // 回到上一个位置。
                            return tokens;
                        }

                        braceBalance--;
                        break;

                    // 直到文件结尾也未能平衡花括号或查看到右花括号，则产生错误。
                    case SyntaxKind.EndOfFileToken:
                        this.Error = AbstractLexer.MakeError(ErrorCode.ERR_UnterminatedStringLiteral);
                        return tokens;
                }

                tokens.Add(token); // 枚举识别到的内部标志。
            }
        }
    }
}
