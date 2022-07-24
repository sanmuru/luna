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

    private protected bool IsTriviaContainsEndOfLine(SyntaxList<LuaSyntaxNode> triviaList)
    {
        if (triviaList.Count == 0) return false;

        foreach (var trivia in triviaList)
        {
            if (trivia.IsTriviaWithEndOfLine())
                return true;
        }

        return false;
    }

    private protected bool IsTriviaContainsEndOfLine(GreenNode? node) =>
        node is not null && this.IsTriviaContainsEndOfLine(new SyntaxList<LuaSyntaxNode>(node));

    private protected partial bool MatchFactoryContext(GreenNode green, SyntaxFactoryContext context) =>
#warning 未完成。
        true;
}
