﻿using System.Diagnostics;
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
        void ParseStatements(in SyntaxListBuilder<StatementSyntax> statementsBuilder) =>
        this.ParseSyntaxList(
            statementsBuilder,
            parseNodeFunc: _ => this.ParseStatement(),
            predicateNode: _ => this.IsPossibleStatement());

#if TESTING
    internal
#else
    private
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
    internal
#else
    private
#endif
        StatementSyntax ParseStatement()
    {
        switch (this.CurrentTokenKind)
        {
            case SyntaxKind.SemicolonToken:
                return this._syntaxFactory.EmptyStatement(this.EatToken());
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

#if TESTING
    internal
#else
    private
#endif
        AssignmentStatementSyntax ParseAssignmentStatement()
    {
        var left = this.ParseAssgLvalueList();

        // 当赋值符号缺失时，中断整个语句的分析。
        if (this.CurrentTokenKind != SyntaxKind.EqualsToken)
            return this._syntaxFactory.AssignmentStatement(left, this.EatToken(SyntaxKind.EqualsToken), this.CreateMissingExpressionList());

        var equals = this.EatToken();
        var right = this.ParseExpressionList();
        return this._syntaxFactory.AssignmentStatement(left, equals, right);
    }

#if TESTING
    internal
#else
    private
#endif
        ExpressionListSyntax ParseAssgLvalueList()
    {
        if (this.CurrentTokenKind != SyntaxKind.CommaToken && !this.IsPossibleExpression())
            return this.CreateMissingExpressionList(ErrorCode.ERR_IdentifierExpected);

        return this.ParseSeparatedSyntaxList(
            parseNodeFunc: index =>
            {
                if (this.IsPossibleExpression())
                {
                    var expr = this.ParseExpression();
                    return expr switch
                    {
                        // 仅标识符语法和成员操作语法（普通或索引）能作为赋值符号左侧表达式。
                        IdentifierNameSyntax or
                        MemberAccessExpressionSyntax => expr,

                        _ => this.AddError(expr, ErrorCode.ERR_AssgLvalueExpected)
                    };
                }
                else
                {
                    // 第一项缺失的情况：
                    if (index == 0)
                        Debug.Assert(this.CurrentTokenKind == SyntaxKind.CommaToken);
                    return this.AddError(this.CreateMissingIdentifierName(), ErrorCode.ERR_IdentifierExpected);
                }
            },
            predicateNode: _ => true,
            predicateSeparator: _ => this.CurrentTokenKind == SyntaxKind.CommaToken,
            createListFunc: list => this._syntaxFactory.ExpressionList(list))!;
    }

#if TESTING
    internal
#else
    private
#endif
        LabelStatementSyntax ParseLabelStatement()
    {
        var leftColonColon = this.EatToken(SyntaxKind.ColonColonToken);
        var labelName = this.ParseIdentifierName();
        var rightColonColon = this.EatToken(SyntaxKind.ColonColonToken);
        return this._syntaxFactory.LabelStatement(leftColonColon, labelName, rightColonColon);
    }

#if TESTING
    internal
#else
    private
#endif
        BreakStatementSyntax ParseBreakStatement()
    {
        var breakKeyword = this.EatToken(SyntaxKind.BreakKeyword);
        return this._syntaxFactory.BreakStatement(breakKeyword);
    }

#if TESTING
    internal
#else
    private
#endif
        GotoStatementSyntax ParseGotoStatement()
    {
        var gotoKeyword = this.EatToken(SyntaxKind.GotoKeyword);
        var labelName = this.ParseIdentifierName();
        return this._syntaxFactory.GotoStatement(gotoKeyword, labelName);
    }

#if TESTING
    internal
#else
    private
#endif
        ReturnStatementSyntax ParseReturnStatement()
    {
        var returnKeyword = this.EatToken(SyntaxKind.ReturnKeyword);
        var expressions = this.ParseExpressionListOpt();
        return this._syntaxFactory.ReturnStatement(returnKeyword, expressions);
    }

#if TESTING
    internal
#else
    private
#endif
        DoStatementSyntax ParseDoStatement()
    {
        var doKeyword = this.EatToken(SyntaxKind.DoKeyword);
        var block = this.ParseBlock();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this._syntaxFactory.DoStatement(doKeyword, block, endKeyword);
    }

#if TESTING
    internal
#else
    private
#endif
        WhileStatementSyntax ParseWhileStatement()
    {
        var whileKeyword = this.EatToken(SyntaxKind.WhileKeyword);
        var condition = this.ParseExpression();
        var doKeyword = this.EatToken(SyntaxKind.DoKeyword);
        var block = this.ParseBlock();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this._syntaxFactory.WhileStatement(whileKeyword, condition, doKeyword, block, endKeyword);
    }

#if TESTING
    internal
#else
    private
#endif
        RepeatStatementSyntax ParseRepeatStatement()
    {
        var repeatKeyword = this.EatToken(SyntaxKind.RepeatKeyword);
        var block = this.ParseBlock();
        var untilKeyword = this.EatToken(SyntaxKind.UntilKeyword);
        var condition = this.ParseExpression();
        return this._syntaxFactory.RepeatStatement(repeatKeyword, block, untilKeyword, condition);
    }

#if TESTING
    internal
#else
    private
#endif
        IfStatementSyntax ParseIfStatement()
    {
        var ifKeyword = this.EatToken(SyntaxKind.IfKeyword);
        var condition = this.ParseExpression();
        var thenKeyword = this.EatToken(SyntaxKind.ThenKeyword);
        var block = this.ParseBlock();
        var elseIfClauses = this.ParseElseIfClausesOpt();
        var elseClause = this.ParseElseClauseOpt();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this._syntaxFactory.IfStatement(ifKeyword, condition, thenKeyword, block, elseIfClauses, elseClause, endKeyword);
    }

#if TESTING
    internal
#else
    private
#endif
        SyntaxList<ElseIfClauseSyntax> ParseElseIfClausesOpt() =>
        this.ParseSyntaxList(
            parseNodeFunc: _ => this.ParseElseIfClause(),
            predicateNode: _ => this.CurrentTokenKind == SyntaxKind.ElseIfKeyword);

#if TESTING
    internal
#else
    private
#endif
        ElseClauseSyntax? ParseElseClauseOpt() =>
        this.PeekToken().Kind == SyntaxKind.ElseIfKeyword ?
            this.ParseElseClause() : null;

#if TESTING
    internal
#else
    private
#endif
        ElseIfClauseSyntax ParseElseIfClause()
    {
        var elseIfKeyword = this.EatToken(SyntaxKind.ElseIfKeyword);
        var condition = this.ParseExpression();
        var thenKeyword = this.EatToken(SyntaxKind.ThenKeyword);
        var block = this.ParseBlock();
        return this._syntaxFactory.ElseIfClause(elseIfKeyword, condition, thenKeyword, block);
    }

#if TESTING
    internal
#else
    private
#endif
        ElseClauseSyntax ParseElseClause()
    {
        var elseKeyword = this.EatToken(SyntaxKind.ElseKeyword);
        var block = this.ParseBlock();
        return this._syntaxFactory.ElseClause(elseKeyword, block);
    }

#if TESTING
    internal
#else
    private
#endif
        IfStatementSyntax ParseMisplaceElseIf()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.ElseIfKeyword);

        var ifKeyword = this.EatToken(SyntaxKind.IfKeyword, ErrorCode.ERR_ElseIfCannotStartStatement);
        var condition = this.ParseExpression();
        var thenKeyword = this.EatToken(SyntaxKind.ThenKeyword);
        var block = this.ParseBlock();
        var elseIfClauses = this.ParseElseIfClausesOpt();
        var elseClause = this.ParseElseClauseOpt();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this._syntaxFactory.IfStatement(ifKeyword, condition, thenKeyword, block, elseIfClauses, elseClause, endKeyword);
    }

