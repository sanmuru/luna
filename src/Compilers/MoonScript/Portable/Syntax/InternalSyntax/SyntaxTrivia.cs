namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

partial class SyntaxTrivia
{
    public int WhiteSpaceIndent => this.IsWhiteSpace ? 0 :
        this.Text.Sum(SyntaxFacts.WhiteSpaceIndent);
}
