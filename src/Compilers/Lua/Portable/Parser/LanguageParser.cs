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
        var block = this.ParseBlock();
        var endOfFile = this.EatToken(SyntaxKind.EndOfFileToken);
        return this._syntaxFactory.Chunk(block, endOfFile);
    }

#if TESTING
    internal
#else
    private
#endif
        BlockSyntax ParseBlock()
    {
        var statementBuilder = this._pool.Allocate<StatementSyntax>();
        this.ParseStatements(statementBuilder);

        SyntaxList<StatementSyntax> statements;
        if (LanguageParser.IsLargeEnoughNonEmptyStatementList(statementBuilder))
            statements = new SyntaxList<StatementSyntax>(SyntaxList.List(((SyntaxListBuilder)statementBuilder).ToArray()));
        else
            statements = statementBuilder;

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
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicateNode)
        where TNode : LuaSyntaxNode
    {
        int lastTokenPosition = -1;
        int index = 0;
        while (this.CurrentTokenKind != SyntaxKind.EndOfFileToken &&
            this.IsMakingProgress(ref lastTokenPosition))
        {
            if (!predicateNode(index)) break;

            var node = parseNodeFunc(index);
            builder.Add(node);

            index++;
        }
    }

    private SyntaxList<TNode> ParseSyntaxList<TNode>(
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicateNode)
        where TNode : LuaSyntaxNode
    {
        var builder = this._pool.Allocate<TNode>();
        this.ParseSyntaxList(builder, parseNodeFunc, predicateNode);
        var list = this._pool.ToListAndFree(builder);
        return list;
    }

    private TList? ParseSyntaxList<TNode, TList>(
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicateNode,
        Func<SyntaxList<TNode>, TList?> createListFunc)
        where TNode : LuaSyntaxNode
        where TList : LuaSyntaxNode
    {
        var list = createListFunc(this.ParseSyntaxList(parseNodeFunc, predicateNode));
        return list;
    }

    private void ParseSeparatedSyntaxList<TNode>(
        in SeparatedSyntaxListBuilder<TNode> builder,
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicateNode,
        Func<int, bool> predicateSeparator)
        where TNode : LuaSyntaxNode
    {
        if (!predicateNode(0)) return;
        TNode node = parseNodeFunc(0);
        builder.Add(node);

        int lastTokenPosition = -1;
        int index = 1;
        while (this.CurrentTokenKind != SyntaxKind.EndOfFileToken &&
            this.IsMakingProgress(ref lastTokenPosition))
        {
            if (!predicateSeparator(index - 1)) break;

            var resetPoint = this.GetResetPoint();

            var separator = this.EatToken();
            if (predicateNode(index))
            {
                builder.AddSeparator(separator);

                node = parseNodeFunc(index);
                builder.Add(node);

                index++;
            }
            else
                this.Reset(ref resetPoint);
        }
    }

    private SeparatedSyntaxList<TNode> ParseSeparatedSyntaxList<TNode>(
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicateNode,
        Func<int, bool> predicateSeparator)
        where TNode : LuaSyntaxNode
    {
        var builder = this._pool.AllocateSeparated<TNode>();
        this.ParseSeparatedSyntaxList(builder, parseNodeFunc, predicateNode, predicateSeparator);
        var list = this._pool.ToListAndFree(builder);
        return list;
    }

    private TList? ParseSeparatedSyntaxList<TNode, TList>(
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicateNode,
        Func<int, bool> predicateSeparator,
        Func<SeparatedSyntaxList<TNode>, TList?> createListFunc)
        where TNode : LuaSyntaxNode
        where TList : LuaSyntaxNode
    {
        var list = createListFunc(this.ParseSeparatedSyntaxList(parseNodeFunc, predicateNode, predicateSeparator));
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
