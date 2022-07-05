using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax;

public sealed partial class SkippedTokensTriviaSyntax : StructuredTriviaSyntax, ISkippedTokensTriviaSyntax
{
    #region 未实现
#warning 未实现。
    public SyntaxTokenList Tokens => throw new NotImplementedException();

    public SkippedTokensTriviaSyntax() : base(null, null, 0) { }

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
    #endregion
}
