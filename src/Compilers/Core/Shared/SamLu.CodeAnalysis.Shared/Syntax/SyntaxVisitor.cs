#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;
#endif

#if LANG_LUA
/// <summary>
/// 表示Lua语法的访问者基类。访问者每次访问和处理一个<see cref="LuaSyntaxNode"/>节点并产生类型为<typeparamref name="TResult"/>的结果。
/// </summary>
/// <inheritdoc/>
#elif LANG_MOONSCRIPT
/// <summary>
/// 表示MoonScript语法的访问者基类。访问者每次访问和处理一个<see cref="MoonScriptSyntaxNode"/>节点并产生类型为<typeparamref name="TResult"/>的结果。
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
    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override TResult? DefaultVisit(
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        node) => default;
}

#if LANG_LUA
/// <summary>
/// 表示Lua语法的访问者基类。访问者每次访问和处理一个<see cref="LuaSyntaxNode"/>节点。
/// </summary>
/// <inheritdoc/>
#elif LANG_MOONSCRIPT
/// <summary>
/// 表示MoonScript语法的访问者基类。访问者每次访问和处理一个<see cref="MoonScriptSyntaxNode"/>节点。
/// </summary>
/// <inheritdoc/>
#endif
public abstract partial class
#if LANG_LUA
     LuaSyntaxVisitor : CommonSyntaxVisitor<LuaSyntaxNode>
#elif LANG_MOONSCRIPT
     MoonScriptSyntaxVisitor : CommonSyntaxVisitor<MoonScriptSyntaxNode>
#endif
{
    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override void DefaultVisit(
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        node)
    { }
}
