using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

internal partial class SyntaxToken
{
    /// <summary>
    /// 获取此语法标志的空白缩进量。
    /// </summary>
    public virtual int WhiteSpaceIndent
    {
        get
        {
            int indent = 0;
            bool isContinuedWhiteSpace = true;
            foreach (var node in this.LeadingTrivia)
            {
                Debug.Assert(node is SyntaxTrivia);
                var trivia = (SyntaxTrivia)node;
                if (trivia.IsTriviaWithEndOfLine())
                {
                    indent = 0;
                    isContinuedWhiteSpace = true;
                }
                else if (trivia.IsWhiteSpace)
                {
                    // 在语法琐碎列表的较前位置中遇到了非空白语法琐碎，不继续累加缩进量。
                    if (!isContinuedWhiteSpace) continue;

                    indent += trivia.WhiteSpaceIndent;
                }
                else
                {
                    isContinuedWhiteSpace = false;
                }
            }
            return indent;
        }
    }

    internal const SyntaxKind FirstTokenWithWellKnownText = SyntaxKind.PlusToken;
    internal const SyntaxKind LastTokenWithWellKnownText = SyntaxKind.MultiLineCommentTrivia;
}
