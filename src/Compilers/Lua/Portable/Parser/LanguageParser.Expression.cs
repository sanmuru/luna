using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class LanguageParser
{
    private ExpressionListSyntax ParseExpressionList() =>
        this.ParseExpressionListOpt() ??
            // 创建一个缺失的标识符名称语法来组成表达式列表，并报告错误信息。
            this.CreateMissingExpressionList();

    private ExpressionListSyntax? ParseExpressionListOpt()
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
                this.syntaxFactory.ExpressionList(list));
    }

    private ExpressionListSyntax CreateMissingExpressionList()
    {
        var missing = this.syntaxFactory.ExpressionList(
            new(SyntaxList.List(
                this.CreateMissingIdentifierName()
            ))
        );
        return this.AddError(missing, ErrorCode.ERR_InvalidExprTerm);
    }

#if TESTING
    protected internal
#else
    private protected
#endif
        bool IsPossibleExpression() =>
        this.CurrentTokenKind switch
        {
            SyntaxKind.NilKeyword or
            SyntaxKind.FalseKeyword or
            SyntaxKind.TrueKeyword or
            SyntaxKind.NumericLiteralToken or
            SyntaxKind.StringLiteralExpression or
            SyntaxKind.MultiLineRawStringLiteralToken or
            SyntaxKind.DotDotDotToken or
            SyntaxKind.OpenParenToken or
            SyntaxKind.FunctionKeyword or
            SyntaxKind.OpenBraceToken or
            SyntaxKind.MinusToken or
            SyntaxKind.NotKeyword or
            SyntaxKind.HashToken or
            SyntaxKind.TildeToken => true,

            SyntaxKind.IdentifierToken => true,

            _ => false
        };


#if TESTING
    protected internal
#else
    private protected
#endif
        ExpressionSyntax ParseExpression()
    {
        var expr = this.ParseExpressionWithoutOperator();
        return SyntaxFacts.IsBinaryExpressionOperatorToken(this.CurrentTokenKind) ?
            this.ParseExpressionWithOperator(expr) : expr;
    }

#if TESTING
    protected internal
#else
    private protected
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
            SyntaxKind.StringLiteralExpression =>
                this.ParseLiteralExpression(SyntaxKind.StringLiteralExpression
#if DEBUG
                    , SyntaxKind.StringLiteralExpression
#endif
                    ),
            SyntaxKind.MultiLineRawStringLiteralToken =>
                this.ParseLiteralExpression(SyntaxKind.MultiLineRawStringLiteralToken
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
                this.ParseIdentifierStartedExpression(),

            _ =>
                throw ExceptionUtilities.Unreachable,
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

                default:
                    return expr;
            }
        }

        throw ExceptionUtilities.Unreachable;
    }

    private ExpressionSyntax ParseExpressionWithOperator(ExpressionSyntax? first = null)
    {
        ExpressionWithOperatorParser innerParser;
        if (first is null)
            innerParser = new(this);
        else
            innerParser = new(this, first);

        return innerParser.ParseExpressionWithOperator();
    }

    private protected LiteralExpressionSyntax ParseLiteralExpression(SyntaxKind kind
#if DEBUG
        , SyntaxKind currentTokenKind
#endif
        )
    {
#if DEBUG
        Debug.Assert(this.CurrentTokenKind == currentTokenKind);
#endif

        return this.syntaxFactory.LiteralExpression(kind, this.EatToken());
    }

    private protected ParenthesizedExpressionSyntax ParseParenthesizedExpression()
    {
        var openParen = this.EatToken(SyntaxKind.OpenParenToken);
        var expression = this.ParseExpression();
        var closeParen = this.EatToken(SyntaxKind.CloseParenToken);
        return this.syntaxFactory.ParenthesizedExpression(openParen, expression, closeParen);
    }

    private FunctionDefinitionExpressionSyntax ParseFunctionDefinitionExpression()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.FunctionKeyword);

        var function = this.EatToken(SyntaxKind.FunctionKeyword);
        this.ParseFunctionBody(out var parameters, out var block, out var end);
        return this.syntaxFactory.FunctionDefinitionExpression(function, parameters, block, end);
    }

    private TableConstructorExpressionSyntax ParseTableConstructorExpression()
    {
        var openBrace = this.EatToken(SyntaxKind.OpenBraceToken);
        var field = this.ParseFieldList();
        var closeBrace = this.EatToken(SyntaxKind.CloseBraceToken);
        return this.syntaxFactory.TableConstructorExpression(openBrace, field, closeBrace);
    }

    private ExpressionSyntax ParseIdentifierStartedExpression()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.IdentifierToken);

        return this.ParseIdentifierName();
    }

    private ExpressionSyntax ParseSimpleMemberAccessExpressionSyntax(ExpressionSyntax self)
    {
        var dot = this.EatToken(SyntaxKind.DotToken);
        var member = this.ParseIdentifierName();
        return this.syntaxFactory.SimpleMemberAccessExpression(self, dot, member);
    }

    private ExpressionSyntax ParseIndexMemberAccessExpressionSyntax(ExpressionSyntax self)
    {
        var openBracket = this.EatToken(SyntaxKind.OpenBracketToken);
        var member = this.ParseExpression();
        var closeBracket = this.EatToken(SyntaxKind.CloseBracketToken);
        return this.syntaxFactory.IndexMemberAccessExpression(self, openBracket, member, closeBracket);
    }

}
