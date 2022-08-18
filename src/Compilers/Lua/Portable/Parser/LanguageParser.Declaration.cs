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
            return this._syntaxFactory.ItemField(this.ParseFieldValue());
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
        var equals = this.EatToken();
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

        // 跳过后方的标志直到右方括号。
        var skippedTokensTrivia = this.SkipTokens(token => token.Kind is not SyntaxKind.CloseBracketToken);
        if (skippedTokensTrivia is not null)
        {
            skippedTokensTrivia = this.AddError(skippedTokensTrivia, ErrorCode.ERR_InvalidExprTerm);
            expr = this.AddTrailingSkippedSyntax(expr, skippedTokensTrivia);
        }

        return expr;
    }

#if TESTING
    internal
#else
    private
#endif
        ExpressionSyntax ParseFieldValue()
    {
        ExpressionSyntax expr;
        if (this.IsPossibleExpression())
            expr = this.ParseExpression();
        else
            // 创建缺失的标识符名称语法用来表示缺失的表达式。
            expr = this.CreateMissingIdentifierName();

        // 跳过后方的标志和表达式直到字段结束。
        var skippedTokensTrivia = this.SkipTokensAndExpressions(token => token.Kind is not SyntaxKind.CommaToken or SyntaxKind.CloseBraceToken);
        if (skippedTokensTrivia is not null)
        {
            skippedTokensTrivia = this.AddError(skippedTokensTrivia, ErrorCode.ERR_InvalidFieldValueTerm);
            expr = this.AddTrailingSkippedSyntax(expr, skippedTokensTrivia);
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
