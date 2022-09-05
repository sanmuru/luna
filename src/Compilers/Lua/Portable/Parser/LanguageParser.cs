using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

using System.Xml.Linq;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

partial class LanguageParser
{
    internal ChunkSyntax ParseCompilationUnit()
    {
        var block = this.ParseBlock(SyntaxKind.Chunk);
        var endOfFile = this.EatToken(SyntaxKind.EndOfFileToken);
        return this._syntaxFactory.Chunk(block, endOfFile);
    }

#if TESTING
    internal
#else
    private
#endif
        BlockSyntax ParseBlock(SyntaxKind structure)
    {
        this._syntaxFactoryContext.EnterStructure(structure);

        var statementBuilder = this._pool.Allocate<StatementSyntax>();
        this.ParseStatements(statementBuilder);

        SyntaxList<StatementSyntax> statements;
        if (LanguageParser.IsLargeEnoughNonEmptyStatementList(statementBuilder))
            statements = new SyntaxList<StatementSyntax>(SyntaxList.List(((SyntaxListBuilder)statementBuilder).ToArray()));
        else
            statements = statementBuilder;

        this._syntaxFactoryContext.LeaveStructure(structure);

        var returnStat = this.CurrentTokenKind == SyntaxKind.ReturnKeyword ? this.ParseReturnStatement() : null;
        var block = this._syntaxFactory.Block(statements, returnStat);

        this._pool.Free(statementBuilder);
        return block;
    }

    #region SkipTokensAndNodes
    private GreenNode? SkipTokens(Func<SyntaxToken, bool> predicate, LuaSyntaxVisitor<SyntaxToken>? visitor = null)
    {
        if (predicate(this.CurrentToken))
        {
            var builder = this._pool.Allocate<SyntaxToken>();
            do
            {
                var token = this.EatToken();
                builder.Add(visitor is null ? token : visitor.Visit(token));
            }
            while (predicate(this.CurrentToken));
            return this._syntaxFactory.SkippedTokensTrivia(this._pool.ToListAndFree(builder));
        }

        return null;
    }

    private GreenNode? SkipTokensAndExpressions(Func<SyntaxToken, bool> predicate, LuaSyntaxVisitor<LuaSyntaxNode>? visitor = null)
    {
        var builder = this._pool.Allocate<LuaSyntaxNode>();
        while (true)
        {
            if (this.IsPossibleExpression())
            {
                var expr = this.ParseExpressionCore();
                builder.Add(visitor is null ? expr : visitor.Visit(expr));
            }
            else if (predicate(this.CurrentToken))
            {
                var token = this.EatToken();
                builder.Add(visitor is null ? token : visitor.Visit(token));
            }
            else break;
        }

        if (builder.Count == 0)
        {
            this._pool.Free(builder);
            return null;
        }
        else
            return this._pool.ToListAndFree(builder).Node;
    }

    private GreenNode? SkipTokensAndStatements(Func<SyntaxToken, bool> predicate, LuaSyntaxVisitor<LuaSyntaxNode>? visitor = null)
    {
        var builder = this._pool.Allocate<LuaSyntaxNode>();
        while (true)
        {
            if (this.IsPossibleStatement())
            {
                var stat = this.ParseStatement();
                builder.Add(visitor is null ? stat : visitor.Visit(stat));
            }
            else if (predicate(this.CurrentToken))
            {
                var token = this.EatToken();
                builder.Add(visitor is null ? token : visitor.Visit(token));
            }
            else break;
        }

        if (builder.Count == 0)
        {
            this._pool.Free(builder);
            return null;
        }
        else
            return this._pool.ToListAndFree(builder).Node;
    }
    #endregion

    #region ParseSyntaxList & ParseSeparatedSyntaxList
    private void ParseSyntaxList<TNode>(
        in SyntaxListBuilder<TNode> builder,
        Func<int, bool> predicateNode,
        Func<int, bool, TNode> parseNode,
        int minCount = 0)
        where TNode : LuaSyntaxNode
    {
        int lastTokenPosition = -1;
        int index = 0;
        while (this.CurrentTokenKind != SyntaxKind.EndOfFileToken &&
            this.IsMakingProgress(ref lastTokenPosition))
        {
            if (!predicateNode(index)) break;

            const bool missing = false;
            var node = parseNode(index, missing);
            builder.Add(node);

            index++;
        }
        // 处理缺失（最小数量不足）的部分。
        while (index < minCount)
        {
            const bool missing = true;
            var node = parseNode(index, missing);
            builder.Add(node);

            index++;
        }
    }

