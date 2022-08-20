using System.Diagnostics;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class LanguageParser
{
#if TESTING
    internal
#else
    private
#endif
        void ParseFunctionBody(out ParameterListSyntax parameters, out BlockSyntax block, out SyntaxToken endKeyword)
    {
        parameters = this.ParseParameterList();
        block = this.ParseBlock();
        endKeyword = this.EatToken(SyntaxKind.EndKeyword);
    }

    #region 形参
#if TESTING
    internal
#else
    private
#endif
        ParameterListSyntax ParseParameterList()
    {
        var openParen = this.EatToken(SyntaxKind.OpenParenToken);
        var parameters = this.ParseSeparatedSyntaxList(
            parseNodeFunc: _ => this.ParseParameter(),
            predicate: _ => true);
        var closeParen = this.EatToken(SyntaxKind.CloseParenToken);
        return this._syntaxFactory.ParameterList(openParen, parameters, closeParen);
    }

#if TESTING
    internal
#else
    private
#endif
        ParameterSyntax ParseParameter()
    {
        var identifier = this.CurrentTokenKind switch
        {
            SyntaxKind.IdentifierToken or
            SyntaxKind.DotDotDotToken => this.EatToken(),

            _ => LanguageParser.CreateMissingIdentifierToken(),
        };
        if (identifier.IsMissing)
            identifier = this.AddError(identifier, ErrorCode.ERR_IdentifierExpected);
        return this._syntaxFactory.Parameter(identifier);
    }
    #endregion

    #region 字段
#if TESTING
    internal
#else
    private
#endif
        FieldListSyntax ParseFieldList() =>
        this.ParseSeparatedSyntaxList(
            parseNodeFunc: _ => this.ParseField(),
            predicate: _ => true,
            list => this._syntaxFactory.FieldList(list))!;

#if TESTING
    internal
#else
    private
#endif
        FieldSyntax ParseField()
    {
        // 解析键值对表字段。
        if (this.CurrentTokenKind == SyntaxKind.OpenBracketToken ||
            (SyntaxFacts.IsLiteralToken(this.CurrentTokenKind) && this.PeekToken(1).Kind == SyntaxKind.EqualsToken)) // 错误使用常量作为键。
            return this.ParseKeyValueField();
        // 解析名值对表字段。
        else if (this.CurrentTokenKind == SyntaxKind.EqualsToken || // 错误遗失标识符。
            (this.CurrentTokenKind == SyntaxKind.IdentifierToken && this.PeekToken(1).Kind == SyntaxKind.EqualsToken))
            return this.ParseNameValueField();
        // 解析列表项表字段。
        else
        {
            if (this.CurrentTokenKind == SyntaxKind.EndOfFileToken)
                return this._syntaxFactory.ItemField(this.CreateMissingIdentifierName());
            else
                return this._syntaxFactory.ItemField(this.ParseFieldValue());
        }
    }

#if TESTING
    internal
#else
    private
#endif
        NameValueFieldSyntax ParseNameValueField()
    {
        var name = this.ParseIdentifierName();
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.EqualsToken);
        var equals = this.EatToken(SyntaxKind.EqualsToken);
        var value = this.ParseFieldValue();
        return this._syntaxFactory.NameValueField(name, equals, value);
    }

#if TESTING
    internal
#else
    private
#endif
        KeyValueFieldSyntax ParseKeyValueField()
    {
        var openBracket = this.EatToken(SyntaxKind.OpenBracketToken);
        var key = this.ParseFieldKey();
        var closeBracket = this.EatToken(SyntaxKind.CloseBracketToken);
        var equals = this.EatToken(SyntaxKind.EqualsToken);
        var value = this.ParseFieldValue();
        return this._syntaxFactory.KeyValueField(openBracket, key, closeBracket, equals, value);
    }

#if TESTING
    internal
#else
    private