#if TESTING
    internal
#else
    private
#endif
        StatementSyntax ParseForStatement()
    {
        var forKeyword = this.EatToken(SyntaxKind.ForKeyword);
        var namesBuilder = this._pool.AllocateSeparated<IdentifierNameSyntax>();
        this.ParseSeparatedIdentifierNames(namesBuilder);
        switch (this.CurrentTokenKind)
        {
            case SyntaxKind.InKeyword:// 是迭代for循环。
                return this.ParseIterableForStatement(forKeyword, this._pool.ToListAndFree(namesBuilder));
            case SyntaxKind.EqualsToken: // 是增量for循环。
                if (namesBuilder.Count == 1)
                    return this.ParseIncrementalForStatement(forKeyword, (IdentifierNameSyntax)namesBuilder[0]!);
                else // 定义了多个标识符。
                {
                    // 只保留第一个标识符，将后续的标识符标志及分隔符标志均处理为被跳过的语法标志。
                    var names = this._pool.ToListAndFree(namesBuilder);
                    var name = names[0]!;
                    var skippedTokens = this._pool.Allocate<SyntaxToken>();
                    skippedTokens.AddRange(names.GetWithSeparators(), 1, namesBuilder.Count - 1);
                    // 将被跳过的语法标志添加到第一个标识符的尾部。
                    name = this.AddTrailingSkippedSyntax(name, this._syntaxFactory.SkippedTokensTrivia(this._pool.ToListAndFree(skippedTokens)));
                    // 添加错误。
                    this.AddError(name, ErrorCode.ERR_TooManyIdentifiers);

                    return this.ParseIncrementalForStatement(forKeyword, name);
                }
            default: // 不知道是什么结构，推断使用最适合的结构。
                if (namesBuilder.Count == 1) // 单个标识符，推断使用增量for循环。
                    return this.ParseIncrementalForStatement(forKeyword, (IdentifierNameSyntax)namesBuilder[0]!);
                else // 多个标识符，推断使用迭代for循环。
                    return this.ParseIterableForStatement(forKeyword, this._pool.ToListAndFree(namesBuilder));
        }
    }

