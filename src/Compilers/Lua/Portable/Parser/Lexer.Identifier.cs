using System.Diagnostics;
using System.Runtime.CompilerServices;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;
#endif

partial class Lexer
{
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
            // 没有后续字符，立即返回结果。
            if (currentOffset == characterWindowCount)
            {
                var length = currentOffset - startOffset;
                if (length == 0) return false;

                this.TextWindow.AdvanceChar(length);
                info.Text = this.TextWindow.Intern(characterWindow, startOffset, length);
                info.StringValue = info.Text;
                return true;
            }

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

            // 处理终止字符。
            else if (
                SyntaxFacts.IsWhiteSpace(c) || // 属于空白字符
                SyntaxFacts.IsNewLine(c) || // 属于换行符
                (c >= 32 && c <= 126)) // 属于ASCII可显示字符范围
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

            // 慢速扫描处理ASCII可显示字符范围外的可用的Unicode字符。
            // 因为SyntaxFacts.IsIdentifierStartCharacter和SyntaxFacts.IsIdentifierPartCharacter是高开销的方法，所以在快速扫描阶段不进行。
            else
            {
                if (currentOffset == startOffset ?
                    SyntaxFacts.IsIdentifierStartCharacter(c) :
                    SyntaxFacts.IsIdentifierPartCharacter(c)
                )
                    continue;
                else
                    return false;
            }
        }
    }
}
