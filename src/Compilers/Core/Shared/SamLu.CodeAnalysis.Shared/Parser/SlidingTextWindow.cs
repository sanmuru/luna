using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;
#endif

/// <inheritdoc/>
internal sealed class SlidingTextWindow : SamLu.CodeAnalysis.Syntax.InternalSyntax.SlidingTextWindow
{
    public SlidingTextWindow(SourceText text) : base(text) { }

    public override int GetNewLineWidth() => SlidingTextWindow.GetNewLineWidth(this.PeekChar(0), this.PeekChar(1));

    /// <summary>
    /// 获取换行字符序列的宽度。
    /// </summary>
    /// <param name="currentChar">第一个字符。</param>
    /// <param name="nextChars">后续的字符序列。</param>
    /// <returns>换行字符序列的宽度。</returns>
    public static int GetNewLineWidth(char currentChar, params char[] nextChars)
    {
        Debug.Assert(SyntaxFacts.IsNewLine(currentChar));

        if (nextChars.Length >= 1 && SyntaxFacts.IsNewLine(currentChar, nextChars[0]))
            // "\r\n"
            return 2;
        else
            // 其他1个字符宽度的换行字符序列。
            return 1;
    }

    public override string GetText(int position, int length, bool intern)
    {
        int offset = position - this._basis;

        switch (length)
        {
            case 0: return string.Empty;
            case 1:
                if (this._characterWindow[offset] == ' ')
                    return " ";
                else if (this._characterWindow[offset] == '\n')
                    return "\n";
                break;
            case 2:
                char firstChar = this._characterWindow[offset];
                char nextChar = this._characterWindow[offset + 1];
                if (firstChar == '\r' && nextChar == '\n')
                    return "\r\n";
                else if (firstChar == '-' && nextChar == '-')
                    return "--";
                break;
            case 3:
                if (this._characterWindow[offset] == '-' &&
                    this._characterWindow[offset + 1] == '-' &&
                    this._characterWindow[offset + 2] == ' ')
                    return "-- ";
                break;
        }

        if (intern) return this.Intern(this._characterWindow, offset, length);
        else return new string(this._characterWindow, offset, length);
    }

    public char NextUnicodeDecEscape(out SyntaxDiagnosticInfo? info, out char surrogate)
    {
        info = null;

        int start = this.Position;

        char c = this.NextChar();
        Debug.Assert(c == '\\');

        c = this.NextChar();
        Debug.Assert(SyntaxFacts.IsDecDigit(c));

        // 最多识别7位十进制数字（支持Utf-8字符共1114111个），提前遇到非十进制数字字符时中断。
        uint codepoint = 0;
        for (int i = 1; i <= 7; i++)
        {
            if (codepoint <= 0x10FFFF)
                codepoint = codepoint * 10 + (uint)SyntaxFacts.DecValue(c);
            else if (codepoint != uint.MaxValue)
                codepoint = uint.MaxValue;

            if (i == 7) break;
            else if (SyntaxFacts.IsDecDigit(this.PeekChar()))
                c = this.NextChar();
            else
                break;
        }

        if (codepoint == uint.MaxValue)
        {
            info ??= this.CreateIllegalEscapeDiagnostic(start, ErrorCode.ERR_IllegalEscape);
            surrogate = SlidingTextWindow.InvalidCharacter;
            return SlidingTextWindow.InvalidCharacter;
        }

        return SlidingTextWindow.GetCharsFromUtf32(codepoint, out surrogate);
    }

