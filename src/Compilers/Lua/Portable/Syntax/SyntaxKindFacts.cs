using System.Diagnostics;

namespace SamLu.CodeAnalysis.Lua;

public static partial class SyntaxKindFacts
{
    public static bool IsKeywordKind(SyntaxKind kind) =>
        SyntaxKindFacts.IsReservedKeyword(kind) || SyntaxKindFacts.IsContextualKeyword(kind);

    public static IEnumerable<SyntaxKind> GetKeywordKinds() => SyntaxKindFacts.GetReservedKeywordKinds().Concat(SyntaxKindFacts.GetContextualKeywordKinds());

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

    public static bool IsAnyUnaryExpression(SyntaxKind token) => SyntaxKindFacts.IsPrefixUnaryExpression(token) || SyntaxKindFacts.IsPostfixUnaryExpression(token);

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

    public static bool IsPostfixUnaryExpression(SyntaxKind token) =>
        SyntaxKindFacts.GetPostfixUnaryExpression(token) != SyntaxKind.None;

    public static bool IsPostfixUnaryExpressionOperatorToken(SyntaxKind token) => GetPostfixUnaryExpression(token) != SyntaxKind.None;

    public static SyntaxKind GetPostfixUnaryExpression(SyntaxKind token) =>
        token switch
        {
            _ => SyntaxKind.None
        };
}
