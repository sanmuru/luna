using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua;

partial class LuaSemanticModel
{
    /// <inheritdoc cref="LuaCompilation.Language"/>
    public sealed override string Language => LanguageNames.Lua;

#warning 未实现
    public partial ISymbol GetDeclaredSymbol(LuaSyntaxNode node, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
