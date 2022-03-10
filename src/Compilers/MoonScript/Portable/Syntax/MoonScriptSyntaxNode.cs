namespace SamLu.CodeAnalysis.MoonScript;

public abstract partial class MoonScriptSyntaxNode
{
    internal Syntax.InternalSyntax.MoonScriptSyntaxNode MoonScriptGreen => (Syntax.InternalSyntax.MoonScriptSyntaxNode)this.Green;

    public SyntaxKind Kind() => this.MoonScriptGreen.Kind;
}
