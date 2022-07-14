using System.Collections.Immutable;

namespace SamLu.CodeAnalysis.MoonScript;

partial class MoonScriptParseOptions
{
    /// <summary>
    /// 获取已定义的解析器符号的名称。
    /// </summary>
    /// <remarks>
    /// MoonScript.NET不支持预处理指令。
    /// </remarks>
    public override IEnumerable<string> PreprocessorSymbolNames => ImmutableArray<string>.Empty;
}
