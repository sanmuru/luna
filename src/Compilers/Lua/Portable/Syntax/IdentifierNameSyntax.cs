using SamLu.CodeAnalysis.Lua.Syntax;

namespace SamLu.CodeAnalysis.Lua;

partial class SyntaxFactory
{
    /// <summary>
    /// Creates an IdentifierNameSyntax node.
    /// </summary>
    /// <param name="name">The identifier name.</param>
    public static IdentifierNameSyntax IdentifierName(string name)
    {
        return SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(name));
    }
}
