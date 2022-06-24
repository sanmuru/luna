﻿using System.Diagnostics;

namespace SamLu.CodeAnalysis.Lua;

public static partial class SyntaxFacts
{
    /// <summary>
    /// 获取部分语法种类对应的文本表示。
    /// </summary>
    /// <param name="kind">要获取的语法种类。</param>
    /// <returns>表示<paramref name="kind"/>的文本。</returns>
    public static string GetText(SyntaxKind kind) =>
        kind switch
        {
            // 标点
            SyntaxKind.PlusToken => "+",
            SyntaxKind.MinusToken => "-",
            SyntaxKind.AsteriskToken => "*",
            SyntaxKind.SlashToken => "/",
            SyntaxKind.CaretToken => "^",
            SyntaxKind.PersentToken => "%",
            SyntaxKind.HashToken => "#",
            SyntaxKind.AmpersandToken => "&",
            SyntaxKind.TildeToken => "~",
            SyntaxKind.BarToken => "|",
            SyntaxKind.LessThanToken => "<",
            SyntaxKind.GreaterThanToken => ">",
            SyntaxKind.EqualsToken => "=",
            SyntaxKind.OpenParenToken => "(",
            SyntaxKind.CloseParenToken => ")",
            SyntaxKind.OpenBraceToken => "{",
            SyntaxKind.CloseBraceToken => "}",
            SyntaxKind.OpenBracketToken => "[",
            SyntaxKind.CloseBracketToken => "]",
            SyntaxKind.ColonToken => ":",
            SyntaxKind.SemicolonToken => ";",
            SyntaxKind.CommanToken => ",",
            SyntaxKind.DotToken => ".",
            SyntaxKind.LessThanLessThanToken => "<<",
            SyntaxKind.LessThanEqualsToken => "<=",
            SyntaxKind.GreaterThanGreaterThanToken => ">>",
            SyntaxKind.GreaterThanEqualsToken => ">=",
            SyntaxKind.SlashSlashToken => "//",
            SyntaxKind.EqualsEqualsToken => "==",
            SyntaxKind.TildeEqualsToken => "~=",
            SyntaxKind.ColonColonToken => "::",
            SyntaxKind.DotDotToken => "..",
            SyntaxKind.DotDotDotToken => "...",

            // 关键字
            SyntaxKind.AndKeyword => "and",
            SyntaxKind.BreakKeyword => "break",
            SyntaxKind.DoKeyword => "do",
            SyntaxKind.ElseKeyword => "else",
            SyntaxKind.ElseIfKeyword => "elseif",
            SyntaxKind.EndKeyword => "end",
            SyntaxKind.FalseKeyword => "false",
            SyntaxKind.ForKeyword => "for",
            SyntaxKind.FunctionKeyword => "function",
            SyntaxKind.GotoKeyword => "goto",
            SyntaxKind.IfKeyword => "if",
            SyntaxKind.InKeyword => "in",
            SyntaxKind.LocalKeyword => "local",
            SyntaxKind.NilKeyword => "nil",
            SyntaxKind.NotKeyword => "not",
            SyntaxKind.OrKeyword => "or",
            SyntaxKind.RepeatKeyword => "repeat",
            SyntaxKind.ReturnKeyword => "return",
            SyntaxKind.ThenKeyword => "then",
            SyntaxKind.TrueKeyword => "true",
            SyntaxKind.UntilKeyword => "until",
            SyntaxKind.WhileKeyword => "while",
            SyntaxKind.GlobalEnvironmentKeyword => "_G",
            SyntaxKind.EnvironmentKeyword => "_ENV",
            SyntaxKind.MetatableMetafield => "__metatable",
            SyntaxKind.AdditionMetamethod => "__add",
            SyntaxKind.SubtractionMetamethod => "__sub",
            SyntaxKind.MultiplicationMetamethod => "__mul",
            SyntaxKind.DivisionMetamethod => "__div",
            SyntaxKind.ModuloMetamethod => "__mod",
            SyntaxKind.ExponentiationMetamethod => "__pow",
            SyntaxKind.NegationMetamethod => "__unm",
            SyntaxKind.FloorDivisionMetamethod => "__idiv",
            SyntaxKind.BitwiseAndMetamethod => "__band",
            SyntaxKind.BitwiseOrMetamethod => "__bor",
            SyntaxKind.BitwiseExclusiveOrMetamethod => "__bxor",
            SyntaxKind.BitwiseNotMetamethod => "__bnot",
            SyntaxKind.BitwiseLeftShiftMetamethod => "__shl",
            SyntaxKind.BitwiseRightShiftMetamethod => "__shr",
            SyntaxKind.ConcatenationMetamethod => "__concat",
            SyntaxKind.LengthMetamethod => "__len",
            SyntaxKind.EqualMetamethod => "__eq",
            SyntaxKind.LessThanMetamethod => "__lt",
            SyntaxKind.LessEqualMetamethod => "__le",
            SyntaxKind.IndexingAccessMetamethod => "__index",
            SyntaxKind.CallMetamethod => "__call",
            SyntaxKind.PairsMetamethod => "__pairs",
            SyntaxKind.ToStringMetamethod => "__tostring",
            SyntaxKind.GarbageCollectionMetamethod => "__gc",
            SyntaxKind.ToBeClosedMetamethod => "__close",
            SyntaxKind.WeakModeMetafield => "__mode",
            SyntaxKind.NameMetafield => "__name",

            _ => string.Empty
        };

