using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua
{
    public enum SyntaxKind : ushort
    {
        None = 0,
        List = GreenNode.ListKind,

        // declarations
        CompilationUnit = 8840,
    }
}
