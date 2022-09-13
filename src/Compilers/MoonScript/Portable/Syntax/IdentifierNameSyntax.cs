using SamLu.CodeAnalysis.MoonScript.Syntax;

namespace SamLu.CodeAnalysis.MoonScript;

partial class SyntaxFactory
{
    /// <summary>
    /// Creates an IdentifierNameSyntax node.
    /// </summary>
    /// <param name="name">The identifier name.</param>
    public static IdentifierNameSyntax IdentifierName(string name)
    {
        return IdentifierName(SyntaxFactory.Identifier(name));
    }
}
