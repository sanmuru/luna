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
}
