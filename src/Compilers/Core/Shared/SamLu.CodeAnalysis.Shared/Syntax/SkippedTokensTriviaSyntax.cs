using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax;
#endif

public sealed partial class SkippedTokensTriviaSyntax : StructuredTriviaSyntax, ISkippedTokensTriviaSyntax
{
    #region 未实现
#warning 未实现。
    public SyntaxTokenList Tokens => throw new NotImplementedException();

    public SkippedTokensTriviaSyntax() : base(null, null, 0) { }

    public override TResult? Accept<TResult>(
#if LANG_LUA
        LuaSyntaxVisitor<TResult>
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxVisitor<TResult>
#endif
        visitor)
        where TResult : default
    {
        throw new NotImplementedException();
    }

    public override void Accept(
#if LANG_LUA
        LuaSyntaxVisitor
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxVisitor
#endif
        visitor)
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
