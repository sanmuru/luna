using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class LanguageParser
{
    private ExpressionListSyntax? ParseExpressionListOpt()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    private TableConstructorExpressionSyntax ParseTableConstructorExpression()
    {
        throw new NotImplementedException();
    }

    private ExpressionSyntax ParseIdentifierStartedExpression()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.IdentifierToken);

        throw new NotImplementedException();
    }

    private ExpressionSyntax ParseExpressionWithOperator(ExpressionSyntax? expr = null)
    {
        throw new NotImplementedException();
    }

    private ExpressionSyntax ParseExpressionWithRightAssociatedOperator(ExpressionSyntax expr)
    {
        Stack<ExpressionSyntax> exprStack = new(10);
        Stack<SyntaxToken> optStack = new(10);
        exprStack.Push(expr);

        bool previewStateIsExpr = true; // 上一个状态是表达式。
        var skippedTokenBuilder = SyntaxListBuilder<SyntaxToken>.Create();
        ErrorCode errorCode = default;
Next:
        var kind = this.CurrentTokenKind;
        if (SyntaxFacts.IsUnaryExpressionOperatorToken(kind))
        {
            if (previewStateIsExpr)
            {
                if (kind is SyntaxKind.MinusToken or SyntaxKind.TildeToken) // 可能是二元运算符。
                    goto Final;

                // 在应该出现二元运算符的位置出现了一元运算符。
                errorCode = ErrorCode.ERR_InvalidExprTerm;
                collectErrorAndSkip();
                goto Next;
            }

            appendSkippedTokensTrivia();
            optStack.Push(this.EatToken());
        }
        else if (SyntaxFacts.IsBinaryExpression(kind))
        {
            if (kind == SyntaxKind.CaretToken)
            {
                if (!previewStateIsExpr)
                {
                    var token = optStack.Peek();
                    if (SyntaxFacts.IsUnaryExpressionOperatorToken(token.Kind)) // 一元运算符后缺失表达式
                    {
                        errorCode = ErrorCode.ERR_InvalidExprTerm;
                        collectErrorAndSkip();
                        goto Next;
                    }
                }

                appendSkippedTokensTrivia();
                optStack.Push(this.EatToken());
                previewStateIsExpr = false;
            }
            else if (kind == SyntaxKind.DotDotToken)
            {
                var token = optStack.Peek();
                if (!previewStateIsExpr)
                {
                    if (SyntaxFacts.IsUnaryExpressionOperatorToken(token.Kind)) // 一元运算符后缺失表达式
                    {
                        errorCode = ErrorCode.ERR_InvalidExprTerm;
                        collectErrorAndSkip();
                    }
                }

                // 组合前方的操作符表达式。
                // 正好一元运算符也是右结合，因此可以和取幂运算符一起处理。
                if (token.Kind != SyntaxKind.DotDotToken)
                {
                    expr = exprStack.Pop(); // 出栈最后一个表达式。
                    do
                    {
                        if (token.Kind == SyntaxKind.CaretToken) // 取幂二元运算符
                        {
                            Debug.Assert(exprStack.Count != 0);

                            expr = this.syntaxFactory.BinaryExpression(
                                SyntaxKind.ExponentiationExpression,
                                exprStack.Pop(),
                                optStack.Pop(),
                                expr);
                        }
                        else // 一元运算符
                            expr = this.syntaxFactory.UnaryExpression(
                                SyntaxFacts.GetUnaryExpression(token.Kind),
                                optStack.Pop(),
                                expr);

                        if (optStack.Count == 0) break;
                        token = optStack.Peek();
                    }
                    while (token.Kind != SyntaxKind.CaretToken);
                    exprStack.Push(expr); // 入栈结果表达式以进行后续操作。
                }

                appendSkippedTokensTrivia();
                optStack.Push(this.EatToken());
                previewStateIsExpr = false;
            }
            else
                goto Final;
        }
        else
        {
            if (previewStateIsExpr)
                // 在应该出现运算符的位置出现了其他标志。
                goto Final;

            appendSkippedTokensTrivia();
            exprStack.Push(this.ParseExpressionWithoutOperator());
        }
        goto Next;

Final:
        {
            // 组合前方的操作符表达式。
            if (optStack.Count > 0)
            {
                expr = exprStack.Pop(); // 出栈最后一个表达式。
                var token = optStack.Peek();
                do
                {
                    if (token.Kind is SyntaxKind.CaretToken or SyntaxKind.TildeToken) // 二元运算符
                    {
                        Debug.Assert(exprStack.Count != 0);

                        expr = this.syntaxFactory.BinaryExpression(
                            SyntaxFacts.GetBinaryExpression(token.Kind),
                            exprStack.Pop(),
                            optStack.Pop(),
                            expr);
                    }
                    else // 一元运算符
                        expr = this.syntaxFactory.UnaryExpression(
                            SyntaxFacts.GetUnaryExpression(token.Kind),
                            optStack.Pop(),
                            expr);

                    if (optStack.Count == 0) break;
                    token = optStack.Peek();
                }
                while (token.Kind != SyntaxKind.CaretToken);
                exprStack.Push(expr); // 入栈结果表达式以进行后续操作。
            }

            Debug.Assert(exprStack.Count == 1);
            return exprStack.Pop();
        }

        // 收集错误消息并跳过当前标志。 
        void collectErrorAndSkip()
        {
            skippedTokenBuilder.Add(this.EatToken());
        }

        void appendSkippedTokensTrivia()
        {
            // 快速跳过没有错误的情况。
            if (skippedTokenBuilder.Count == 0) return;

            var trivia = this.syntaxFactory.SkippedTokensTrivia(skippedTokenBuilder.ToList());
            trivia = this.AddError(trivia, errorCode); // 报告诊断错误。
            if (previewStateIsExpr)
            {
                exprStack.Push(this.AddTrailingSkippedSyntax(exprStack.Pop(), trivia));
            }
            else
            {
                optStack.Push(this.AddTrailingSkippedSyntax(optStack.Pop(), trivia));
            }
        }
    }
    private ExpressionSyntax ParseSimpleMemberAccessExpressionSyntax(ExpressionSyntax expr)
    {
        throw new NotImplementedException();
    }

    private ExpressionSyntax ParseIndexMemberAccessExpressionSyntax(ExpressionSyntax expr)
    {
        throw new NotImplementedException();
    }

}