    public char NextHexEscape(out SyntaxDiagnosticInfo? info, out char surrogate)
    {
        Debug.Assert(this.PeekChar(0) == '\\' || this.PeekChar(1) == 'x');

        info = null;
        int start = this.Position;
        var hasError = false;
        var builder = ArrayBuilder<byte>.GetInstance();

        // 第一个byte必定能获取到。
        Debug.Assert(this.NextHexEscapeCore(0, out var firstByte, ref hasError));
        if (!hasError)
        {
            int count = firstByte switch
            {
                <= 0b01111111 => 1,
                >= 0b11000000 and <= 0b11011111 => 2,
                >= 0b11100000 and <= 0b11101111 => 3,
                >= 0b11110000 and <= 0b11110111 => 4,
                _ => 0
            };
            builder.Add(firstByte);

            for (int index = 1; index < count; index++)
            {
                if (!this.NextHexEscapeCore(index, out var restByte, ref hasError))
                {
                    hasError = true;
                    break;
                }
                else if (!hasError)
                    builder.Add(restByte);
            }
        }

        if (hasError)
        {
            info = this.CreateIllegalEscapeDiagnostic(start, ErrorCode.ERR_IllegalEscape);
            surrogate = SlidingTextWindow.InvalidCharacter;
            return SlidingTextWindow.InvalidCharacter;
        }

        var utf8Bytes = builder.ToArrayAndFree();
        var utf16Bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, utf8Bytes);
        Debug.Assert(utf16Bytes.Length is 2 or 4);
        surrogate = utf16Bytes.Length switch
        {
            2 => InvalidCharacter,
            4 => BitConverter.ToChar(utf16Bytes, 2),
            _ => throw Roslyn.Utilities.ExceptionUtilities.Unreachable
        };
        return BitConverter.ToChar(utf16Bytes, 0);
    }

    private bool NextHexEscapeCore(int index, out byte byteValue, ref bool hasError)
    {
        byteValue = 0;

        if (this.PeekChar(0) != '\\' || this.PeekChar(1) != 'x')
            return false;

        this.AdvanceChar(2);

        char c;
        // 识别2位十六进制数字。
        if (SyntaxFacts.IsHexDigit(this.PeekChar()))
        {
            c = this.NextChar();
            byteValue = (byte)SyntaxFacts.HexValue(c);

            if (SyntaxFacts.IsHexDigit(this.PeekChar()))
            {
                c = this.NextChar();
                byteValue = (byte)((byteValue << 4) + SyntaxFacts.HexValue(c));

                if (index == 0)
                    hasError = byteValue switch
                    {
                        <= 0b01111111 => false,
                        >= 0b11000000 and <= 0b11011111 => false,
                        >= 0b11100000 and <= 0b11101111 => false,
                        >= 0b11110000 and <= 0b11110111 => false,
                        _ => true
                    };
                else
                    hasError = byteValue is not >= 0b10000000 and <= 0b10111111;
            }
            else hasError |= true;
        }
        else hasError |= true;

        return true;
    }

    public char NextUnicodeHexEscape(out SyntaxDiagnosticInfo? info, out char surrogate)
    {
        info = null;

        int start = this.Position;

        char c = this.NextChar();
        surrogate = SlidingTextWindow.InvalidCharacter;
        Debug.Assert(c == '\\');

        c = this.NextChar();
        Debug.Assert(c == 'u');

        if (this.PeekChar() != '{') // 强制要求的左花括号。
        {
            info = this.CreateIllegalEscapeDiagnostic(start, ErrorCode.ERR_IllegalEscape);
            return SlidingTextWindow.InvalidCharacter;
        }
        else
            this.AdvanceChar();

        if (!SyntaxFacts.IsHexDigit(this.PeekChar())) // 至少要有1位十六进制数字。
        {
            info = this.CreateIllegalEscapeDiagnostic(start, ErrorCode.ERR_IllegalEscape);
            return SlidingTextWindow.InvalidCharacter;
        }
        else
            c = this.NextChar();

        // 最少识别1位十六进制数字，提前遇到非十六进制数字字符时中断。
        uint codepoint = 0;
        for (int i = 1; ; i++)
        {
            if (codepoint <= 0x10FFFF)
                codepoint = (codepoint << 4) + (uint)SyntaxFacts.HexValue(c);
            else if (codepoint != uint.MaxValue)
                codepoint = uint.MaxValue;

            if (SyntaxFacts.IsHexDigit(this.PeekChar()))
                c = this.NextChar();
            else
                break;
        }

        if (this.PeekChar() != '}') // 强制要求的右花括号。
        {
            info ??= this.CreateIllegalEscapeDiagnostic(start, ErrorCode.ERR_IllegalEscape);
            return SlidingTextWindow.InvalidCharacter;
        }
        else
            this.AdvanceChar();

        if (codepoint == uint.MaxValue)
        {
            info ??= this.CreateIllegalEscapeDiagnostic(start, ErrorCode.ERR_IllegalEscape);
            return SlidingTextWindow.InvalidCharacter;
        }

        return SlidingTextWindow.GetCharsFromUtf32(codepoint, out surrogate);
    }

    internal static char GetCharsFromUtf32(uint codepoint, out char lowSurrogate)
    {
        if (codepoint < 0x00010000)
        {
            lowSurrogate = InvalidCharacter;
            return (char)codepoint;
        }
        else
        {
            Debug.Assert(codepoint > 0x0000FFFF && codepoint <= 0x0010FFFF);
            lowSurrogate = (char)((codepoint - 0x00010000) % 0x0400 + 0xDC00);
            return (char)((codepoint - 0x00010000) / 0x0400 + 0xD800);
        }
    }

    private SyntaxDiagnosticInfo CreateIllegalEscapeDiagnostic(int start, ErrorCode code) =>
        new(
            start - this.LexemeStartPosition,
            this.Position - start,
            code);
}
