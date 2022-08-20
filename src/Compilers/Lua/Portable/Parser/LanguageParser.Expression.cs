using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class LanguageParser
{
#if TESTING
    internal
#else
    private
#endif
        ExpressionListSyntax ParseExpressionList() =>
        this.ParseExpressionListOpt() ??
            // 创建一个缺失的标识符名称语法来组成表达式列表，并报告错误信息。
            this.CreateMissingExpressionList();

#if TESTING
    internal
#else
    private
#endif
        ExpressionListSyntax? ParseExpressionListOpt()
    {
        if (this.CurrentTokenKind != SyntaxKind.CommaToken && !this.IsPossibleExpression()) return null;

        return this.ParseSeparatedSyntaxList(
            index =>
            {
                if (this.IsPossibleExpression())
                    return this.ParseExpression();
                else
                {
                    // 第一项缺失的情况：
                    if (index == 0)
                        Debug.Assert(this.CurrentTokenKind == SyntaxKind.CommaToken);
                    return this.CreateMissingIdentifierName();
                }
            },
            _ => true,
            list => list.Count == 0 ? null :
                this._syntaxFactory.ExpressionList(list));
    }

#if TESTING
    internal
#else
    private
#endif
        ExpressionListSyntax CreateMissingExpressionList()
    {
        var missing = this._syntaxFactory.ExpressionList(
            new(SyntaxList.List(
                this.CreateMissingIdentifierName()
            ))
        );
        return this.AddError(missing, ErrorCode.ERR_InvalidExprTerm);
    }

#if TESTING
    internal
#else
    private
#endif
        bool IsPossibleExpression() => this.CurrentTokenKind is
        SyntaxKind.NilKeyword or
        SyntaxKind.FalseKeyword or
        SyntaxKind.TrueKeyword or
        SyntaxKind.NumericLiteralToken or
        SyntaxKind.StringLiteralToken or
        SyntaxKind.MultiLineRawStringLiteralToken or
        SyntaxKind.DotDotDotToken or
        SyntaxKind.OpenParenToken or
        SyntaxKind.FunctionKeyword or
        SyntaxKind.OpenBraceToken or
        SyntaxKind.MinusToken or
        SyntaxKind.NotKeyword or
        SyntaxKind.HashToken or
        SyntaxKind.TildeToken or
        SyntaxKind.IdentifierToken;

#if TESTING
    internal
#else
    private
#endif
        ExpressionSyntax ParseExpression(bool reportError = true)
    {
        if (this.IsPossibleExpression())
            return this.ParseExpressionCore();
        else
        {
            var missing = this.CreateMissingIdentifierName();
            if (reportError)
                return this.ReportMissingExpression(missing);
            else
                return missing;
        }
    }

    /// <summary>
    /// 报告缺失的表达式错误。
    /// </summary>
    /// <typeparam name="TExpression">表达式节点的类型。</typeparam>
    /// <param name="expr">要添加错误信息的表达式节点。</param>
    /// <returns>添加错误信息后的<paramref name="expr"/>。</returns>
    private TExpression ReportMissingExpression<TExpression>(TExpression expr) where TExpression : ExpressionSyntax
    {
        var kind = this.CurrentTokenKind;
        if (kind == SyntaxKind.EndOfFileToken)
            return this.AddError(expr, ErrorCode.ERR_ExpressionExpected);
        else
            return this.AddError(expr, ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(kind));
    }

    private ExpressionSyntax ParseExpressionCore()
    {
        Debug.Assert(this.IsPossibleExpression(), "必须先检查当前标志是否可能为表达式的开始，请使用ParseExpression。");

        ExpressionSyntax expr;
        if (SyntaxFacts.IsUnaryExpressionOperatorToken(this.CurrentTokenKind))
            expr = this.ParseExpressionWithOperator();
        else
        {
            expr = this.ParseExpressionWithoutOperator();
            if (SyntaxFacts.IsBinaryExpressionOperatorToken(this.CurrentTokenKind))
                expr = this.ParseExpressionWithOperator(expr);
        }

        return expr;
    }

#if TESTING
    internal
#else
    private
#endif
        ExpressionSyntax ParseExpressionWithoutOperator()
    {
        ExpressionSyntax expr = this.CurrentTokenKind switch
        {
            // 字面量
            SyntaxKind.NilKeyword =>
                this.ParseLiteralExpression(SyntaxKind.NilLiteralExpression
#if DEBUG
                    , SyntaxKind.NilKeyword
#endif
                    ),
            SyntaxKind.FalseKeyword =>
                this.ParseLiteralExpression(SyntaxKind.FalseLiteralExpression
#if DEBUG
                    , SyntaxKind.FalseKeyword
#endif
                    ),
            SyntaxKind.TrueKeyword =>
                this.ParseLiteralExpression(SyntaxKind.TrueLiteralExpression
#if DEBUG
                    , SyntaxKind.TrueKeyword
#endif
                    ),
            SyntaxKind.NumericLiteralToken =>
                this.ParseLiteralExpression(SyntaxKind.NumericLiteralExpression
#if DEBUG
                    , SyntaxKind.NumericLiteralToken
#endif
                    ),
            SyntaxKind.StringLiteralToken =>
                this.ParseLiteralExpression(SyntaxKind.StringLiteralExpression
#if DEBUG
                    , SyntaxKind.StringLiteralToken
#endif
                    ),
            SyntaxKind.MultiLineRawStringLiteralToken =>
                this.ParseLiteralExpression(SyntaxKind.StringLiteralExpression
#if DEBUG
                    , SyntaxKind.MultiLineRawStringLiteralToken
#endif
                    ),
            SyntaxKind.DotDotDotToken =>
                this.ParseLiteralExpression(SyntaxKind.VariousArgumentsExpression
#if DEBUG
                    , SyntaxKind.DotDotDotToken
#endif
                    ),

            SyntaxKind.MinusToken or
            SyntaxKind.NotKeyword or
            SyntaxKind.HashToken or
            SyntaxKind.TildeToken =>
                this.ParseExpressionWithOperator(),

            SyntaxKind.OpenParenToken =>
                this.ParseParenthesizedExpression(),

            SyntaxKind.FunctionKeyword =>
                this.ParseFunctionDefinitionExpression(),

            SyntaxKind.OpenBraceToken =>
                this.ParseTableConstructorExpression(),

            SyntaxKind.IdentifierToken =>
                this.ParseIdentifierName(),

            _ =>
                throw ExceptionUtilities.Unreachable
        };

        int lastTokenPosition = -1;
        while (IsMakingProgress(ref lastTokenPosition))
        {
            switch (this.CurrentTokenKind)
            {
                case SyntaxKind.DotToken:
                    expr = this.ParseSimpleMemberAccessExpressionSyntax(expr);
                    break;
                case SyntaxKind.OpenBracketToken:
                    expr = this.ParseIndexMemberAccessExpressionSyntax(expr);
                    break;

                case SyntaxKind.ColonToken:
                    expr = this.ParseImplicitSelfParameterInvocationExpression(expr);
                    break;

                default:
                    if (this.IsPossibleInvocationArguments())
                    {
                        expr = this.ParseInvocationExpressionSyntax(expr);
                        break;
                    }
                    return expr;
            }
        }

        throw ExceptionUtilities.Unreachable;
    }

#if TESTING
    internal
#else
    private
#endif
        ExpressionSyntax ParseExpressionWithOperator(ExpressionSyntax? first = null)
    {
        ExpressionWithOperatorParser innerParser;
        if (first is null)
            innerParser = new(this);
        else
            innerParser = new(this, first);

        return innerParser.ParseExpressionWithOperator();
    }

#if TESTING
    internal
#else
    private
#endif
        LiteralExpressionSyntax ParseLiteralExpression(SyntaxKind kind
#if DEBUG
        , SyntaxKind currentTokenKind
#endif
        )
    {
#if DEBUG
        Debug.Assert(this.CurrentTokenKind == currentTokenKind);
#endif

        return this._syntaxFactory.LiteralExpression(kind, this.EatToken());
    }

#if TESTING
    internal
#else
    private
#endif
        ParenthesizedExpressionSyntax ParseParenthesizedExpression()
    {
        var openParen = this.EatToken(SyntaxKind.OpenParenToken);
        var expression = this.ParseExpression();
        var closeParen = this.EatToken(SyntaxKind.CloseParenToken);
        return this._syntaxFactory.ParenthesizedExpression(openParen, expression, closeParen);
    }

#if TESTING
    internal
#else
    private
#endif
        FunctionDefinitionExpressionSyntax ParseFunctionDefinitionExpression()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.FunctionKeyword);

        var function = this.EatToken(SyntaxKind.FunctionKeyword);
        this.ParseFunctionBody(out var parameters, out var block, out var end);
        return this._syntaxFactory.FunctionDefinitionExpression(function, parameters, block, end);
    }

