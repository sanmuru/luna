using System.Diagnostics;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class LanguageParser
{
    [NonCopyable]
    private ref partial struct ExpressionWithOperatorParser
    {
        private readonly LanguageParser _parser;
        private ParseState _state;

        private enum ParseState : byte
        {
            Initial = 0,        // 初始状态
            UnaryStart,         // 一元运算符后方
            BinaryStart,        // 二元运算符后方
            Terminal,           // 可以结束
            Skip = Bad - 1,     // 跳过当前标志后继续
            Bad = byte.MaxValue // 立即错误
        }

        private static ParseState Transit(ParseState state, SyntaxKind opt, LanguageParser parser) =>
            state switch
            {
                ParseState.Initial or
                ParseState.UnaryStart => opt switch
                {
                    SyntaxKind.MinusToken or
                    SyntaxKind.NotKeyword or
                    SyntaxKind.HashToken or
                    SyntaxKind.TildeToken => ParseState.UnaryStart,

                    SyntaxKind.PlusToken or
                    SyntaxKind.AsteriskToken or
                    SyntaxKind.SlashToken or
                    SyntaxKind.SlashSlashToken or
                    SyntaxKind.CaretToken or
                    SyntaxKind.PersentToken or
                    SyntaxKind.AmpersandToken or
                    SyntaxKind.BarToken or
                    SyntaxKind.LessThanLessThanToken or
                    SyntaxKind.GreaterThanGreaterThanToken or
                    SyntaxKind.DotDotToken or
                    SyntaxKind.LessThanToken or
                    SyntaxKind.LessThanEqualsToken or
                    SyntaxKind.GreaterThanToken or
                    SyntaxKind.GreaterThanEqualsToken or
                    SyntaxKind.EqualsEqualsToken or
                    SyntaxKind.TildeEqualsToken or
                    SyntaxKind.AndKeyword or
                    SyntaxKind.OrKeyword => ParseState.Skip,

                    _ => parser.IsPossibleExpression() ?
                    ParseState.Terminal : ParseState.Bad
                },
                ParseState.BinaryStart => opt switch
                {
                    SyntaxKind.PlusToken or
                    SyntaxKind.MinusToken or
                    SyntaxKind.AsteriskToken or
                    SyntaxKind.SlashToken or
                    SyntaxKind.SlashSlashToken or
                    SyntaxKind.CaretToken or
                    SyntaxKind.PersentToken or
                    SyntaxKind.HashToken or
                    SyntaxKind.AmpersandToken or
                    SyntaxKind.TildeToken or
                    SyntaxKind.BarToken or
                    SyntaxKind.LessThanLessThanToken or
                    SyntaxKind.GreaterThanGreaterThanToken or
                    SyntaxKind.DotDotToken or
                    SyntaxKind.LessThanToken or
                    SyntaxKind.LessThanEqualsToken or
                    SyntaxKind.GreaterThanToken or
                    SyntaxKind.GreaterThanEqualsToken or
                    SyntaxKind.EqualsEqualsToken or
                    SyntaxKind.TildeEqualsToken or
                    SyntaxKind.AndKeyword or
                    SyntaxKind.NotKeyword or
                    SyntaxKind.OrKeyword => ParseState.Skip,

                    _ => parser.IsPossibleExpression() ?
                    ParseState.Terminal : ParseState.Bad
                },
                ParseState.Terminal => opt switch
                {
                    SyntaxKind.HashToken or
                    SyntaxKind.NotKeyword => ParseState.Skip,

                    SyntaxKind.PlusToken or
                    SyntaxKind.MinusToken or
                    SyntaxKind.AsteriskToken or
                    SyntaxKind.SlashToken or
                    SyntaxKind.SlashSlashToken or
                    SyntaxKind.CaretToken or
                    SyntaxKind.PersentToken or
                    SyntaxKind.AmpersandToken or
                    SyntaxKind.TildeToken or
                    SyntaxKind.BarToken or
                    SyntaxKind.LessThanLessThanToken or
                    SyntaxKind.GreaterThanGreaterThanToken or
                    SyntaxKind.DotDotToken or
                    SyntaxKind.LessThanToken or
                    SyntaxKind.LessThanEqualsToken or
                    SyntaxKind.GreaterThanToken or
                    SyntaxKind.GreaterThanEqualsToken or
                    SyntaxKind.EqualsEqualsToken or
                    SyntaxKind.TildeEqualsToken or
                    SyntaxKind.AndKeyword or
                    SyntaxKind.OrKeyword => ParseState.BinaryStart,

                    _ => ParseState.Bad
                },

                _ => throw ExceptionUtilities.Unreachable
            };

        public ExpressionWithOperatorParser(LanguageParser parser)
        {
            this._parser = parser;
            this._state = ParseState.Initial;
        }

        public ExpressionWithOperatorParser(LanguageParser parser, ExpressionSyntax expr)
        {
            this._parser = parser;
            this._state = ParseState.Terminal;
            this._exprStack.Push(expr);
        }

        private LuaSyntaxNode? TryEatTokenOrExpression()
        {
            LuaSyntaxNode? result = null;

            var skippedTokenListBuilder = SyntaxListBuilder<SyntaxToken>.Create();
            while (this._state != ParseState.Bad && result is null)
            {
                var state = Transit(this._state, this._parser.CurrentTokenKind, this._parser);
                switch (state)
                {
                    case ParseState.UnaryStart:
                    case ParseState.BinaryStart:
                        result = this._parser.EatToken();
                        this._state = state;
                        break;

                    case ParseState.Terminal:
                        result = this._parser.ParseExpressionWithoutOperator();
                        this._state = state;
                        break;

                    case ParseState.Skip:
                        skippedTokenListBuilder.Add(this._parser.EatToken());
                        continue;

                    case ParseState.Bad:
                        result = null;
                        this._state = state;
                        break;

                    default:
                        throw ExceptionUtilities.Unreachable;
                }
            }

            this.AppendSkippedTokensTrivia(skippedTokenListBuilder, ErrorCode.ERR_InvalidExprTerm);
            return result;
        }

        private readonly Stack<ExpressionSyntax> _exprStack = new(10);
        private readonly Stack<(SyntaxToken opt, bool isUnary)> _optStack = new(10);

        private bool CurrentTokenIsUnary
        {
            get
            {
                Debug.Assert(this._state is ParseState.UnaryStart or ParseState.BinaryStart);
                return this._state == ParseState.UnaryStart;
            }
        }

        private bool IsAssociative(SyntaxToken nextOpt)
        {
            bool nextIsUnary = this.CurrentTokenIsUnary;
            (var opt, var isUnary) = this._optStack.Peek();
            var precedence = SyntaxFacts.GetOperatorPrecedence(nextOpt.Kind, nextIsUnary);
            var nextPrecedence = SyntaxFacts.GetOperatorPrecedence(opt.Kind, isUnary);
            // 优先级不同情况下：
            if (nextPrecedence != precedence) return nextPrecedence < precedence; // 下一个运算符优先级比上一个运算符优先级低时才可。

            // 优先级相同情况下：
            if (SyntaxFacts.IsUnaryExpressionOperatorToken(opt.Kind)) return false; // 上一个运算符是一元运算符时不可。

            if (SyntaxFacts.IsLeftAssociativeBinaryExpressionOperatorToken(opt.Kind)) return true; // 上一个运算符是左结合时才可。

            // 其他情况：
            return false;
        }

        internal ExpressionSyntax ParseExpressionWithOperator()
        {
            LuaSyntaxNode? tokenOrExpression = null;
Next:
            tokenOrExpression = this.TryEatTokenOrExpression();
            if (tokenOrExpression is null) goto Final;
            else if (tokenOrExpression is SyntaxToken nextOpt)
            {
                // 前方有多个可组合的二元运算符，全部组合。
                while (this._optStack.Count > 0 && this.IsAssociative(nextOpt))
                {
                    // 此时上一个运算符不应该是一元运算符。
                    Debug.Assert(this._optStack.Peek().isUnary == false);
                    Debug.Assert(this._exprStack.Count >= 2);
                    var opt = this._optStack.Pop().opt;
                    var right = this._exprStack.Pop();
                    var left = this._exprStack.Pop();
                    this._exprStack.Push(this._parser.syntaxFactory.BinaryExpression(
                        SyntaxFacts.GetBinaryExpression(opt.Kind),
                        left,
                        opt,
                        right
                    ));
                }
                this._optStack.Push((nextOpt, this.CurrentTokenIsUnary));
            }
            else if (tokenOrExpression is ExpressionSyntax expr)
            {
                // 前方有多个一元运算符，全部组合。
                while (this._optStack.Count > 0 && this._optStack.Peek().isUnary)
                {
                    var opt = this._optStack.Pop().opt;
                    expr = this._parser.syntaxFactory.UnaryExpression(
                        SyntaxFacts.GetUnaryExpression(opt.Kind),
                        opt,
                        expr
                    );
                }
                this._exprStack.Push(expr);
            }

Final:
/* 有两种情况到达这里：
 * 1. 表达式语法正确，结构没有缺失。
 * 2. 发生错误，无法继续。
 */
            if (this._exprStack.Count == 1 && this._optStack.Count == 0) // 最理想的成功情况。
                return this._exprStack.Pop();

            // 错误情况1：
        }

        private void AppendSkippedTokensTrivia(in SyntaxListBuilder skippedTokenBuilder, in ErrorCode errorCode)
        {
            // 快速跳过没有错误的情况。
            if (skippedTokenBuilder.Count == 0) return;

            var trivia = this._parser.syntaxFactory.SkippedTokensTrivia(skippedTokenBuilder.ToList());
            trivia = this._parser.AddError(trivia, errorCode); // 报告诊断错误。
            if (previewStateIsExpr)
            {
                exprStack.Push(this.AddTrailingSkippedSyntax(exprStack.Pop(), trivia));
            }
            else
            {
                optStack.Push(this.AddTrailingSkippedSyntax(optStack.Pop(), trivia));
            }
        }

        private void collectErrorAndSkip()
        {
            skippedTokenBuilder.Add(this.EatToken());
        }
    }
}