    /// <summary>
    /// 指定语法种类是否表示关键字。
    /// </summary>
    /// <param name="kind">要查询的语法种类。</param>
    /// <returns>若<paramref name="kind"/>表示关键字，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool IsKeywordKind(SyntaxKind kind) =>
        SyntaxKindFacts.IsReservedKeyword(kind) || SyntaxKindFacts.IsContextualKeyword(kind);

    /// <summary>
    /// 获取所有关键字语法种类。
    /// </summary>
    /// <returns>所有关键字语法种类。</returns>
    public static IEnumerable<SyntaxKind> GetKeywordKinds() =>
        SyntaxKindFacts.GetReservedKeywordKinds().Concat(SyntaxKindFacts.GetContextualKeywordKinds());

    #region 保留关键字
    /// <summary>
    /// 获取所有保留关键字语法种类。
    /// </summary>
    /// <returns>所有保留关键字语法种类。</returns>
    public static IEnumerable<SyntaxKind> GetReservedKeywordKinds()
    {
        for (int i = (int)SyntaxKind.AndKeyword; i <= (int)SyntaxKind.WhileKeyword; i++)
            yield return (SyntaxKind)i;
    }

    /// <summary>
    /// 指定语法种类是否表示保留关键字。
    /// </summary>
    /// <param name="kind">要查询的语法种类。</param>
    /// <returns>若<paramref name="kind"/>表示保留关键字，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool IsReservedKeyword(SyntaxKind kind) =>
        kind switch
        {
            >= SyntaxKind.AndKeyword and <= SyntaxKind.WhileKeyword => true,

            _ => false
        };

    public static SyntaxKind GetKeywordKind(string text) =>
        text switch
        {
            "and" => SyntaxKind.AndKeyword,
            "break" => SyntaxKind.BreakKeyword,
            "do" => SyntaxKind.DoKeyword,
            "else" => SyntaxKind.ElseKeyword,
            "elseif" => SyntaxKind.ElseIfKeyword,
            "end" => SyntaxKind.EndKeyword,
            "false" => SyntaxKind.FalseKeyword,
            "for" => SyntaxKind.ForKeyword,
            "function" => SyntaxKind.FunctionKeyword,
            "goto" => SyntaxKind.GotoKeyword,
            "if" => SyntaxKind.IfKeyword,
            "in" => SyntaxKind.InKeyword,
            "local" => SyntaxKind.LocalKeyword,
            "nil" => SyntaxKind.NilKeyword,
            "not" => SyntaxKind.NotKeyword,
            "or" => SyntaxKind.OrKeyword,
            "repeat" => SyntaxKind.RepeatKeyword,
            "return" => SyntaxKind.ReturnKeyword,
            "then" => SyntaxKind.ThenKeyword,
            "true" => SyntaxKind.TrueKeyword,
            "until" => SyntaxKind.UntilKeyword,
            "while" => SyntaxKind.WhileKeyword,

            _ => SyntaxKind.None
        };
    #endregion

    #region 上下文关键字
    /// <summary>
    /// 获取所有上下文关键字语法种类。
    /// </summary>
    /// <returns>所有上下文关键字语法种类。</returns>
    public static IEnumerable<SyntaxKind> GetContextualKeywordKinds()
    {
        // 上下文关键词
        for (int i = (int)SyntaxKind.GlobalEnvironmentKeyword; i <= (int)SyntaxKind.EnvironmentKeyword; i++)
            yield return (SyntaxKind)i;

        // 元字段和元方法
        for (int i = (int)SyntaxKind.MetatableMetafield; i <= (int)SyntaxKind.NameMetafield; i++)
            yield return (SyntaxKind)i;
    }

