using SamLu.CodeAnalysis;

namespace SyntaxGenerator.Lua;

internal sealed class LuaSourceWriter : SourceWriter
{
    protected override string LanguageName => LanguageNames.Lua;

    protected override string RootNamespace => "SamLu.CodeAnalysis.Lua";

    public LuaSourceWriter(TextWriter writer, Tree tree, CancellationToken cancellationToken = default) : base(writer, tree, cancellationToken) { }
}
