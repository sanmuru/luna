using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax;

public abstract partial class StructuredTriviaSyntax :
#if LANG_LUA
    LuaSyntaxNode
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxNode
#endif
    , IStructuredTriviaSyntax
{
    private SyntaxTrivia _parent;

    /// <summary>
    /// 获取上级语法琐碎内容。
    /// </summary>
    public override SyntaxTrivia ParentTrivia => this._parent;

    internal StructuredTriviaSyntax(
#if LANG_LUA
        InternalSyntax.LuaSyntaxNode green
#elif LANG_MOONSCRIPT
        InternalSyntax.MoonScriptSyntaxNode green
#endif
        , SyntaxNode? parent, int position) : base(green, position, parent?.SyntaxTree) => Debug.Assert(parent is null || position >= 0);

    /// <summary>
    /// 从语法琐碎内容中创建一个结构化语法节点的实例。
    /// </summary>
    /// <param name="trivia">提供必要信息的语法琐碎内容。</param>
    /// <returns>包含信息的结构化语法节点。</returns>
    internal static StructuredTriviaSyntax Create(SyntaxTrivia trivia)
    {
        var node = trivia.RequiredUnderlyingNode;
        var parent = trivia.Token.Parent;
        var position = trivia.Position;
        var red = (StructuredTriviaSyntax)node.CreateRed(parent, position);
        red._parent = trivia;
        return red;
    }
}