    /// <summary>
    /// 指定语法种类是否表示上下文关键字。
    /// </summary>
    /// <param name="kind">要查询的语法种类。</param>
    /// <returns>若<paramref name="kind"/>表示上下文关键字，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool IsContextualKeyword(SyntaxKind kind) =>
        // 元字段和元方法
        SyntaxKindFacts.IsMetafield(kind) ||

        // 上下文关键词
        kind switch
        {
            >= SyntaxKind.GlobalEnvironmentKeyword and <= SyntaxKind.EnvironmentKeyword => true,

            _ => false
        };

    /// <summary>
    /// 指定语法种类是否表示元字段和元方法。
    /// </summary>
    /// <param name="kind">要查询的语法种类。</param>
    /// <returns>若<paramref name="kind"/>表示元字段和元方法，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool IsMetafield(SyntaxKind kind) =>
        kind switch
        {
            >= SyntaxKind.MetatableMetafield and <= SyntaxKind.NameMetafield => true,

            _ => false
        };

    public static SyntaxKind GetContextualKeywordKind(string text) =>
        text switch
        {
            // 上下文关键字
            "_G" => SyntaxKind.GlobalEnvironmentKeyword,
            "_ENV" => SyntaxKind.EnvironmentKeyword,

            // 元字段和元方法
            _ => SyntaxKindFacts.GetMetafieldKind(text)
        };
    #endregion

    #region 标点
    /// <summary>
    /// 获取所有标点语法种类。
    /// </summary>
    /// <returns>所有标点语法种类。</returns>
    public static IEnumerable<SyntaxKind> GetPunctuationKinds()
    {
        for (int i = (int)SyntaxKind.PlusToken; i <= (int)SyntaxKind.DotDotDotToken; i++)
            yield return (SyntaxKind)i;
    }

    /// <summary>
    /// 指定语法种类是否表示标点。
    /// </summary>
    /// <param name="kind">要查询的语法种类。</param>
    /// <returns>若<paramref name="kind"/>表示标点，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool IsPunctuation(SyntaxKind kind) =>
        kind switch
        {
            >= SyntaxKind.PlusToken and <= SyntaxKind.DotDotDotToken => true,

            _ => false
        };
    #endregion

    /// <summary>
    /// 指定语法种类是否表示标点或关键字（包含文件结尾标识）。
    /// </summary>
    /// <param name="kind">要查询的语法种类。</param>
    /// <returns>若<paramref name="kind"/>表示标点或关键字，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool IsPunctuationOrKeyword(SyntaxKind kind) =>
        kind == SyntaxKind.EndOfFileToken ||
        SyntaxKindFacts.IsPunctuation(kind) ||
        SyntaxKindFacts.IsKeywordKind(kind);

    /// <summary>
    /// 指定语法种类是否表示字面量。
    /// </summary>
    /// <param name="kind">要查询的语法种类。</param>
    /// <returns>若<paramref name="kind"/>表示字面量，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    internal static bool IsLiteral(SyntaxKind kind) =>
        kind switch
        {
            >= SyntaxKind.IdentifierToken and <= SyntaxKind.MultiLineRawStringLiteralToken => true,

            _ => false
        };

    /// <summary>
    /// 指定语法种类是否表示标志。
    /// </summary>
    /// <param name="kind">要查询的语法种类。</param>
    /// <returns>若<paramref name="kind"/>表示标志，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool IsAnyToken(SyntaxKind kind)
    {
        Debug.Assert(Enum.IsDefined(typeof(SyntaxKind), kind));
        return kind switch
        {
            >= SyntaxKind.PlusToken and < SyntaxKind.EndOfLineTrivia => true,

            _ => false
        };
    }

    /// <summary>
    /// 指定语法种类是否表示琐碎内容。
    /// </summary>
    /// <param name="kind">要查询的语法种类。</param>
    /// <returns>若<paramref name="kind"/>表示琐碎内容，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool IsTrivia(SyntaxKind kind) =>
        kind switch
        {
            >= SyntaxKind.EndOfLineTrivia and <= SyntaxKind.SkippedTokensTrivia => true,

            _ => false
        };

