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

        var block = this._syntaxFactory.Block(statements);

        this._pool.Free(statementBuilder);
        return block;
    }

    private GreenNode? SkipTokens(Func<SyntaxToken, bool> predicate, LuaSyntaxVisitor<SyntaxToken>? visitor = null)
    {
        if (predicate(this.CurrentToken))
        {
            var builder = this._pool.Allocate<SyntaxToken>();
            do
            {
                var token = this.EatToken();
                builder.Add(visitor is null ? token : visitor.VisitToken(token));
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
                builder.Add(visitor is null ? token : visitor.VisitToken(token));
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

    #region ParseSyntaxList & ParseSeparatedSyntaxList
    private void ParseSyntaxList<TNode>(
        in SyntaxListBuilder<TNode> builder,
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicate)
        where TNode : LuaSyntaxNode
    {
        int lastTokenPosition = -1;
        int index = 0;
        while (this.CurrentTokenKind != SyntaxKind.EndOfFileToken &&
            this.IsMakingProgress(ref lastTokenPosition))
        {
            if (predicate(index))
            {
                var node = parseNodeFunc(index);
                builder.Add(node);

                index++;
            }
        }
    }

    private SyntaxList<TNode> ParseSyntaxList<TNode>(
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicate)
        where TNode : LuaSyntaxNode
    {
        var builder = this._pool.Allocate<TNode>();
        this.ParseSyntaxList(builder, parseNodeFunc, predicate);
        var list = this._pool.ToListAndFree(builder);
        return list;
    }

    private TList? ParseSyntaxList<TNode, TList>(
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicate,
        Func<SyntaxList<TNode>, TList?> createListFunc)
        where TNode : LuaSyntaxNode
        where TList : LuaSyntaxNode
    {
        var list = createListFunc(this.ParseSyntaxList(parseNodeFunc, predicate));
        return list;
    }

    private void ParseSeparatedSyntaxList<TNode>(
        in SeparatedSyntaxListBuilder<TNode> builder,
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicate)
        where TNode : LuaSyntaxNode
    {
        if (!predicate(0)) return;
        TNode node = parseNodeFunc(0);
        builder.Add(node);

        int lastTokenPosition = -1;
        int index = 1;
        while (this.CurrentTokenKind == SyntaxKind.CommaToken &&
            this.IsMakingProgress(ref lastTokenPosition))
        {
            if (predicate(index))
            {
                var separator = this.EatToken();
                builder.AddSeparator(separator);

                node = parseNodeFunc(index);
                builder.Add(node);

                index++;
            }
        }
    }

    private SeparatedSyntaxList<TNode> ParseSeparatedSyntaxList<TNode>(
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicate)
        where TNode : LuaSyntaxNode
    {
        var builder = this._pool.AllocateSeparated<TNode>();
        this.ParseSeparatedSyntaxList(builder, parseNodeFunc, predicate);
        var list = this._pool.ToListAndFree(builder);
        return list;
    }

    private TList? ParseSeparatedSyntaxList<TNode, TList>(
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicate,
        Func<SeparatedSyntaxList<TNode>, TList?> createListFunc)
        where TNode : LuaSyntaxNode
        where TList : LuaSyntaxNode
    {
        var list = createListFunc(this.ParseSeparatedSyntaxList(parseNodeFunc, predicate));
        return list;
    }
    #endregion

    private partial bool MatchFactoryContext(GreenNode green, SyntaxFactoryContext context) =>
#warning 未完成。
        true;
}
