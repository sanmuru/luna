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

            var builder = SyntaxTokenListBuilder.Create();
            /* 按照合法语法，插值字符串字面量由至少两个插值字符串字面量文本标志构成。
             * 因为进行向后扫描，所以仅有最后一个插值字符串字面量文本标志不会检测到存在插值语法。
             */
            var hasInterpolation = false;
            while (true)
            {
                // 扫描一个字符串字面量。
                if (this.ScanInterpolatedStringLiteralText(quote, ref hasInterpolation))
                {
                    // 扫描到符合字符串字面量格式的标志。
                    SyntaxDiagnosticInfo[]? errors = this.Error is null ? null : new[] { this.Error };
                    if (hasInterpolation) // 存在插值语法，则是插值字符串字面量文本标志。
                    {
                        builder.Add(createToken(this._lexer, errors));

                        foreach (var innerToken in this.ScanInterpolatedStringContent())
                            builder.Add(innerToken);
                    }
                    else // 不存在插值语法。
                    {
                        if (builder.Count == 0) // 若是第一个扫描到的标志，则表示普通字符串字面量标志；
                            return false; // 此方法仅处理插值字符串字面量，因此返回失败。

                        // 前方已扫描了插值字符串字面量文本标志，则后方所有的标志都应视为插值字符串字面量文本标志。
                        builder.Add(createToken(this._lexer, errors));
                        break;
                    }
                }
            }

            info.Kind = SyntaxKind.InterpolatedStringToken;
            info.Text = this._lexer.TextWindow.GetText(intern: true);
            info.SyntaxTokenListValue = builder.ToList();

            return true;

            // 创建一个插值字符串字面量文本标志。
            SyntaxToken createToken(Lexer lexer, SyntaxDiagnosticInfo[]? errors)
            {
                TokenInfo contentInfo = new()
                {
                    Kind = SyntaxKind.InterpolatedStringTextToken,
                    ValueKind = SpecialType.System_String,
                    Text = lexer.TextWindow.GetText(intern: true),
                    StringValue = lexer._builder.Length == 0 ? string.Empty : lexer.TextWindow.Intern(lexer._builder)
                };
                return lexer.Create(contentInfo, null, null, errors);
            }
        }

        private bool ScanInterpolatedStringLiteralText(char quote, ref bool hasInterpolation)
        {
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
                if (c == '\\') // 转义字符前缀
                    this._lexer.ScanEscapeSequence();
                else if (c == '#' && this._lexer.TextWindow.PeekChar(1) == '{')
                {
                    var start = this._lexer.TextWindow.Position;
                    hasInterpolation = true;
                    this._lexer.TextWindow.AdvanceChar(2);
                    return true;
                }
                else if (c == quote) // 字符串结尾
                {
                    this._lexer.TextWindow.AdvanceChar();
                    hasInterpolation = false;
                    return true;
                }
                // 字符串中可能包含非正规的Utf-16以外的字符，检查是否真正到达文本结尾来验证这些字符不是由用户代码引入的情况。
                else if (c == SlidingTextWindow.InvalidCharacter && this._lexer.TextWindow.IsReallyAtEnd())
                {
                    Debug.Assert(this._lexer.TextWindow.Width > 0);
                    this.Error = AbstractLexer.MakeError(ErrorCode.ERR_UnterminatedStringLiteral);
                    hasInterpolation = false;
                    return true;
                }
                else // 普通字符
                {
                    this._lexer.TextWindow.AdvanceChar();
                    this._lexer._builder.Append(c);
                }
            }
        }

        private IEnumerable<SyntaxToken> ScanInterpolatedStringContent()
        {
            List<SyntaxToken> tokens = new();

            int braceBalance = 0;
            var mode = LexerMode.Syntax;
            while (true)
            {
                // 花括号已平衡，且下一个字符是结束花括号，终止枚举。
                if (braceBalance == 0 && this._lexer.TextWindow.PeekChar() == '}')
                    return tokens;

                var token = this._lexer.Lex(mode);
                switch (token.Kind)
                {
                    case SyntaxKind.OpenBraceToken:
                        braceBalance++;
                        break;
                    case SyntaxKind.CloseBraceToken:
                        braceBalance--;
                        break;

                    // 直到文件结尾也未能平衡花括号或查看到结束花括号，则产生错误。
                    case SyntaxKind.EndOfFileToken:
                        this.Error = AbstractLexer.MakeError(ErrorCode.ERR_UnterminatedStringLiteral);
                        return tokens;
                }

                tokens.Add(token); // 枚举识别到的内部标志。
            }

            return tokens;
        }
    }
}
