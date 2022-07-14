using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.MoonScript.Syntax;

public sealed partial class ChunkSyntax : MoonScriptSyntaxNode, ICompilationUnitSyntax
{
    #region 未实现
#warning 未实现。
    public SyntaxToken EndOfFileToken => throw new NotImplementedException();

    public ChunkSyntax() : base(null, null, 0) { }

    public override TResult? Accept<TResult>(MoonScriptSyntaxVisitor<TResult> visitor)
        where TResult : default
    {
        throw new NotImplementedException();
    }

    public override void Accept(MoonScriptSyntaxVisitor visitor)
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
