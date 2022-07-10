using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using RealParser = SamLu.CodeAnalysis.RealParser;
using IntegerParser = SamLu.CodeAnalysis.IntegerParser;

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
internal partial class Lexer
{
    private static partial LexerMode ModeOf(LexerMode mode) => mode & LexerMode.MaskLexMode;

    public partial SyntaxToken Lex(LexerMode mode)
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
                throw ExceptionUtilities.UnexpectedValue(Lexer.ModeOf(this._mode));
        }
    }

    private partial SyntaxToken Create(
        in TokenInfo info,
        SyntaxListBuilder? leading,
        SyntaxListBuilder? trailing,
        SyntaxDiagnosticInfo[]? errors)
    {
        Debug.Assert(info.Kind != SyntaxKind.IdentifierToken || info.StringValue is not null);

        var leadingNode = leading?.ToListNode();
        var trailingNode = trailing?.ToListNode();

        SyntaxToken token = info.Kind switch
        {
            // 标识符标志
            SyntaxKind.IdentifierToken => SyntaxFactory.Identifier(info.ContextualKind, leadingNode, info.Text!, info.StringValue!, trailingNode),

            // 数字字面量标志
            SyntaxKind.NumericLiteralToken =>
                info.ValueKind switch
                {
                    // 64位整数
                    SpecialType.System_Int64 => SyntaxFactory.Literal(leadingNode, info.Text!, info.LongValue, trailingNode),
                    SpecialType.System_UInt64 => SyntaxFactory.Literal(leadingNode, info.Text!, info.ULongValue, trailingNode),
                    // 64位双精度浮点数
                    SpecialType.System_Double => SyntaxFactory.Literal(leadingNode, info.Text!, info.DoubleValue, trailingNode),
                    _ => throw ExceptionUtilities.UnexpectedValue(info.ValueKind),
                },

            // 字符串字面量标志
            SyntaxKind.StringLiteralToken or
            // 多行原始字符串字面量标志
            SyntaxKind.MultiLineRawStringLiteralToken => SyntaxFactory.Literal(leadingNode, info.Text!, info.Kind, info.StringValue!, trailingNode),

            // 文件结尾标志
            SyntaxKind.EndOfFileToken => SyntaxFactory.Token(leadingNode, SyntaxKind.EndOfFileToken, trailingNode),

            // 异常枚举值
            SyntaxKind.None => SyntaxFactory.BadToken(leadingNode, info.Text!, trailingNode),

            // 标点或关键字
            _ => SyntaxFactory.Token(leadingNode, info.Kind, trailingNode)
        };

        // 为标志添加诊断。
        if (errors is not null)
            token = token.WithDiagnosticsGreen(errors);

        return token;
    }

    /// <summary>创建表示空白字符的<see cref="SyntaxTrivia"/>对象的函数。</summary>
    Func<SyntaxTrivia>? _createWhiteSpaceTriviaFunction;
    /// <summary>
    /// 创建表示空白字符的<see cref="SyntaxTrivia"/>对象。
    /// </summary>
    /// <returns>使用当前识别到的</returns>
    private SyntaxTrivia CreateWhiteSpaceTrivia() =>
        SyntaxFactory.WhiteSpace(this.TextWindow.GetText(intern: true));

    private partial void ScanSyntaxToken(ref TokenInfo info)
    {
        // 初始化以准备新的标志扫描。
        info.Kind = SyntaxKind.None;
        info.ContextualKind = SyntaxKind.None;
        info.Text = null;
        char c;
        int startingPosition = this.TextWindow.Position;

        // 开始扫描标志。
        c = this.TextWindow.PeekChar();
        switch (c)
        {
            case '+':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.PlusToken;
                break;

            case '-':
                this.TextWindow.AdvanceChar();
                info.Kind = SyntaxKind.MinusToken;
                break;
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
                        info.Kind = SyntaxKind.LessThanLessThanToken;
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
                        info.Kind = SyntaxKind.GreaterThanGreaterThanToken;
                        break;

                    default:
                        info.Kind = SyntaxKind.GreaterThanToken;
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
                            {
                                this.ScanMultiLineStringLiteral(ref info, i - 1);
                                break;
                            }
                            else goto default; // 未匹配到完整的多行原始字符字面量的起始语法。
                        }
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
                if (!this.ScanNumericLiteral(ref info))
                {
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
                }
                break;

            // 字符串字面量
            case '\"':
            case '\'':
                this.ScanSingleLineStringLiteral(ref info);
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

    private void CheckFeatureAvaliability(MessageID feature)
    {
        var info = feature.GetFeatureAvailabilityDiagnosticInfo(this.Options);
        if (info is not null)
            this.AddError(info.Code, info.Arguments);
    }

    [MemberNotNull(nameof(Lexer._createWhiteSpaceTriviaFunction))]
    private SyntaxTrivia ScanWhiteSpace()
    {
        this._createWhiteSpaceTriviaFunction ??= this.CreateWhiteSpaceTrivia;
        int hashCode = Hash.FnvOffsetBias;
        bool onlySpaces = true;

NextChar:
        char c = this.TextWindow.PeekChar();
        switch (c)
        {
            // 连续处理空白符。
            case ' ':
                this.TextWindow.AdvanceChar();
                hashCode = Hash.CombineFNVHash(hashCode, c);
                goto NextChar;

            // 遇到换行符停下。
            case '\r':
            case '\n':
                break;

            default:
                // 处理其他空白字符，但注明并非仅普通的空格字符。
                if (SyntaxFacts.IsWhiteSpace(c))
                {
                    onlySpaces = false;
                    goto case ' ';
                }
                break;
        }

        if (this.TextWindow.Width == 1 && onlySpaces)
            return SyntaxFactory.Space;
        else
        {
            var width = this.TextWindow.Width;
            if (width < Lexer.MaxCachedTokenSize)
                return this._cache.LookupTrivia(
                    this.TextWindow.CharacterWindow,
                    this.TextWindow.LexemeRelativeStart,
                    width,
                    hashCode,
                    this._createWhiteSpaceTriviaFunction);
            else
                return this._createWhiteSpaceTriviaFunction();
        }
    }

    private partial bool ScanIdentifierOrKeyword(ref TokenInfo info)
    {
        info.ContextualKind = SyntaxKind.None;

        if (this.ScanIdentifier(ref info))
        {
            Debug.Assert(info.Text is not null);

            if (!this._cache.TryGetKeywordKind(info.Text, out info.Kind))
            {
                info.Kind = SyntaxKind.IdentifierToken;
                info.ContextualKind = info.Kind;
            }
            else if (SyntaxFacts.IsContextualKeyword(info.Kind))
            {
                info.ContextualKind = info.Kind;
                info.Kind = SyntaxKind.IdentifierToken;
            }

            // 排除关键字，剩下的必然是标识符。
            if (info.Kind == SyntaxKind.None)
                info.Kind = SyntaxKind.IdentifierToken;

            return true;
        }
        else
        {
            info.Kind = SyntaxKind.None;
            return false;
        }
    }

    private bool ScanIdentifier(ref TokenInfo info) =>
        ScanIdentifier_FastPath(ref info) || ScanIdentifier_SlowPath(ref info);

    /// <summary>快速扫描标识符。</summary>
    private bool ScanIdentifier_FastPath(ref TokenInfo info) =>
        this.ScanIdentifierCore(ref info, isFastPath: true);

    /// <summary>慢速扫描标识符。</summary>
    private bool ScanIdentifier_SlowPath(ref TokenInfo info) =>
        this.ScanIdentifierCore(ref info, isFastPath: false);

    /// <summary>扫描标识符的实现方法。</summary>
    /// <remarks>此方法应尽可能地内联以减少调用深度。</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ScanIdentifierCore(ref TokenInfo info, bool isFastPath)
    {
        var currentOffset = this.TextWindow.Offset;
        var characterWindow = this.TextWindow.CharacterWindow;
        var characterWindowCount = this.TextWindow.CharacterWindowCount;

        var startOffset = currentOffset;

        while (true)
        {
            // 没有后续字符，立即返回扫描失败。
            if (currentOffset == characterWindowCount)
                return false;

            var c = characterWindow[currentOffset++];

            // 数字
            if (c >= '0' && c <= '9')
            {
                // 首字符不能是数字。
                if (currentOffset == startOffset)
                    return false;
                else
                    continue;
            }
            // 拉丁字符
            else if (c >= 'a' && c <= 'z')
                continue;
            else if (c >= 'A' && c <= 'Z')
                continue;
            // 下划线
            else if (c == '_')
                continue;
            // ASCII可显示字符范围外的可用的Unicode字符
            // 因为SyntaxFacts.IsIdentifierStartCharacter和SyntaxFacts.IsIdentifierPartCharacter是高开销的方法，所以在快速扫描阶段不进行。
            else if (!isFastPath)
            {
                if (currentOffset == startOffset ?
                    SyntaxFacts.IsIdentifierStartCharacter(c) :
                    SyntaxFacts.IsIdentifierPartCharacter(c)
                )
                    continue;
                else
                    return false;
            }

            // 处理终止字符。
            if (
                SyntaxFacts.IsWhiteSpace(c) || // 属于空白字符
                SyntaxFacts.IsNewLine(c) || // 属于换行符
                (c >= 32 || c <= 126)) // 属于ASCII可显示字符范围
            {
                currentOffset--;
                var length = currentOffset - startOffset;
                this.TextWindow.AdvanceChar(length);
                info.Text = this.TextWindow.Intern(characterWindow, startOffset, length);
                info.StringValue = info.Text;
                return true;
            }
            // 其余字符一律留给慢速扫描处理。
            else if (isFastPath)
                return false;
            // 慢速扫描处理
            else
                // 目前没有找到例外情况。
                Debug.Fail($"扫描标识符时遇到意外的终止字符“{c}”。");
        }
    }

    #region 数字字面量
    /// <summary>
    /// 扫描一个完整的整型数字字面量。
    /// </summary>
    /// <param name="integerIsAbsent">扫描到的整型数字字面量是否缺失。</param>
    /// <param name="isHex">是否是十六进制格式。</param>
    private void ScanNumericLiteralSingleInteger(ref bool integerIsAbsent, bool isHex)
    {
        char c;
        while (true)
        {
            c = this.TextWindow.PeekChar();
            if (isHex ?
                SyntaxFacts.IsHexDigit(c) :
                SyntaxFacts.IsDecDigit(c)
            )
            {
                this._builder.Append(c); // 将接受的字符推入缓存。
                integerIsAbsent = false;
                this.TextWindow.AdvanceChar();
            }
            else break;
        }
    }

    /// <summary>
    /// 扫描一个数字字面量
    /// </summary>
    /// <param name="info">要填充的标志信息对象。</param>
    /// <returns>
    /// 若扫描成功，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </returns>
    /// <remarks>
    /// 扫描接受的格式有：
    /// <list type="bullet">
    ///     <item>
    ///         <term>十进制小数格式</term>
    ///         <description>由于第一个字符为<c>.</c>，所以没有十六进制前缀。</description>
    ///     </item>
    ///     <item>
    ///         <term>十进制整数格式</term>
    ///         <description>不含小数点；后方有可选的指数表示：<c>e</c>或<c>E</c>后跟可正可负十进制整型数字。</description>
    ///     </item>
    ///     <item>
    ///         <term>十六进制整数或小数格式</term>
    ///         <description>小数格式时含小数点，整数部分和小数部分不能同时缺省；前方有<c>0x</c>或<c>0X</c>前缀；后方有可选的指数表示：<c>p</c>或<c>P</c>后跟可正可负十进制整型数字。</description>
    ///     </item>
    /// </list>
    /// </remarks>
    private partial bool ScanNumericLiteral(ref TokenInfo info)
    {
        bool isHex = false; // 是否为十六进制。
        bool hasDecimal = false; // 是否含有小数部分。
        bool hasExponent = false; // 是否含有指数部分。
        bool integeralPartIsAbsent = true; // 整数部分是否缺省。
        bool fractionalPartIsAbsent = true; // 小数部分是否缺省。
        info.Text = null;
        info.ValueKind = SpecialType.None;
        this._builder.Clear();

        // 扫描可能存在的十六进制前缀。
        char c = this.TextWindow.PeekChar();
        if (c == '0')
        {
            c = this.TextWindow.PeekChar(1);
            switch (c)
            {
                case 'x':
                case 'X':
                    this.TextWindow.AdvanceChar(2);
                    isHex = true;
                    break;
            }
        }

        /* 向后扫描一个完整的整型数字字面量，可能遇到这个字面量的宽度为零的情况。
         * 作为小数格式的整数部分时是合法的，但是作为整数格式时是不合法的。
         * 后者情况将在排除前者情况后生成诊断错误。
         */
        this.ScanNumericLiteralSingleInteger(ref integeralPartIsAbsent, isHex);

        int resetMarker = this.TextWindow.Position; // 回退记号。
        if (this.TextWindow.PeekChar() == '.') // 扫描小数点。
        {
            c = this.TextWindow.PeekChar(1);
            if (isHex ?
                SyntaxFacts.IsHexDigit(c) :
                SyntaxFacts.IsDecDigit(c)
            ) // 符合小数部分格式。
            {
                // 确认含有小数部分。
                hasDecimal = true;
                this._builder.Append('.');
                this.TextWindow.AdvanceChar();

                // 先将回退记号推进到第一个连续的0-9的最后一位。
                this.ScanNumericLiteralSingleInteger(ref fractionalPartIsAbsent, isHex: false);
                resetMarker = this.TextWindow.Position;

                this.ScanNumericLiteralSingleInteger(ref fractionalPartIsAbsent, isHex);
                Debug.Assert(fractionalPartIsAbsent == false); // 必定存在小数部分。
            }
            else if (integeralPartIsAbsent)
            {
                // 整数和小数部分同时缺失。
                if (isHex) // 存在十六进制前缀，则推断数字字面量格式错误。
                    this.AddError(Lexer.MakeError(ErrorCode.ERR_InvalidNumber));
                else // 除了一个“.”以外没有任何其他字符，推断不是数字字面量标志。
                    return false;

            }
            else
                // 小数部分缺失。
                hasDecimal = true;
        }

        // 现在数字部分已经处理完，接下来处理指数表示。
        c = this.TextWindow.PeekChar();
        if (isHex ?
            c == 'p' || c == 'P' :
            c == 'e' || c == 'E'
        )
        {
            char c2 = this.TextWindow.PeekChar(1);
            char sign = char.MaxValue;
            bool signedExponent = false;
            if (c2 == '-' || c2 == '+') // 有符号指数
            {
                signedExponent = true;
                sign = c2;
                c2 = this.TextWindow.PeekChar(2);
            }

            if (SyntaxFacts.IsDecDigit(c2)) // 符合指数格式。
            {
                // 确认含有指数部分。
                hasExponent = true;
                hasDecimal = true;
                this._builder.Append(c);

                if (signedExponent)
                {
                    this._builder.Append(sign);
                    this.TextWindow.AdvanceChar(2);
                }
                else
                {
                    this.TextWindow.AdvanceChar();
                }

                bool exponentPartIsAbsent = true;
                this.ScanNumericLiteralSingleInteger(ref exponentPartIsAbsent, isHex: false);
                Debug.Assert(exponentPartIsAbsent == false); // 必定存在指数部分。
            }
        }
        // 指数部分格式不符，为了防止破坏后续的标志，尽可能回退到上一个可接受的回退记号的位置。
        if (!hasExponent)
        {
            if (resetMarker != this.TextWindow.Position)
            {
                int length = this.TextWindow.Position - resetMarker;
                this._builder.Remove(this._builder.Length - length, length);
                this.Reset(resetMarker);
            }
        }

        // 填充标志信息前最后一步：检查特性的可用性。
        if (isHex)
        {
            if (hasDecimal) // 十六进制浮点数自Lua 5.2添加，需要检查特性是否可用。
                this.CheckFeatureAvaliability(MessageID.IDS_FeatureHexadecimalFloatConstant);

            if (hasExponent) // 以2为底数的指数表示自Lua 5.2添加，需要检查特性是否可用。
                this.CheckFeatureAvaliability(MessageID.IDS_FeatureBinaryExponent);
        }

        info.Kind = SyntaxKind.NumericLiteralToken;
        info.Text = this.TextWindow.GetText(true);
        Debug.Assert(info.Text is not null);
        var valueText = this.TextWindow.Intern(this._builder);
        if (hasDecimal)
            this.ParseRealValue(ref info, valueText, isHex);
        else
            this.ParseIntegerValue(ref info, valueText, isHex);

        return true;
    }

    /// <summary>
    /// 解析整型数字。
    /// </summary>
    private void ParseIntegerValue(ref TokenInfo info, string text, bool isHex)
    {
        if (isHex)
        {
            if (IntegerParser.TryParseHexadecimalInt64(text, out long result))
            {
                info.ValueKind = SpecialType.System_Int64;
                info.LongValue = result;
                return;
            }
        }
        else
        {
            if (IntegerParser.TryParseDecimalInt64(text, out ulong result))
            {
                if (result <= (ulong)long.MaxValue)
                {
                    info.ValueKind = SpecialType.System_Int64;
                    info.LongValue = (long)result;
                }
                else
                {
                    info.ValueKind = SpecialType.System_UInt64;
                    info.ULongValue = result;
                }
                return;
            }
        }

        this.AddError(Lexer.MakeError(ErrorCode.ERR_NumberOverflow));
    }

    /// <summary>
    /// 解析浮点型数字。
    /// </summary>
    private void ParseRealValue(ref TokenInfo info, string text, bool isHex)
    {
        if (isHex)
        {
            if (RealParser.TryParseHexadecimalDouble(text, out double result))
            {
                info.ValueKind = SpecialType.System_Double;
                info.DoubleValue = result;
                return;
            }
        }
        else
        {
            if (RealParser.TryParseDecimalDouble(text, out double result))
            {
                info.ValueKind = SpecialType.System_Double;
                info.DoubleValue = result;
                return;
            }
        }

        this.AddError(Lexer.MakeError(ErrorCode.ERR_NumberOverflow));
    }
    #endregion

    #region 字符串字面量
    private partial bool ScanSingleLineStringLiteral(ref TokenInfo info)
    {
        char quote = this.TextWindow.NextChar();
        Debug.Assert(quote == '\'' || quote == '"');

        this._builder.Clear();

        while (true)
        {
            char c = this.TextWindow.PeekChar();
            if (c == '\\') // 转义字符前缀
                this.ScanEscapeSequence();
            else if (c == quote) // 字符串结尾
            {
                this.TextWindow.AdvanceChar();
                break;
            }
            // 字符串可能中包含非正规的Utf-16以外的字符，检查是否真正到达文本结尾来验证这些字符不是由用户代码引入的情况。
            else if (SyntaxFacts.IsNewLine(c) ||
                (c == SlidingTextWindow.InvalidCharacter && this.TextWindow.IsReallyAtEnd())
            )
            {
                Debug.Assert(this.TextWindow.Width > 0);
                this.AddError(ErrorCode.ERR_NewlineInConst);
                break;
            }
            else // 普通字符
            {
                this.TextWindow.AdvanceChar();
                this._builder.Append(c);
            }
        }

        info.Kind = SyntaxKind.StringLiteralToken;
        info.ValueKind = SpecialType.System_String;
        info.Text = this.TextWindow.GetText(intern: true);

        if (this._builder.Length == 0)
            info.StringValue = string.Empty;
        else
            info.StringValue = this.TextWindow.Intern(this._builder);

        return true;
    }


    /// <summary>
    /// 扫描一个转义序列。
    /// </summary>
    private void ScanEscapeSequence()
    {
        var start = this.TextWindow.Position;
        SyntaxDiagnosticInfo? error;

        char c = this.TextWindow.NextChar();

        Debug.Assert(c == '\\');

        c = this.TextWindow.NextChar();
        switch (c)
        {
            // 转义后返回自己的字符
            case '\'':
            case '"':
            case '\\':
                this._builder.Append(c);
                break;

            // 常用转义字符
            case 'a':
                this._builder.Append('\a');
                break;
            case 'b':
                this._builder.Append('\b');
                break;
            case 'f':
                this._builder.Append('\f');
                break;
            case 'n':
                this._builder.Append('\n');
                break;
            case 'r':
                this._builder.Append('\r');
                break;
            case 't':
                this._builder.Append('\t');
                break;
            case 'v':
                this._builder.Append('\v');
                break;

            // 十进制数字表示的Unicode字符
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
                this.TextWindow.Reset(start);
                c = this.TextWindow.NextUnicodeDecEscape(out error);
                if (c != SlidingTextWindow.InvalidCharacter)
                    this._builder.Append(c);
                this.AddError(error);
                break;

            // 十六进制数字表示的ASCII字符
            case 'x':
                this.TextWindow.Reset(start);
                c = this.TextWindow.NextAsciiHexEscape(out error);
                if (c != SlidingTextWindow.InvalidCharacter)
                    this._builder.Append(c);
                this.AddError(error);
                break;

            // 十六进制数字表示的Unicode字符
            case 'u':
                this.TextWindow.Reset(start);
                c = this.TextWindow.NextUnicodeHexEscape(out error);
                if (c != SlidingTextWindow.InvalidCharacter)
                    this._builder.Append(c);
                this.AddError(error);
                break;

            // 后方紧跟的连续的字面量的空白字符和换行字符。
            case 'z':
                c = this.TextWindow.PeekChar();
                while (SyntaxFacts.IsWhiteSpace(c) || SyntaxFacts.IsNewLine(c))
                {
                    this.TextWindow.AdvanceChar();

                    // 跳过这些字符。

                    c = this.TextWindow.PeekChar();
                }
                break;

            // 插入换行字符序列本身。
            // Windows系统的换行字符序列为“\r\n”；
            // Unix系统的换行字符序列为“\n”；
            // Mac系统的换行字符序列为“\r”。
            case '\r':
            case '\n':
                this._builder.Append('\n');
                if (c == '\r' && this.TextWindow.PeekChar() == '\n')
                    this.TextWindow.AdvanceChar(); // 跳过这个字符。
                break;

            default:
                this.AddError(
                    start,
                    this.TextWindow.Position - start,
                    ErrorCode.ERR_IllegalEscape);
                break;
        }
    }

    private partial bool ScanMultiLineStringLiteral(ref TokenInfo info, int level)
    {
        if (this.ScanLongBrackets(out var isTerminal, level))
        {
            info.Kind = SyntaxKind.MultiLineRawStringLiteralToken;
            info.ValueKind = SpecialType.System_String;
            info.Text = this.TextWindow.GetText(intern: true);
            info.StringValue = this.TextWindow.Intern(this._builder);

            if (!isTerminal)
                this.AddError(ErrorCode.ERR_UnterminatedStringLiteral);

            return true;
        }

        return false;
    }
    #endregion

    #region 语法琐碎内容
    private partial void LexSyntaxTrivia(
        bool afterFirstToken,
        bool isTrailing,
        ref SyntaxListBuilder triviaList)
    {
        while (true)
        {
            this.Start();
            char c = this.TextWindow.PeekChar();
            if (c == ' ')
            {
                this.AddTrivia(this.ScanWhiteSpace(), ref triviaList);
                continue;
            }
            else if (c > 127)
            {
                if (SyntaxFacts.IsWhiteSpace(c))
                    c = ' ';
                else if (SyntaxFacts.IsNewLine(c))
                    c = '\n';
            }

            switch (c)
            {
                case ' ':
                case '\t':
                case '\v':
                case '\f':
                case '\u001A':
                    this.AddTrivia(this.ScanWhiteSpace(), ref triviaList);
                    continue;

                case '-':
                    if (this.TextWindow.PeekChar(1) == '-')
                    {
                        this.TextWindow.AdvanceChar(2);
                        this.AddTrivia(this.ScanComment(), ref triviaList);
                        continue;
                    }
                    else goto default;

                default:
                    {
                        var endOfLine = this.ScanEndOfLine();
                        if (endOfLine is not null)
                        {
                            this.AddTrivia(endOfLine, ref triviaList);
                            if (isTrailing)
                                // 当分析的是后方语法琐碎内容时，分析成功后直接退出。
                                return;
                            else
                                // 否则进行下一个语法琐碎内容的分析。
                                continue;
                        }
                    }

                    // 下一个字符不是空白字符，终止扫描。
                    return;
            }
        }
    }

    private partial LuaSyntaxNode? ScanEndOfLine()
    {
        char c1 = this.TextWindow.PeekChar();
        char c2 = this.TextWindow.PeekChar(1);
        if (SyntaxFacts.IsNewLine(c1, c2))
        {
            this.TextWindow.AdvanceChar(2);
            if (c1 == '\r' && c2 == '\n')
                return SyntaxFactory.CarriageReturnLineFeed;
            else
                return SyntaxFactory.EndOfLine(new string(new[] { c1, c2 }));
        }
        else if (SyntaxFacts.IsNewLine(c1))
        {
            this.TextWindow.AdvanceChar();
            if (c1 == '\n')
                return SyntaxFactory.LineFeed;
            else if (c1 == '\r')
                return SyntaxFactory.CarriageReturn;
            else
                return SyntaxFactory.EndOfLine(new string(new[] { c1 }));
        }

        return null;
    }

    private partial SyntaxTrivia ScanComment()
    {
        if (this.ScanLongBrackets(out bool isTerminal))
        {
            if (!isTerminal)
                this.AddError(ErrorCode.ERR_OpenEndedComment);
        }
        else
            this.ScanToEndOfLine(isTrim: true);

        var text = this.TextWindow.GetText(intern: false);
        return SyntaxFactory.Comment(text);
    }
    #endregion
}
