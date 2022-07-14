using System.Diagnostics;
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

    public char NextUnicodeDecEscape(out SyntaxDiagnosticInfo? info)
    {
        info = null;

        int start = this.Position;

        char c = this.NextChar();
        Debug.Assert(c == '\\');

        c = this.NextChar();
        Debug.Assert(SyntaxFacts.IsDecDigit(c));

        // 最多识别5位十进制数字（Unicode字符有65535个），提前遇到非十进制数字字符时中断。
        int intChar = 0;
        for (int i = 1; ; i++)
        {
            intChar = intChar * 10 + SyntaxFacts.DecValue(c);
            if (i == 5) break;
            else if (SyntaxFacts.IsDecDigit(this.PeekChar()))
                c = this.NextChar();
            else
                break;
        }
        if (intChar > ushort.MaxValue) // 超出Unicode字符范围。
            info = this.CreateIllegalEscapeDiagnostic(start, ErrorCode.ERR_IllegalEscape);

        return (char)intChar;
    }

    public char NextAsciiHexEscape(out SyntaxDiagnosticInfo? info)
    {
        info = null;

        int start = this.Position;

        char c = this.NextChar();
        Debug.Assert(c == '\\');

        c = this.NextChar();
        Debug.Assert(c == 'x');

        // 识别2位十六进制数字（Ascii字符有256个）。
        int intChar = 0;
        if (SyntaxFacts.IsHexDigit(this.PeekChar()))
        {
            c = this.NextChar();
            intChar = SyntaxFacts.HexValue(c);

            if (SyntaxFacts.IsHexDigit(this.PeekChar()))
            {
                c = this.NextChar();
                intChar = (intChar << 4) + SyntaxFacts.HexValue(c);
                return (char)intChar;
            }
        }

        info = this.CreateIllegalEscapeDiagnostic(start, ErrorCode.ERR_IllegalEscape);
        return SlidingTextWindow.InvalidCharacter;
    }

    public char NextUnicodeHexEscape(out SyntaxDiagnosticInfo? info)
    {
        info = null;

        int start = this.Position;

        char c = this.NextChar();
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
        int intChar = 0;
        for (int i = 1; ; i++)
        {
            intChar = (intChar << 4) + SyntaxFacts.HexValue(c);
            if (intChar > ushort.MaxValue) // 超出Unicode字符范围。
                info ??= this.CreateIllegalEscapeDiagnostic(start, ErrorCode.ERR_IllegalEscape);

            if (SyntaxFacts.IsHexDigit(this.PeekChar()))
                c = this.NextChar();
            else
                break;
        }

        if (this.PeekChar() != '}') // 强制要求的右花括号。
            info ??= this.CreateIllegalEscapeDiagnostic(start, ErrorCode.ERR_IllegalEscape);
        else
            this.AdvanceChar();

        if (info is not null)
            return SlidingTextWindow.InvalidCharacter;

        return (char)intChar;
    }

    private SyntaxDiagnosticInfo CreateIllegalEscapeDiagnostic(int start, ErrorCode code) =>
        new(
            start - this.LexemeStartPosition,
            this.Position - start,
            code);
}
