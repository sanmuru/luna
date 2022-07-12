using SamLu.CodeAnalysis.Lua;

namespace SyntaxGenerator.Lua;

internal sealed class LuaGrammarGenerator : GrammarGenerator
{
    protected override string LanguageName => "Lua";

    protected override string[] MajorRules => new[]
    {
        "ChunkSyntax",
        "StatementSyntax",
        "ExpressionSyntax",
        "StructuredTriviaSyntax"
    };

    protected override IEnumerable<Kind> GetModifiers() => Enumerable.Empty<Kind>();

    protected override Production HandleList(Field field, string elementType) =>
        (elementType != "SyntaxToken" ? RuleReference(elementType) :
            field.Name == "Commas" ? new Production("','") :
            field.Name == "Modifiers" ? RuleReference("Modifier") :
            RuleReference(elementType))
                .Suffix(field.MinCount == 0 ? "*" : "+");

    protected override Production HandleTokenName(string tokenName) =>
        GetSyntaxKind(tokenName) is var kind && kind == SyntaxKind.None ? RuleReference("SyntaxToken") :
           SyntaxFacts.GetText(kind) is var text && text != "" ? new Production(text == "'" ? "'\\''" : $"'{text}'") :
           tokenName.StartsWith("EndOf") ? new Production("") :
           tokenName.StartsWith("Omitted") ? new Production("/* epsilon */") : RuleReference(tokenName);

    private SyntaxKind GetSyntaxKind(string name)
        => GetMembers<SyntaxKind>().Where(k => k.ToString() == name).SingleOrDefault();
}