#if TESTING
    internal
#else
    private
#endif
        ForInStatementSyntax ParseIterableForStatement(SyntaxToken forKeyword, SeparatedSyntaxList<IdentifierNameSyntax> names)
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.InKeyword);

        var inKeyword = this.EatToken(SyntaxKind.InKeyword);
        var iteration = this.ParseExpression();
        var doKeyword = this.EatToken(SyntaxKind.DoKeyword);
        var block = this.ParseBlock();
        var endKeyword = this.EatToken(SyntaxKind.EndKeyword);
        return this._syntaxFactory.ForInStatement(
            forKeyword,
            names,
            inKeyword,
            iteration,
            doKeyword,
            block,
            endKeyword);
    }

#if TESTING
    internal
#else
    private
#endif
        ForStatementSyntax ParseIncrementalForStatement(SyntaxToken forKeyword, IdentifierNameSyntax name)
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
        return this._syntaxFactory.ForStatement(
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

#if TESTING
    internal
#else
    private
#endif
        FunctionDefinitionStatementSyntax ParseFunctionDefinitionStatement()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.FunctionKeyword);

        var function = this.EatToken(SyntaxKind.FunctionKeyword);
        var name = this.ParseName();
        this.ParseFunctionBody(out var parameters, out var block, out var end);
        return this._syntaxFactory.FunctionDefinitionStatement(function, name, parameters, block, end);
    }

#if TESTING
    internal
#else
    private
#endif
        LocalFunctionDefinitionStatementSyntax ParseLocalFunctionDefinitionStatement()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.LocalKeyword);
        Debug.Assert(this.PeekToken(1).Kind == SyntaxKind.FunctionKeyword);

        var local = this.EatToken(SyntaxKind.LocalKeyword);
        var function = this.EatToken(SyntaxKind.FunctionKeyword);
        var name = this.ParseIdentifierName();
        this.ParseFunctionBody(out var parameters, out var block, out var end);
        return this._syntaxFactory.LocalFunctionDefinitionStatement(local, function, name, parameters, block, end);
    }

#if TESTING
    internal
#else
    private
#endif
        LocalDeclarationStatementSyntax ParseLocalDeclarationStatement()
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

        return this._syntaxFactory.LocalDeclarationStatement(local, identifiers, equals, values);
    }
}
