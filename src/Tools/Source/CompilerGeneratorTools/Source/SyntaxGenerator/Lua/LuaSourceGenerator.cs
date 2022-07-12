using Microsoft.CodeAnalysis;

namespace SyntaxGenerator.Lua;

[Generator]
internal sealed class LuaSourceGenerator : SourceGenerator
{
    private protected override SourceWriter CreateSourceWriter(TextWriter writer, Tree tree, CancellationToken cancellationToken) => new LuaSourceWriter(writer, tree, cancellationToken);
}
