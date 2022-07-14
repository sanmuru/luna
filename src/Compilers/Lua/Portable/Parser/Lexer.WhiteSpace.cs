﻿using System.Diagnostics.CodeAnalysis;
using Roslyn.Utilities;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;
#endif

partial class Lexer
{
    /// <summary>创建表示空白字符的<see cref="SyntaxTrivia"/>对象的函数。</summary>
    Func<SyntaxTrivia>? _createWhiteSpaceTriviaFunction;
    /// <summary>
    /// 创建表示空白字符的<see cref="SyntaxTrivia"/>对象。
    /// </summary>
    /// <returns>使用当前识别到的</returns>
    private SyntaxTrivia CreateWhiteSpaceTrivia() =>
        SyntaxFactory.WhiteSpace(this.TextWindow.GetText(intern: true));

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
}
