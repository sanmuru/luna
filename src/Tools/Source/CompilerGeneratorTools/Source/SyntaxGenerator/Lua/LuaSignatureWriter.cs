namespace SyntaxGenerator.Lua;

internal sealed class LuaSignatureWriter : SignatureWriter
{
    protected override string RootNamespace => "SamLu.CodeAnalysis.Lua";

    public LuaSignatureWriter(TextWriter writer, Tree tree) : base(writer, tree) { }
}
