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
            return false;
        else if (statements.Count == 1)
            // If we have a single statement, it might be small, like "return null", or large,
            // like a loop or if or switch with many statements inside. Use the width as a proxy for
            // how big it is. If it's small, its better to forgo a many children list anyway, since the
            // weak reference would consume as much memory as is saved.
            return statements[0]!.Width > 60;
        else
            // For 2 or more statements, go ahead and create a many-children lists.
            return true;
    }

#if TESTING
    protected internal
#else
    private protected
#endif
        void ParseStatements(in SyntaxListBuilder<StatementSyntax> statementsBuilder) =>
        this.ParseSyntaxList(
            statementsBuilder,
            parseNodeFunc: _ => this.ParseStatement(),
            predicate: _ => this.IsPossibleStatement());

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
            SyntaxKind.ForKeyword or
            SyntaxKind.FunctionKeyword or
            SyntaxKind.LocalKeyword => true,

            SyntaxKind.CommaToken or // 表达式列表的分隔符
            SyntaxKind.EqualsToken => true, // 赋值操作符
            _ => this.IsPossibleExpression()
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
            case SyntaxKind.FunctionKeyword:
                return this.ParseFunctionDefinitionStatement();
            case SyntaxKind.LocalKeyword:
                if (this.PeekToken(1).Kind == SyntaxKind.FunctionKeyword)
                    return this.ParseLocalFunctionDefinitionStatement();
                else
                    return this.ParseLocalDeclarationStatement();

            case SyntaxKind.CommaToken: // 表达式列表的分隔符
            case SyntaxKind.EqualsToken: // 赋值操作符
                return this.ParseAssignmentStatement();
            default:
                return this.ParseAssignmentStatement();
        }

        throw ExceptionUtilities.Unreachable;
    }

    private AssignmentStatementSyntax ParseAssignmentStatement()
    {
        var left = this.ParseExpressionList();
        var equals = this.EatToken(SyntaxKind.EqualsToken);
        var right = this.ParseExpressionList();
        var semicolon = this.TryEatToken(SyntaxKind.SemicolonToken);
        return this.syntaxFactory.AssignmentStatement(left, equals, right, semicolon);
    }

    private protected LabelStatementSyntax ParseLabelStatement()
    {
        var leftColonColon = this.EatToken(SyntaxKind.ColonColonToken);
        var labelName = this.ParseIdentifierName();
        var rightColonColon = this.EatToken(SyntaxKind.ColonColonToken);
        return this.syntaxFactory.LabelStatement(leftColonColon, labelName, rightColonColon);
    }

    private protected BreakStatementSyntax ParseBreakStatement()
    {
        var breakKeyword = this.EatToken(SyntaxKind.BreakKeyword);
        return this.syntaxFactory.BreakStatement(breakKeyword);
    }

    private protected GotoStatementSyntax ParseGotoStatement()
    {
        var gotoKeyword = this.EatToken(SyntaxKind.GotoKeyword);
        var labelName = this.ParseIdentifierName();
        var semicolon = this.TryEatToken(SyntaxKind.SemicolonToken);
        return this.syntaxFactory.GotoStatement(gotoKeyword, labelName, semicolon);
    }

    private protected ReturnStatementSyntax ParseReturnStatement()
    {
        var returnKeyword = this.EatToken(SyntaxKind.ReturnKeyword);
        var expressions = this.ParseExpressionListOpt();
        var semicolon = this.TryEatToken(SyntaxKind.SemicolonToken);
        return this.syntaxFactory.ReturnStatement(returnKeyword, expressions, semicolon);
    }

    private protected DoStatementSyntax ParseDoStatement()
    {
        var doKeyword = this.EatToken(SyntaxKind.DoKeyword);
        var block = this.ParseBlock();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this.syntaxFactory.DoStatement(doKeyword, block, endKeyword);
    }

    private protected WhileStatementSyntax ParseWhileStatement()
    {
        var whileKeyword = this.EatToken(SyntaxKind.WhileKeyword);
        var condition = this.ParseExpression();
        var doKeyword = this.EatToken(SyntaxKind.DoKeyword);
        var block = this.ParseBlock();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this.syntaxFactory.WhileStatement(whileKeyword, condition, doKeyword, block, endKeyword);
    }

    private protected RepeatStatementSyntax ParseRepeatStatement()
    {
        var repeatKeyword = this.EatToken(SyntaxKind.RepeatKeyword);
        var block = this.ParseBlock();
        var untilKeyword = this.EatToken(SyntaxKind.UntilKeyword);
        var condition = this.ParseExpression();
        var semicolon = this.TryEatToken(SyntaxKind.SemicolonToken);
        return this.syntaxFactory.RepeatStatement(repeatKeyword, block, untilKeyword, condition, semicolon);
    }

    private protected IfStatementSyntax ParseIfStatement()
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

    private protected SyntaxList<ElseIfClauseSyntax> ParseElseIfClausesOpt() =>
        this.ParseSyntaxList(
            parseNodeFunc: _ => this.ParseElseIfClause(),
            predicate: _ => this.CurrentTokenKind == SyntaxKind.ElseIfKeyword);

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

    private protected IfStatementSyntax ParseMisplaceElseIf()
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
        var namesBuilder = this.pool.AllocateSeparated<IdentifierNameSyntax>();
        this.ParseSeparatedIdentifierNames(namesBuilder);
        switch (this.CurrentTokenKind)
        {
            case SyntaxKind.InKeyword:// 是迭代for循环。
                return this.ParseIterableForStatement(forKeyword, this.pool.ToListAndFree(namesBuilder));
            case SyntaxKind.EqualsToken: // 是增量for循环。
                if (namesBuilder.Count == 1)
                    return this.ParseIncrementalForStatement(forKeyword, (IdentifierNameSyntax)namesBuilder[0]!);
                else // 定义了多个标识符。
                {
                    // 只保留第一个标识符，将后续的标识符标志及分隔符标志均处理为被跳过的语法标志。
                    var names = this.pool.ToListAndFree(namesBuilder);
                    var name = names[0]!;
                    var skippedTokens = this.pool.Allocate<SyntaxToken>();
                    skippedTokens.AddRange(names.GetWithSeparators(), 1, namesBuilder.Count - 1);
                    // 将被跳过的语法标志添加到第一个标识符的尾部。
                    name = this.AddTrailingSkippedSyntax(name, this.syntaxFactory.SkippedTokensTrivia(this.pool.ToListAndFree(skippedTokens)));
                    // 添加错误。
                    this.AddError(name, ErrorCode.ERR_TooManyIdentifiers);

                    return this.ParseIncrementalForStatement(forKeyword, name);
                }
            default: // 不知道是什么结构，推断使用最适合的结构。
                if (namesBuilder.Count == 1) // 单个标识符，推断使用增量for循环。
                    return this.ParseIncrementalForStatement(forKeyword, (IdentifierNameSyntax)namesBuilder[0]!);
                else // 多个标识符，推断使用迭代for循环。
                    return this.ParseIterableForStatement(forKeyword, this.pool.ToListAndFree(namesBuilder));
        }
    }

    private protected ForInStatementSyntax ParseIterableForStatement(SyntaxToken forKeyword, SeparatedSyntaxList<IdentifierNameSyntax> names)
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

    private protected ForStatementSyntax ParseIncrementalForStatement(SyntaxToken forKeyword, IdentifierNameSyntax name)
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

    private FunctionDefinitionStatementSyntax ParseFunctionDefinitionStatement()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.FunctionKeyword);

        var function = this.EatToken(SyntaxKind.FunctionKeyword);
        var name = this.ParseName();
        this.ParseFunctionBody(out var parameters, out var block, out var end);
        return this.syntaxFactory.FunctionDefinitionStatement(function, name, parameters, block, end);
    }

    private LocalFunctionDefinitionStatementSyntax ParseLocalFunctionDefinitionStatement()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.LocalKeyword);
        Debug.Assert(this.PeekToken(1).Kind == SyntaxKind.FunctionKeyword);

        var local = this.EatToken(SyntaxKind.LocalKeyword);
        var function = this.EatToken(SyntaxKind.FunctionKeyword);
        var name = this.ParseIdentifierName();
        this.ParseFunctionBody(out var parameters, out var block, out var end);
        return this.syntaxFactory.LocalFunctionDefinitionStatement(local, function, name, parameters, block, end);
    }

    private LocalDeclarationStatementSyntax ParseLocalDeclarationStatement()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.LocalKeyword);

        var local = this.EatToken(SyntaxKind.LocalKeyword);
        var identifiers = this.ParseSeparatedIdentifierNames();
        SyntaxToken? equals = null;
        ExpressionListSyntax? values = null;
        if (this.CurrentTokenKind == SyntaxKind.EqualsToken)
        {
            equals = this.EatToken();
            values = this.ParseExpressionListOpt();
        }
        SyntaxToken? semicolon = null;
        if (this.CurrentTokenKind == SyntaxKind.SemicolonToken)
        {
            semicolon = this.EatToken();
            if (equals is not null && values is null)
                semicolon = this.AddError(semicolon, ErrorCode.ERR_InvalidExprTerm);
        }
        if (equals is not null)
        {
            // 创建一个缺失的标识符名称语法来组成表达式列表，并报告错误信息。
            values ??= this.CreateMissingExpressionList();
        }

        return this.syntaxFactory.LocalDeclarationStatement(local, identifiers, equals, values, semicolon);
    }
}
