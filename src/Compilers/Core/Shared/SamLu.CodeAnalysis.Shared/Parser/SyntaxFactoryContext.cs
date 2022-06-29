#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;
#endif

/// <summary>
/// 为构建语法节点提供上下文信息，有助于决定节点是否能在增量分析中重用。
/// </summary>
/// <remarks>
/// 在<see cref="SyntaxParser"/>外部应为只读（但由于性能原因并不强制限制）。
/// </remarks>
internal partial class SyntaxFactoryContext
{
    /* 此类中存放用于构建语法节点的必要的上下文信息。
     * 
     * 基于Lua的所有支持语言通用的字段放置于此文件；
     * 各语言特定的字段放置于其对应项目的同名文件中。
     */
#warning 需补充上下文信息字段。
}
