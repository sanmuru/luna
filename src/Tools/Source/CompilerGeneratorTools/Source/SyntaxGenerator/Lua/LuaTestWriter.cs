namespace SyntaxGenerator.Lua;

internal sealed class LuaTestWriter : TestWriter
{
    public LuaTestWriter(TextWriter writer, Tree tree, CancellationToken cancellationToken = default) : base(writer, tree, cancellationToken) { }

    protected override string RootNamespace => "SamLu.CodeAnalysis.Lua";
}
