using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

internal partial class Lexer
{
    /// <summary>
    /// 存放语法标识的必要信息。
    /// </summary>
    internal struct TokenInfo
    {
        /// <summary>
        /// 直接语法类别。
        /// </summary>
        internal SyntaxKind Kind;
        /// <summary>
        /// 上下文语法类别。
        /// </summary>
        internal SyntaxKind ContextualKind;
        /// <summary>
        /// 语法标识的文本表示。
        /// </summary>
        internal string? Text;
        /// <summary>
        /// 语法标识的值类别。
        /// </summary>
        internal SpecialType ValueKind;
        internal bool HasIdentifierEscapeSequence;
        /// <summary>
        /// 语法标识的字符串类型值。
        /// </summary>
        internal string? StringValue;
        /// <summary>
        /// 语法标识的32位整数类型值。
        /// </summary>
        internal int IntValue;
        /// <summary>
        /// 语法标识的64位整数类型值。
        /// </summary>
        internal long LongValue;
        /// <summary>
        /// 语法标识的32位单精度浮点数类型值。
        /// </summary>
        internal float FloatValue;
        /// <summary>
        /// 语法标识的64位双精度浮点数类型值。
        /// </summary>
        internal double DoubleValue;
        /// <summary>
        /// 语法标识是否为逐字。
        /// </summary>
        internal bool IsVerbatim;
    }

}
