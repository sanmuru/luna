using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

internal partial class MoonScriptSyntaxNode
{
    public virtual object? Value => this.Kind switch
    {
        SyntaxKind.TrueKeyword => Boxes.BoxedTrue,
        SyntaxKind.FalseKeyword => Boxes.BoxedFalse,
        SyntaxKind.NilKeyword => null,
        _ => this.KindText
    };

    internal static partial NodeFlags SetFactoryContext(NodeFlags flags, SyntaxFactoryContext context) => flags;

    public override partial Microsoft.CodeAnalysis.SyntaxToken CreateSeparator<TNode>(SyntaxNode element) => MoonScript.SyntaxFactory.Token(SyntaxKind.CommaToken);

    public override partial bool IsTriviaWithEndOfLine() =>
        this.Kind switch
        {
            SyntaxKind.EndOfLineTrivia or
            SyntaxKind.SingleLineCommentTrivia => true,
            _ => false
        };
}
