using System.Diagnostics;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class LanguageParser
{
    // Is this statement list non-empty, and large enough to make using weak children beneficial?
    private static bool IsLargeEnoughNonEmptyStatementList(SyntaxListBuilder<StatementSyntax> statements)
    {
        if (statements.Count == 0)
        {
            return false;
        }
        else if (statements.Count == 1)
        {
            // If we have a single statement, it might be small, like "return null", or large,
            // like a loop or if or switch with many statements inside. Use the width as a proxy for
            // how big it is. If it's small, its better to forgo a many children list anyway, since the
            // weak reference would consume as much memory as is saved.
            return statements[0].Width > 60;
        }
        else
        {
            // For 2 or more statements, go ahead and create a many-children lists.
            return true;
        }
    }

#if TESTING
    protected internal
#else
    private protected
#endif
        void ParseStatements(in SyntaxListBuilder<StatementSyntax> statementsBuilder)
    {
        bool startAtNewLine = this.IsTriviaContainsEndOfLine(this.prevTokenTrailingTrivia) || this.IsTriviaContainsEndOfLine(this.CurrentToken.LeadingTrivia);

        int lastTokenPosition = -1;
        while (this.CurrentToken.Kind != SyntaxKind.EndOfFileToken
            && IsMakingProgress(ref lastTokenPosition))
        {
            if (this.IsPossibleStatement())
            {
                var statement = this.ParseStatement();
                statementsBuilder.Add(statement);
            }
        }
    }

#if TESTING
    protected internal
#else
    private protected
#endif
        bool IsPossibleStatement() =>
        this.CurrentTokenKind switch
        {
            SyntaxKind.SemicolonToken or
            SyntaxKind.ColonColonToken or
            SyntaxKind.BreakKeyword or
            SyntaxKind.GotoKeyword or
            SyntaxKind.ReturnKeyword or
            SyntaxKind.DoKeyword or
            SyntaxKind.WhileKeyword or
            SyntaxKind.RepeatKeyword or
            SyntaxKind.IfStatement or
            SyntaxKind.ElseIfKeyword or
            SyntaxKind.ElseKeyword or
            SyntaxKind.ForKeyword => true,

            _ => false
        };

#if TESTING
    protected internal
#else
    private protected
#endif
        StatementSyntax ParseStatement()
    {
        switch (this.CurrentTokenKind)
        {
            case SyntaxKind.SemicolonToken:
                return this.syntaxFactory.EmptyStatement(this.EatToken());
            case SyntaxKind.ColonColonToken:
                return this.ParseLabelStatement();
            case SyntaxKind.BreakKeyword:
                return this.ParseBreakStatement();
            case SyntaxKind.GotoKeyword:
                return this.ParseGotoStatement();
            case SyntaxKind.ReturnKeyword:
                return this.ParseReturnStatement();
            case SyntaxKind.DoKeyword:
                return this.ParseDoStatement();
            case SyntaxKind.WhileKeyword:
                return this.ParseWhileStatement();
            case SyntaxKind.RepeatKeyword:
                return this.ParseRepeatStatement();
            case SyntaxKind.IfKeyword:
                return this.ParseIfStatement();
            case SyntaxKind.ElseIfKeyword:
                return this.ParseMisplaceElseIf();
            case SyntaxKind.ForKeyword:
                return this.ParseForStatement();
        }

        throw ExceptionUtilities.Unreachable;
    }

    private protected StatementSyntax ParseLabelStatement()
    {
        var leftColonColon = this.EatToken(SyntaxKind.ColonColonToken);
        var labelName = this.ParseIdentifierName();
        var rightColonColon = this.EatToken(SyntaxKind.ColonColonToken);
        return this.syntaxFactory.LabelStatement(leftColonColon, labelName, rightColonColon);
    }

    private protected StatementSyntax ParseBreakStatement()
    {
        var breakKeyword = this.EatToken(SyntaxKind.BreakKeyword);
        return this.syntaxFactory.BreakStatement(breakKeyword);
    }

    private protected StatementSyntax ParseGotoStatement()
    {
        var gotoKeyword = this.EatToken(SyntaxKind.GotoKeyword);
        var labelName = this.ParseIdentifierName();
        var semicolon = this.TryEatToken(SyntaxKind.SemicolonToken);
        return this.syntaxFactory.GotoStatement(gotoKeyword, labelName, semicolon);
    }

    private protected StatementSyntax ParseReturnStatement()
    {
        var returnKeyword = this.EatToken(SyntaxKind.ReturnKeyword);
        var expressions = this.ParseExpressionListOpt();
        var semicolon = this.TryEatToken(SyntaxKind.SemicolonToken);
        return this.syntaxFactory.ReturnStatement(returnKeyword, expressions, semicolon);
    }

    private protected StatementSyntax ParseDoStatement()
    {
        var doKeyword = this.EatToken(SyntaxKind.DoKeyword);
        var block = this.ParseBlock();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this.syntaxFactory.DoStatement(doKeyword, block, endKeyword);
    }

    private protected StatementSyntax ParseWhileStatement()
    {
        var whileKeyword = this.EatToken(SyntaxKind.WhileKeyword);
        var condition = this.ParseExpression();
        var doKeyword = this.EatToken(SyntaxKind.DoKeyword);
        var block = this.ParseBlock();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this.syntaxFactory.WhileStatement(whileKeyword, condition, doKeyword, block, endKeyword);
    }

    private protected StatementSyntax ParseRepeatStatement()
    {
        var repeatKeyword = this.EatToken(SyntaxKind.RepeatKeyword);
        var block = this.ParseBlock();
        var untilKeyword = this.EatToken(SyntaxKind.UntilKeyword);
        var condition = this.ParseExpression();
        var semicolon = this.TryEatToken(SyntaxKind.SemicolonToken);
        return this.syntaxFactory.RepeatStatement(repeatKeyword, block, untilKeyword, condition, semicolon);
    }

    private protected StatementSyntax ParseIfStatement()
    {
        var ifKeyword = this.EatToken(SyntaxKind.IfKeyword);
        var condition = this.ParseExpression();
        var thenKeyword = this.EatToken(SyntaxKind.ThenKeyword);
        var block = this.ParseBlock();
        var elseIfClauses = this.ParseElseIfClausesOpt();
        var elseClause = this.ParseElseClauseOpt();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this.syntaxFactory.IfStatement(ifKeyword, condition, thenKeyword, block, elseIfClauses, elseClause, endKeyword);
    }

    private protected SyntaxList<ElseIfClauseSyntax> ParseElseIfClausesOpt()
    {
        SyntaxList<ElseIfClauseSyntax> elseIfClauses;

        if (this.PeekToken().Kind == SyntaxKind.ElseIfKeyword)
        {
            var builder = SyntaxListBuilder<ElseIfClauseSyntax>.Create();
            int lastTokenPosition = -1;
            while (this.CurrentToken.Kind == SyntaxKind.ElseIfKeyword
                && IsMakingProgress(ref lastTokenPosition))
            {
                var clause = this.ParseElseIfClause();
                builder.Add(clause);
            }
            elseIfClauses = builder.ToList();
        }
        else
            elseIfClauses = default;

        return elseIfClauses;
    }

    private protected ElseClauseSyntax? ParseElseClauseOpt() =>
        this.PeekToken().Kind == SyntaxKind.ElseIfKeyword ?
            this.ParseElseClause() : null;

    private protected ElseIfClauseSyntax ParseElseIfClause()
    {
        var elseIfKeyword = this.EatToken(SyntaxKind.ElseIfKeyword);
        var condition = this.ParseExpression();
        var thenKeyword = this.EatToken(SyntaxKind.ThenKeyword);
        var block = this.ParseBlock();
        return this.syntaxFactory.ElseIfClause(elseIfKeyword, condition, thenKeyword, block);
    }

    private protected ElseClauseSyntax ParseElseClause()
    {
        var elseKeyword = this.EatToken(SyntaxKind.ElseKeyword);
        var block = this.ParseBlock();
        return this.syntaxFactory.ElseClause(elseKeyword, block);
    }

    private protected StatementSyntax ParseMisplaceElseIf()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.ElseIfKeyword);

        var ifKeyword = this.EatToken(SyntaxKind.IfKeyword, ErrorCode.ERR_ElseIfCannotStartStatement);
        var condition = this.ParseExpression();
        var thenKeyword = this.EatToken(SyntaxKind.ThenKeyword);
        var block = this.ParseBlock();
        var elseIfClauses = this.ParseElseIfClausesOpt();
        var elseClause = this.ParseElseClauseOpt();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this.syntaxFactory.IfStatement(ifKeyword, condition, thenKeyword, block, elseIfClauses, elseClause, endKeyword);
    }

    private protected StatementSyntax ParseForStatement()
    {
        var forKeyword = this.EatToken(SyntaxKind.ForKeyword);
        var namesBuilder = SeparatedSyntaxListBuilder<IdentifierNameSyntax>.Create();
        this.ParseSeparatedIdentifierNames(namesBuilder);
        switch (this.CurrentTokenKind)
        {
            case SyntaxKind.InKeyword:// 是迭代for循环。
                return this.ParseIterableForStatement(forKeyword, namesBuilder.ToList());
            case SyntaxKind.EqualsToken: // 是增量for循环。
                if (namesBuilder.Count == 1)
                    return this.ParseIncrementalForStatement(forKeyword, (IdentifierNameSyntax)namesBuilder[0]!);
                else // 定义了多个标识符。
                {
                    // 只保留第一个标识符，将后续的标识符标志及分隔符标志均处理为被跳过的语法标志。
                    var names = namesBuilder.ToList();
                    var name = names[0]!;
                    var skippedTokens = SyntaxListBuilder<SyntaxToken>.Create();
                    skippedTokens.AddRange(names.GetWithSeparators(), 1, namesBuilder.Count - 1);
                    // 将被跳过的语法标志添加到第一个标识符的尾部。
                    name = this.AddTrailingSkippedSyntax(name, this.syntaxFactory.SkippedTokensTrivia(skippedTokens.ToList()));
                    // 添加错误。
                    this.AddError(name, ErrorCode.ERR_TooManyIdentifiers);

                    return this.ParseIncrementalForStatement(forKeyword, name);
                }
            default: // 不知道是什么结构，推断使用最适合的结构。
                if (namesBuilder.Count == 1) // 单个标识符，推断使用增量for循环。
                    return this.ParseIncrementalForStatement(forKeyword, (IdentifierNameSyntax)namesBuilder[0]!);
                else // 多个标识符，推断使用迭代for循环。
                    return this.ParseIterableForStatement(forKeyword, namesBuilder.ToList());
        }
    }

    private protected StatementSyntax ParseIterableForStatement(SyntaxToken forKeyword, SeparatedSyntaxList<IdentifierNameSyntax> names)
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.InKeyword);

        var inKeyword = this.EatToken(SyntaxKind.InKeyword);
        var iteration = this.ParseExpression();
        var doKeyword = this.EatToken(SyntaxKind.DoKeyword);
        var block = this.ParseBlock();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this.syntaxFactory.ForInStatement(
            forKeyword,
            names,
            inKeyword,
            iteration,
            doKeyword,
            block,
            endKeyword);
    }

    private protected StatementSyntax ParseIncrementalForStatement(SyntaxToken forKeyword, IdentifierNameSyntax name)
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.EqualsToken);

        var equals = this.EatToken(SyntaxKind.EqualsToken);
        var initial = this.ParseExpression();
        var firstComma = this.EatToken(SyntaxKind.CommaToken);
        var limit = this.ParseExpression();

        SyntaxToken? secondComma = null;
        ExpressionSyntax? step = null;
        if (this.CurrentTokenKind == SyntaxKind.CommaToken)
        {
            secondComma = this.EatToken(SyntaxKind.CommaToken);
            step = this.ParseExpression();
        }

        var doKeyword = this.EatToken(SyntaxKind.DoKeyword);
        var block = this.ParseBlock();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this.syntaxFactory.ForStatement(
            forKeyword,
            name,
            equals,
            initial,
            firstComma,
            limit,
            secondComma,
            step,
            doKeyword,
            block,
            endKeyword);
    }
}