    private SyntaxList<TNode> ParseSyntaxList<TNode>(
        Func<int, bool> predicateNode,
        Func<int, bool, TNode> parseNode,
        int minCount = 0)
        where TNode : LuaSyntaxNode
    {
        var builder = this._pool.Allocate<TNode>();
        this.ParseSyntaxList(builder, predicateNode, parseNode, minCount);
        var list = this._pool.ToListAndFree(builder);
        return list;
    }

    private TList? ParseSyntaxList<TNode, TList>(
        Func<int, bool> predicateNode,
        Func<int, bool, TNode> parseNode,
        Func<SyntaxList<TNode>, TList?> createListFunc,
        int minCount = 0)
        where TNode : LuaSyntaxNode
        where TList : LuaSyntaxNode
    {
        var list = createListFunc(this.ParseSyntaxList(predicateNode, parseNode, minCount));
        return list;
    }

    private void ParseSeparatedSyntaxList<TNode, TSeparator>(
        in SeparatedSyntaxListBuilder<TNode> builder,
        Func<int, bool> predicateNode,
        Func<int, bool, TNode> parseNode,
        Func<int, bool> predicateSeparator,
        Func<int, bool, TSeparator> parseSeparator,
        bool allowTrailingSeparator = false,
        int minCount = 0)
        where TNode : LuaSyntaxNode
        where TSeparator : LuaSyntaxNode
    {
        int index = 0;
        if (predicateNode(index))
        {
            const bool missing = false;

            TNode node = parseNode(index, missing);
            builder.Add(node);

            int lastTokenPosition = -1;
            index = 1;
            while (this.CurrentTokenKind != SyntaxKind.EndOfFileToken &&
                this.IsMakingProgress(ref lastTokenPosition))
            {
                if (!predicateSeparator(index - 1)) break;

                var resetPoint = this.GetResetPoint();

                var separator = parseSeparator(index - 1, missing);
                if (predicateNode(index))
                {
                    builder.AddSeparator(separator);

                    node = parseNode(index, missing);
                    builder.Add(node);

                    index++;
                    this.Release(ref resetPoint);
                }
                else // 无法继续，恢复到上一个重置点并退出循环。
                {
                    this.Reset(ref resetPoint);
                    this.Release(ref resetPoint);
                    break;
                }
            }
        }

        // 处理缺失（最小数量不足）的部分。
        while (index < minCount)
        {
            const bool missing = true;

            var separator = parseSeparator(index - 1, missing && (!allowTrailingSeparator || !predicateSeparator(index - 1)));
            if (allowTrailingSeparator) allowTrailingSeparator = false;
            builder.AddSeparator(separator);

            var node = parseNode(index, missing);
            builder.Add(node);

            index++;
        }
    }

    private SeparatedSyntaxList<TNode> ParseSeparatedSyntaxList<TNode, TSeparator>(
        Func<int, bool> predicateNode,
        Func<int, bool, TNode> parseNode,
        Func<int, bool> predicateSeparator,
        Func<int, bool, TSeparator> parseSeparator,
        bool allowTrailingSeparator = false,
        int minCount = 0)
        where TNode : LuaSyntaxNode
        where TSeparator : LuaSyntaxNode
    {
        var builder = this._pool.AllocateSeparated<TNode>();
        this.ParseSeparatedSyntaxList(builder, predicateNode, parseNode, predicateSeparator, parseSeparator, allowTrailingSeparator, minCount);
        var list = this._pool.ToListAndFree(builder);
        return list;
    }

    private TList? ParseSeparatedSyntaxList<TNode, TSeparator, TList>(
        Func<int, bool> predicateNode,
        Func<int, bool, TNode> parseNodeFunc,
        Func<int, bool> predicateSeparator,
        Func<int, bool, TSeparator> parseSeparator,
        Func<SeparatedSyntaxList<TNode>, TList?> createListFunc,
        bool allowTrailingSeparator = false,
        int minCount = 0)
        where TNode : LuaSyntaxNode
        where TSeparator : LuaSyntaxNode
        where TList : LuaSyntaxNode
    {
        var list = createListFunc(this.ParseSeparatedSyntaxList(predicateNode, parseNodeFunc, predicateSeparator, parseSeparator, allowTrailingSeparator, minCount));
        return list;
    }
    #endregion

    private partial bool MatchFactoryContext(GreenNode green, SyntaxFactoryContext context) =>
#warning 未完成。
        true;

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
}
