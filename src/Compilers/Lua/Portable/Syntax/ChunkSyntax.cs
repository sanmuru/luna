using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua.Syntax;

public sealed partial class ChunkSyntax : LuaSyntaxNode, ICompilationUnitSyntax
{
    #region 未实现
#warning 未实现。
    public SyntaxToken EndOfFileToken => throw new NotImplementedException();

    public ChunkSyntax() : base(null, null, 0) { }

    public override TResult? Accept<TResult>(LuaSyntaxVisitor<TResult> visitor)
        where TResult : default
    {
        throw new NotImplementedException();
    }

    public override void Accept(LuaSyntaxVisitor visitor)
    {
        throw new NotImplementedException();
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        throw new NotImplementedException();
    }

    internal override SyntaxNode? GetNodeSlot(int slot)
    {
        throw new NotImplementedException();
    }

    internal SyntaxNode CreateRed()
    {
        throw new NotImplementedException();
    }
    #endregion
}
