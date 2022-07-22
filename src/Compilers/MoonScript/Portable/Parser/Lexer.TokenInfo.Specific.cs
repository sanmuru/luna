using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

partial class Lexer
{
    partial struct TokenInfo
    {
        internal int InnerIndent;
        /// <summary>
        /// 语法标志的语法标志列表类型值。
        /// </summary>
        internal ImmutableArray<SyntaxToken> SyntaxTokenArrayValue;
    }
}
