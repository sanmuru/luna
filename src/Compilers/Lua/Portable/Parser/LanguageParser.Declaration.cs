using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class LanguageParser
{
    private protected void ParseFunctionBody(out ParameterListSyntax parameters, out BlockSyntax block, out SyntaxToken endKeyword)
    {
        parameters = this.ParseParameterList();
        block = this.ParseBlock();
        endKeyword = this.EatToken(SyntaxKind.EndKeyword);
    }

    #region 形参
    private protected ParameterListSyntax ParseParameterList()
    {
        var openParen = this.EatToken(SyntaxKind.OpenParenToken);
        var parameters = this.ParseSeparatedSyntaxList(
            parseNodeFunc: _ => this.ParseParameter(),
            predicate: _ => true);
        var closeParen = this.EatToken(SyntaxKind.CloseParenToken);
        return this.syntaxFactory.ParameterList(openParen, parameters, closeParen);
    }

    private protected ParameterSyntax ParseParameter()
    {
        var identifier = this.CurrentTokenKind switch
        {
            SyntaxKind.IdentifierToken or
            SyntaxKind.DotDotDotToken => this.EatToken(),

            _ => LanguageParser.CreateMissingIdentifierToken(),
        };
        if (identifier.IsMissing)
            identifier = this.AddError(identifier, ErrorCode.ERR_IdentifierExpected);
        return this.syntaxFactory.Parameter(identifier);
    }
    #endregion

    #region 字段
    private protected FieldListSyntax ParseFieldList() =>
        this.ParseSeparatedSyntaxList(
            parseNodeFunc: _ => this.ParseField(),
            predicate: _ => true,
            list => this.syntaxFactory.FieldList(list))!;

    private protected FieldSyntax ParseField()
    {
        // 解析键值对表字段。
        if (this.CurrentTokenKind == SyntaxKind.OpenBracketToken)
            return this.ParseKeyValueField();
        // 解析名值对表字段。
        else if (this.CurrentTokenKind == SyntaxKind.IdentifierToken && this.PeekToken(1).Kind == SyntaxKind.EqualsToken)
            return this.ParseNameValueField();
        // 解析列表项表字段。
        else
            return this.syntaxFactory.ItemField(this.ParseFieldValue());
    }

    private NameValueFieldSyntax ParseNameValueField()
    {
        var name = this.ParseIdentifierName();
        var equals = this.EatToken(SyntaxKind.EqualsToken);
        var value = this.ParseFieldValue();
        return this.syntaxFactory.NameValueField(name, equals, value);
    }

    private protected KeyValueFieldSyntax ParseKeyValueField()
    {
        var openBracket = this.EatToken(SyntaxKind.OpenBracketToken);
        var key = this.ParseFieldKey();
        var closeBracket = this.EatToken(SyntaxKind.CloseBracketToken);
        var equals = this.EatToken(SyntaxKind.EqualsToken);
        var value = this.ParseFieldValue();
        return this.syntaxFactory.KeyValueField(openBracket, key, closeBracket, equals, value);
    }

    private protected ExpressionSyntax ParseFieldKey()
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

    private protected ExpressionSyntax ParseFieldValue()
    {
        ExpressionSyntax expr;
        if (this.IsPossibleExpression())
            expr = this.ParseExpression();
        else
            // 创建缺失的标识符名称语法用来表示缺失的表达式。
            expr = this.CreateMissingIdentifierName();

        // 跳过后方的标志直到字段结束。
        var skippedTokensTrivia = this.SkipTokens(token => token.Kind is not SyntaxKind.CommaToken or SyntaxKind.CloseBraceToken);
        if (skippedTokensTrivia is not null)
        {
            skippedTokensTrivia = this.AddError(skippedTokensTrivia, ErrorCode.ERR_InvalidExprTerm);
            expr = this.AddTrailingSkippedSyntax(expr, skippedTokensTrivia);
        }

        return expr;
    }
    #endregion

    #region 实参
    private protected InvocationArgumentsSyntax ParseInvocationArguments() => this.CurrentTokenKind switch
    {
        SyntaxKind.OpenParenToken => this.ParseArgumentList(),
        SyntaxKind.OpenBraceToken => this.ParseArgumentTable(),
        SyntaxKind.StringLiteralToken => this.ParseArgumentString(),
        _ => throw ExceptionUtilities.UnexpectedValue(this.CurrentTokenKind)
    };

    private protected ArgumentListSyntax ParseArgumentList()
    {
        var openParen = this.EatToken(SyntaxKind.OpenParenToken);
        var arguments = this.ParseSeparatedSyntaxList(
            parseNodeFunc: _ => this.ParseArgument(),
            predicate: _ => true);
        var closeParen = this.EatToken(SyntaxKind.CloseParenToken);
        return this.syntaxFactory.ArgumentList(openParen, arguments, closeParen);
    }

    private protected ArgumentTableSyntax ParseArgumentTable()
    {
        var table = this.ParseTableConstructorExpression();
        return this.syntaxFactory.ArgumentTable(table);
    }

    private protected ArgumentStringSyntax ParseArgumentString()
    {
        var stringLiteral = this.EatToken(SyntaxKind.StringLiteralToken);
        return this.syntaxFactory.ArgumentString(stringLiteral);
    }

    private protected ArgumentSyntax ParseArgument()
    {
        var expr = this.ParseExpression();
        return this.syntaxFactory.Argument(expr);
    }
    #endregion
}
