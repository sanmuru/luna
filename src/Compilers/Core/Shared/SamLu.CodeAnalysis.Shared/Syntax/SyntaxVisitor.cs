using Microsoft.CodeAnalysis;

#if LANG_LUA
using SamLu.CodeAnalysis.Lua.Syntax;
namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
using SamLu.CodeAnalysis.Lua.MoonScript;
namespace SamLu.CodeAnalysis.MoonScript;
#endif

#if LANG_LUA
/// <summary>
/// 表示每个时刻仅访问和处理一个<see cref="LuaSyntaxNode"/>的访问者。
/// </summary>
/// <inheritdoc/>
#elif LANG_MOONSCRIPT
/// <summary>
/// 表示每个时刻仅访问和处理一个<see cref="MoonScriptSyntaxNode"/>的访问者。
/// </summary>
/// <inheritdoc/>
#endif
public abstract partial class
#if LANG_LUA
     LuaSyntaxVisitor<TResult> : CommonSyntaxVisitor<TResult, LuaSyntaxNode>
#elif LANG_MOONSCRIPT
     MoonScriptSyntaxVisitor<TResult> : CommonSyntaxVisitor<TResult, MoonScriptSyntaxNode>
#endif
{
    public override TResult? Visit(
#if LANG_LUA
        LuaSyntaxNode?
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode?
#endif
        node) =>
        (node is not null) ?
            node.Accept(this) :
            // 兜底值。
            default;

    public override TResult? DefaultVisit(
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        node) => default;
}

#if LANG_LUA
/// <summary>
/// 表示每个时刻仅访问和处理一个<see cref="LuaSyntaxNode"/>的访问者。
/// </summary>
#elif LANG_MOONSCRIPT
/// <summary>
/// 表示每个时刻仅访问和处理一个<see cref="MoonScriptSyntaxNode"/>的访问者。
/// </summary>
#endif
public abstract partial class
#if LANG_LUA
     LuaSyntaxVisitor : CommonSyntaxVisitor<LuaSyntaxNode>
#elif LANG_MOONSCRIPT
     MoonScriptSyntaxVisitor : CommonSyntaxVisitor<MoonScriptSyntaxNode>
#endif
{
    public override void Visit(
#if LANG_LUA
        LuaSyntaxNode?
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode?
#endif
        node)
    {
        if (node is not null)
            node.Accept(this);
    }

    public override void DefaultVisit(
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        node)
    { }
}