    /// <summary>
    /// 指定语法种类是否表示名称。
    /// </summary>
    /// <param name="kind">要查询的语法种类。</param>
    /// <returns>若<paramref name="kind"/>表示名称，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool IsName(SyntaxKind kind) =>
        kind switch
        {
            SyntaxKind.IdentifierName or
            SyntaxKind.GenericName or
            SyntaxKind.QualifiedName or
            SyntaxKind.AliasQualifiedName => true,

            _ => false
        };

    //public static bool IsGlobalMemberDeclaration(SyntaxKind kind);
    //public static bool IsTypeDeclaration(SyntaxKind kind)
    //public static bool IsNamespaceMemberDeclaration(SyntaxKind kind)

    public static bool IsAnyUnaryExpression(SyntaxKind token) => SyntaxKindFacts.IsPrefixUnaryExpression(token);

    public static bool IsPrefixUnaryExpression(SyntaxKind token) =>
        SyntaxKindFacts.GetPrefixUnaryExpression(token) != SyntaxKind.None;

    public static bool IsPrefixUnaryExpressionOperatorToken(SyntaxKind token) => GetPrefixUnaryExpression(token) != SyntaxKind.None;

    public static SyntaxKind GetPrefixUnaryExpression(SyntaxKind token) =>
        token switch
        {
            SyntaxKind.PlusToken => SyntaxKind.UnaryMinusExpression,
            SyntaxKind.NotKeyword => SyntaxKind.LogicalNotExpression,
            SyntaxKind.HashToken => SyntaxKind.LengthExpression,
            SyntaxKind.TildeToken => SyntaxKind.BitwiseNotExpression,

            _ => SyntaxKind.None
        };

    public static bool IsPrimaryFunction(SyntaxKind keyword) =>
        SyntaxKindFacts.GetPrimaryFunction(keyword) != SyntaxKind.None;

    public static SyntaxKind GetPrimaryFunction(SyntaxKind keyword) =>
        keyword switch
        {
            _ => SyntaxKind.None
        };

    public static bool IsLiteralExpression(SyntaxKind token) =>
        SyntaxKindFacts.GetLiteralExpression(token) != SyntaxKind.None;

    public static SyntaxKind GetLiteralExpression(SyntaxKind token) =>
        token switch
        {
            SyntaxKind.NilKeyword => SyntaxKind.NilLiteralExpression,
            SyntaxKind.FalseKeyword => SyntaxKind.FalseLiteralExpression,
            SyntaxKind.TrueKeyword => SyntaxKind.TrueLiteralExpression,
            SyntaxKind.NumericLiteralToken => SyntaxKind.NumericLiteralExpression,
            SyntaxKind.StringLiteralToken or
            SyntaxKind.SingleLineRawStringLiteralToken or
            SyntaxKind.MultiLineRawStringLiteralToken => SyntaxKind.StringLiteralExpression,
            SyntaxKind.DotDotDotToken => SyntaxKind.VariousArgumentsExpression,

            _ => SyntaxKind.None
        };

    public static bool IsBinaryExpression(SyntaxKind token) =>
        SyntaxKindFacts.GetBinaryExpression(token) != SyntaxKind.None;

    public static SyntaxKind GetBinaryExpression(SyntaxKind token) =>
        token switch
        {
            SyntaxKind.PlusToken => SyntaxKind.AdditionExpression,
            SyntaxKind.MinusToken => SyntaxKind.SubtractionExpression,
            SyntaxKind.AsteriskToken => SyntaxKind.MultiplicationExpression,
            SyntaxKind.SlashToken => SyntaxKind.DivisionExpression,
            SyntaxKind.SlashSlashToken => SyntaxKind.FloorDivisionExpression,
            SyntaxKind.CaretToken => SyntaxKind.ExponentiationExpression,
            SyntaxKind.PersentToken => SyntaxKind.ModuloExpression,
            SyntaxKind.AmpersandToken => SyntaxKind.BitwiseAndExpression,
            SyntaxKind.TildeToken => SyntaxKind.BitwiseExclusiveOrExpression,
            SyntaxKind.BarToken => SyntaxKind.BitwiseOrExpression,
            SyntaxKind.GreaterThanGreaterThanToken => SyntaxKind.BitwiseRightShiftExpression,
            SyntaxKind.LessThanLessThanToken => SyntaxKind.BitwiseLeftShiftExpression,
            SyntaxKind.DotDotToken => SyntaxKind.ConcatenationExpression,
            SyntaxKind.LessThanToken => SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanEqualsToken => SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.GreaterThanToken => SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanEqualsToken => SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.EqualsEqualsToken => SyntaxKind.EqualExpression,
            SyntaxKind.TildeEqualsToken => SyntaxKind.NotEqualExpression,
            SyntaxKind.AndKeyword => SyntaxKind.AndExpression,
            SyntaxKind.OrKeyword => SyntaxKind.OrExpression,

            _ => SyntaxKind.None
        };

