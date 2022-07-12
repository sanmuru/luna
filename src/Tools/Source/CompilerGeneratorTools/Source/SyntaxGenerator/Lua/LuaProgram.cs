namespace SyntaxGenerator.Lua;

internal sealed class LuaProgram : Program
{
    protected override string LanguageName => SamLu.CodeAnalysis.LanguageNames.Lua;

    protected override GrammarGenerator CreateGrammarGenerator() => new LuaGrammarGenerator();

    protected override SignatureWriter CreateSignatureWriter(TextWriter writer, Tree tree) => new LuaSignatureWriter(writer, tree);

    protected override SourceWriter CreateSourceWriter(TextWriter writer, Tree tree, CancellationToken cancellationToken) => new LuaSourceWriter(writer, tree, cancellationToken);

    protected override TestWriter CreateTestWriter(TextWriter writer, Tree tree, CancellationToken cancellationToken) => new LuaTestWriter(writer, tree, cancellationToken);
}