#if TESTING
    internal
#else
    private
#endif
        TableConstructorExpressionSyntax ParseTableConstructorExpression()
    {
        var openBrace = this.EatToken(SyntaxKind.OpenBraceToken);
        var field = this.ParseFieldList();
        var closeBrace = this.EatToken(SyntaxKind.CloseBraceToken);
        return this._syntaxFactory.TableConstructorExpression(openBrace, field, closeBrace);
    }

#if TESTING
    internal
#else
    private
#endif
        SimpleMemberAccessExpressionSyntax ParseSimpleMemberAccessExpressionSyntax(ExpressionSyntax self)
    {
        var dot = this.EatToken(SyntaxKind.DotToken);
        var member = this.ParseIdentifierName();
        return this._syntaxFactory.SimpleMemberAccessExpression(self, dot, member);
    }

#if TESTING
    internal
#else
    private
#endif
        IndexMemberAccessExpressionSyntax ParseIndexMemberAccessExpressionSyntax(ExpressionSyntax self)
    {
        var openBracket = this.EatToken(SyntaxKind.OpenBracketToken);
        var member = this.ParseExpression();
        var closeBracket = this.EatToken(SyntaxKind.CloseBracketToken);
        return this._syntaxFactory.IndexMemberAccessExpression(self, openBracket, member, closeBracket);
    }