#endif
        ExpressionSyntax ParseFieldKey()
    {
        var expr = this.ParseExpression();

        // 跳过后方的标志和表达式直到等于符号或右方括号。
        var skippedSyntax = this.SkipTokensAndExpressions(
            token => token.Kind is not (SyntaxKind.EqualsToken or SyntaxKind.CloseBracketToken or SyntaxKind.EndOfFileToken),
            new FieldKeySkippedNodesVisitor(this));
        if (skippedSyntax is not null)
            expr = this.AddTrailingSkippedSyntax(expr, skippedSyntax);

        return expr;
    }

    /// <summary>
    /// 处理字段键表达式后方需要跳过的语法标志和语法节点的访问器。
    /// </summary>
    private sealed class FieldKeySkippedNodesVisitor : LuaSyntaxVisitor<LuaSyntaxNode>
    {
        private readonly LanguageParser _parser;

        public FieldKeySkippedNodesVisitor(LanguageParser parser) => this._parser = parser;

        /// <summary>
        /// 处理语法节点。
        /// </summary>
        /// <param name="node">要处理的语法节点。</param>
        /// <returns>处理后的<paramref name="node"/>。</returns>
        public override LuaSyntaxNode? Visit(LuaSyntaxNode? node)
        {
            if (node is null or SyntaxToken)
                return base.Visit(node);
            else
                return this.DefaultVisit(node);
        }

        /// <summary>
        /// 处理语法标志，向语法标志添加<see cref="ErrorCode.ERR_InvalidExprTerm"/>错误。
        /// </summary>
        /// <param name="token">要处理的语法标志。</param>
        /// <returns>处理后的<paramref name="token"/>。</returns>
        public override LuaSyntaxNode? VisitToken(SyntaxToken token) =>
            this._parser.AddError(token, ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(token.Kind));

        /// <summary>
        /// 处理所有语法节点。
        /// </summary>
        /// <remarks>若<paramref name="node"/>不是表达式语法，则抛出异常。</remarks>
        /// <param name="node">要处理的语法节点。</param>
        /// <returns>处理后的<paramref name="node"/>。</returns>
        /// <exception cref="ExceptionUtilities.Unreachable">当<paramref name="node"/>不是表达式语法时，这种情况不应发生。</exception>
        protected override LuaSyntaxNode? DefaultVisit(LuaSyntaxNode node) =>
            node is ExpressionSyntax ? node : throw ExceptionUtilities.Unreachable;
    }

#if TESTING
    internal
#else
    private
#endif
        ExpressionSyntax ParseFieldValue()
    {
        ExpressionSyntax? expr = null;
        if (this.IsPossibleExpression())
            expr = this.ParseExpressionCore();

        // 跳过后方的标志和表达式直到字段结束。
        var skippedSyntax = this.SkipTokensAndExpressions(token => token.Kind is not (SyntaxKind.CommaToken or SyntaxKind.CloseBraceToken or SyntaxKind.EndOfFileToken));
        if (skippedSyntax is null) // 后方没有需要跳过的标志和表达式。
            expr ??= this.ReportMissingExpression(this.CreateMissingIdentifierName());
        else
        {
            skippedSyntax = this.AddError(skippedSyntax, ErrorCode.ERR_InvalidFieldValueTerm);
            expr = this.AddTrailingSkippedSyntax(
                expr ?? this.ReportMissingExpression(this.CreateMissingIdentifierName()),
                skippedSyntax);
        }
        return expr;
    }
    #endregion

    #region 实参
#if TESTING
    internal
#else
    private
#endif
        InvocationArgumentsSyntax ParseInvocationArguments() =>
        this.CurrentTokenKind switch
        {
            SyntaxKind.OpenParenToken => this.ParseArgumentList(),
            SyntaxKind.OpenBraceToken => this.ParseArgumentTable(),
            SyntaxKind.StringLiteralToken => this.ParseArgumentString(),
            _ => throw ExceptionUtilities.UnexpectedValue(this.CurrentTokenKind)
        };

#if TESTING
    internal
#else
    private
#endif
        ArgumentListSyntax ParseArgumentList()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.OpenParenToken);
        var openParen = this.EatToken(SyntaxKind.OpenParenToken);
        var arguments = this.ParseSeparatedSyntaxList(
            parseNodeFunc: _ => this.ParseArgument(),
            predicate: _ => true);
        var closeParen = this.EatToken(SyntaxKind.CloseParenToken);
        return this._syntaxFactory.ArgumentList(openParen, arguments, closeParen);
    }

#if TESTING
    internal
#else
    private
#endif
        ArgumentTableSyntax ParseArgumentTable()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.OpenBraceToken);
        var table = this.ParseTableConstructorExpression();
        return this._syntaxFactory.ArgumentTable(table);
    }

#if TESTING
    internal
#else
    private
#endif
        ArgumentStringSyntax ParseArgumentString()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.StringLiteralToken);
        var stringLiteral = this.EatToken(SyntaxKind.StringLiteralToken);
        return this._syntaxFactory.ArgumentString(stringLiteral);
    }

#if TESTING
    internal
#else
    private
#endif
        ArgumentSyntax ParseArgument()
    {
        var expr = this.ParseExpression();
        return this._syntaxFactory.Argument(expr);
    }
    #endregion
}
