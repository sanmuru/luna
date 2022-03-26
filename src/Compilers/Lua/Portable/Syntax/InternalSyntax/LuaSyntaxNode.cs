using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

internal partial class LuaSyntaxNode
{
    public override partial Microsoft.CodeAnalysis.SyntaxToken CreateSeparator<TNode>(SyntaxNode element) => Lua.SyntaxFactory.Token(SyntaxKind.CommanToken);

    public override partial bool IsTriviaWithEndOfLine() =>
        this.Kind switch
        {
            SyntaxKind.EndOfLineTrivia or
            SyntaxKind.SingleLineCommentTrivia => true,
            _ => false
        };
}