#if TESTING
    internal
#else
    private
#endif
        InvocationExpressionSyntax ParseInvocationExpressionSyntax(ExpressionSyntax expr)
    {
        Debug.Assert(this.IsPossibleInvocationArguments());
        var arguments = this.ParseInvocationArguments();
        return this._syntaxFactory.InvocationExpression(expr, arguments);
    }

#if TESTING
    internal
#else
    private
#endif
        ImplicitSelfParameterInvocationExpressionSyntax ParseImplicitSelfParameterInvocationExpression(ExpressionSyntax expr)
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.ColonToken);
        var colon = this.EatToken(SyntaxKind.ColonToken);
        var name = this.ParseIdentifierName();
        InvocationArgumentsSyntax arguments;
        if (this.IsPossibleInvocationArguments())
            arguments = this.ParseInvocationArguments();
        else
        {
            arguments = this._syntaxFactory.ArgumentList(
                SyntaxFactory.MissingToken(SyntaxKind.OpenParenToken),
                SyntaxFactory.SeparatedList<ArgumentSyntax>(),
                SyntaxFactory.MissingToken(SyntaxKind.CloseParenToken));
            arguments = this.AddError(arguments, ErrorCode.ERR_InvocationArgumentsExpected);
        }
        return this._syntaxFactory.ImplicitSelfParameterInvocationExpression(expr, colon, name, arguments);
    }
}
