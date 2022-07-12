namespace SyntaxGenerator.Lua;

internal sealed class LuaSourceWriter : SourceWriter
{
    public LuaSourceWriter(TextWriter writer, Tree tree, CancellationToken cancellationToken = default) : base(writer, tree, cancellationToken) { }

    protected override string RootNamespace => "SamLu.CodeAnalysis.Lua";
}
