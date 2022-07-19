using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

partial class Lexer
{
    private partial bool ScanStringLiteral(ref TokenInfo info)
    {
        char quote = this.TextWindow.PeekChar();
        Debug.Assert(quote == '\'' || quote == '"');

        if (quote == '"') // 可能是插值字符串字面量。
        {
            int start = this.TextWindow.Position; // 记录起始位置。

            var scanner = new InterpolatedStringScanner(this);
            if (scanner.ScanInterpolatedStringLiteral(ref info)) return true;

            // 将内部扫描器搜集的错误消息传递出来。
            if (scanner.Error is not null)
                this.AddError(scanner.Error);

            info.Text = this.TextWindow.GetText(start, this.TextWindow.Position - start, intern: true);
        }
        else
        {
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
                // 字符串中可能包含非正规的Utf-16以外的字符，检查是否真正到达文本结尾来验证这些字符不是由用户代码引入的情况。
                else if (c == SlidingTextWindow.InvalidCharacter && this.TextWindow.IsReallyAtEnd())
                {
                    Debug.Assert(this.TextWindow.Width > 0);
                    this.AddError(ErrorCode.ERR_UnterminatedStringLiteral);
                    break;
                }
                else // 普通字符
                {
                    this.TextWindow.AdvanceChar();
                    this._builder.Append(c);
                }
            }

            info.Text = this.TextWindow.GetText(intern: true);
        }

        info.Kind = SyntaxKind.StringLiteralToken;
        info.ValueKind = SpecialType.System_String;

        if (this._builder.Length == 0)
            info.StringValue = string.Empty;
        else
            info.StringValue = this.TextWindow.Intern(this._builder);

        return true;
    }
}
