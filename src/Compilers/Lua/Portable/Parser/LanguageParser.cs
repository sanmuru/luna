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
        return this.syntaxFactory.Chunk(block, endOfFile);
    }

#if TESTING
    protected internal
#else
    private protected
#endif
        BlockSyntax ParseBlock()
    {
        var statementBuilder = this.pool.Allocate<StatementSyntax>();
        this.ParseStatements(statementBuilder);

        SyntaxList<StatementSyntax> statements;
        if (LanguageParser.IsLargeEnoughNonEmptyStatementList(statementBuilder))
            statements = new SyntaxList<StatementSyntax>(SyntaxList.List(((SyntaxListBuilder)statementBuilder).ToArray()));
        else
            statements = statementBuilder;

        var block = this.syntaxFactory.Block(statements);

        this.pool.Free(statementBuilder);
        return block;
    }

    private protected SkippedTokensTriviaSyntax? SkipTokens(Func<SyntaxToken, bool> predicate)
    {
        if (predicate(this.CurrentToken))
        {
            var builder = this.pool.Allocate<SyntaxToken>();
            do
                builder.Add(this.EatToken());
            while (predicate(this.CurrentToken));
            return this.syntaxFactory.SkippedTokensTrivia(this.pool.ToListAndFree(builder));
        }

        return null;
    }

    #region ParseSyntaxList & ParseSeparatedSyntaxList
    private protected void ParseSyntaxList<TNode>(
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

    private protected SyntaxList<TNode> ParseSyntaxList<TNode>(
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicate)
        where TNode : LuaSyntaxNode
    {
        var builder = this.pool.Allocate<TNode>();
        this.ParseSyntaxList(builder, parseNodeFunc, predicate);
        var list = this.pool.ToListAndFree(builder);
        return list;
    }

    private protected TList? ParseSyntaxList<TNode, TList>(
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicate,
        Func<SyntaxList<TNode>, TList?> createListFunc)
        where TNode : LuaSyntaxNode
        where TList : LuaSyntaxNode
    {
        var list = createListFunc(this.ParseSyntaxList(parseNodeFunc, predicate));
        return list;
    }

    private protected void ParseSeparatedSyntaxList<TNode>(
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

    private protected SeparatedSyntaxList<TNode> ParseSeparatedSyntaxList<TNode>(
        Func<int, TNode> parseNodeFunc,
        Func<int, bool> predicate)
        where TNode : LuaSyntaxNode
    {
        var builder = this.pool.AllocateSeparated<TNode>();
        this.ParseSeparatedSyntaxList(builder, parseNodeFunc, predicate);
        var list = this.pool.ToListAndFree(builder);
        return list;
    }

    private protected TList? ParseSeparatedSyntaxList<TNode, TList>(
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

    private protected partial bool MatchFactoryContext(GreenNode green, SyntaxFactoryContext context) =>
#warning 未完成。
        true;
}
