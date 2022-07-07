﻿using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

internal partial class Lexer
{
    /// <summary>
    /// 存放语法标志的必要信息。
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
        /// 语法标志的文本表示。
        /// </summary>
        internal string? Text;
        /// <summary>
        /// 语法标志的值类别。
        /// </summary>
        internal SpecialType ValueKind;
        /// <summary>
        /// 语法标志的字符串类型值。
        /// </summary>
        internal string? StringValue;
        /// <summary>
        /// 语法标志的64位整数类型值。
        /// </summary>
        internal long LongValue;
        /// <summary>
        /// 语法标志的64位整数类型值。
        /// </summary>
        /// <remarks>
        /// 主要用于承载紧跟着一个负号（<c>-</c>）的<c>0x8000000000000000</c>。
        /// </remarks>
        internal ulong ULongValue;
        /// <summary>
        /// 语法标志的64位双精度浮点数类型值。
        /// </summary>
        internal double DoubleValue;
    }

}
