using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

[Flags]
internal enum LexerMode
{
    None = 0,

    Syntax = 0x0001,
    DebuggerSyntax = 0x0002,

    MaskLexMode = 0xFFFF
}

/// <summary>
/// 针对MoonScript语言特定的词法解析器。
/// </summary>
partial class Lexer
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
        throw new NotImplementedException();
    }

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

                            /* 为了适应MoonScript根据缩进来表示块体，在识别后方琐碎内容时，连续的语法琐碎内容应在行尾换行后截断。
                             * 并且将下一行起始的连续空白字符作为下一个语法标志的前方语法琐碎内容；
                             * 但是当识别前方琐碎内容时，遇到多个空行（行中没有或只有空白字符）时，不应截断。
                             * 应将所有空白字符和换行字符保存在同一个前方语法琐碎内容中，用于后续分析。
                             */
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
}
