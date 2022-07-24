using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class LanguageParser
{
#if TESTING
    protected internal
#else
    private protected
#endif
         IdentifierNameSyntax ParseIdentifierName()
    {
        var identifier = this.EatToken(SyntaxKind.IdentifierToken);
        return this.syntaxFactory.IdentifierName(identifier);
    }

    private protected void ParseSeparatedIdentifierNames(in SeparatedSyntaxListBuilder<IdentifierNameSyntax> namesBuilder, SyntaxKind separatorKind = SyntaxKind.CommaToken)
    {
        var name = this.ParseIdentifierName();
        namesBuilder.Add(name);

        int lastTokenPosition = -1;
        while (this.CurrentToken.Kind == separatorKind
            && IsMakingProgress(ref lastTokenPosition))
        {
            var comma = this.EatToken(SyntaxKind.CommaToken);
            namesBuilder.AddSeparator(comma);

            name = this.ParseIdentifierName();
            namesBuilder.Add(name);
        }
    }
}