    public static bool IsAssignmentExpression(SyntaxKind kind) =>
        kind switch
        {
            SyntaxKind.SimpleAssignmentExpression => true,

            _ => false
        };

    public static bool IsAssignmentExpressionOperatorToken(SyntaxKind token) =>
        SyntaxKindFacts.GetAssignmentExpression(token) != SyntaxKind.None;

    public static SyntaxKind GetAssignmentExpression(SyntaxKind token) =>
        token switch
        {
            SyntaxKind.EqualsToken => SyntaxKind.SimpleAssignmentExpression,

            _ => SyntaxKind.None
        };

    public static SyntaxKind GetMetafieldKind(string metafieldname) =>
        metafieldname switch
        {
            "__metatable" => SyntaxKind.MetatableMetafield,
            "__add" => SyntaxKind.AdditionMetamethod,
            "__sub" => SyntaxKind.SubtractionMetamethod,
            "__mul" => SyntaxKind.MultiplicationMetamethod,
            "__div" => SyntaxKind.DivisionMetamethod,
            "__mod" => SyntaxKind.ModuloMetamethod,
            "__pow" => SyntaxKind.ExponentiationMetamethod,
            "__unm" => SyntaxKind.NegationMetamethod,
            "__idiv" => SyntaxKind.FloorDivisionMetamethod,
            "__band" => SyntaxKind.BitwiseAndMetamethod,
            "__bor" => SyntaxKind.BitwiseOrMetamethod,
            "__bxor" => SyntaxKind.BitwiseExclusiveOrMetamethod,
            "__bnot" => SyntaxKind.BitwiseNotMetamethod,
            "__shl" => SyntaxKind.BitwiseLeftShiftMetamethod,
            "__shr" => SyntaxKind.BitwiseRightShiftMetamethod,
            "__concat" => SyntaxKind.ConcatenationMetamethod,
            "__len" => SyntaxKind.LengthMetamethod,
            "__eq" => SyntaxKind.EqualMetamethod,
            "__lt" => SyntaxKind.LessThanMetamethod,
            "__le" => SyntaxKind.LessEqualMetamethod,
            "__index" => SyntaxKind.IndexingAccessMetamethod,
            "__call" => SyntaxKind.CallMetamethod,
            "__pairs" => SyntaxKind.PairsMetamethod,
            "__tostring" => SyntaxKind.ToStringMetamethod,
            "__gc" => SyntaxKind.GarbageCollectionMetamethod,
            "__close" => SyntaxKind.ToBeClosedMetamethod,
            "__mode" => SyntaxKind.WeakModeMetafield,
            "__name" => SyntaxKind.NameMetafield,

            _ => SyntaxKind.None
        };

    public static SyntaxKind GetOperatorKind(string operatorMetafieldName) =>
        operatorMetafieldName switch
        {
            "__add" => SyntaxKind.PlusToken,
            "__sub" => SyntaxKind.MinusToken,
            "__mul" => SyntaxKind.AsteriskToken,
            "__div" => SyntaxKind.SlashToken,
            "__mod" => SyntaxKind.PersentToken,
            "__pow" => SyntaxKind.CaretToken,
            "__unm" => SyntaxKind.MinusToken,
            "__idiv" => SyntaxKind.SlashSlashToken,
            "__band" => SyntaxKind.AmpersandToken,
            "__bor" => SyntaxKind.BarToken,
            "__bxor" => SyntaxKind.TildeToken,
            "__bnot" => SyntaxKind.TildeToken,
            "__shl" => SyntaxKind.LessThanLessThanToken,
            "__shr" => SyntaxKind.GreaterThanGreaterThanToken,
            "__concat" => SyntaxKind.DotDotToken,
            "__len" => SyntaxKind.HashToken,
            "__eq" => SyntaxKind.EqualsToken,
            "__lt" => SyntaxKind.LessThanToken,
            "__le" => SyntaxKind.LessThanEqualsToken,

            _ => SyntaxKind.None
        };
}
