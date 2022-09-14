namespace SamLu.CodeAnalysis.Lua.Symbols;

/// <summary>
/// 此枚举描述了所有能提供诊断的组件的类型。
/// 在读取诊断列表前我们需要说明这些组件的类型。
/// </summary>
[Flags]
internal enum CompletionPart
{
    // 对所有符号：
    None = 0,

    // 对函数符号：
    Parameters = 1 << 2,
    Type = 1 << 3,

    All = (1 << 18) - 1,

}
