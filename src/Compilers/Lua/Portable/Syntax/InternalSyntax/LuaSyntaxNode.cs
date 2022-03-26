using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

internal partial class LuaSyntaxNode
{
    public virtual object? Value => this.Kind switch
    {
        SyntaxKind.TrueKeyword => Boxes.BoxedTrue,
        SyntaxKind.FalseKeyword => Boxes.BoxedFalse,
        SyntaxKind.NilKeyword => null,
        _ => this.KindText
    };

    public override partial Microsoft.CodeAnalysis.SyntaxToken CreateSeparator<TNode>(SyntaxNode element) => Lua.SyntaxFactory.Token(SyntaxKind.CommanToken);

    public override partial bool IsTriviaWithEndOfLine() =>
        this.Kind switch
        {
            SyntaxKind.EndOfLineTrivia or
            SyntaxKind.SingleLineCommentTrivia => true,
            _ => false
        };
}
