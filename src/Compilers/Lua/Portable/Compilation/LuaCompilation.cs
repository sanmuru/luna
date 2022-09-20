using System;
using System.Collections.Generic;
using System.Text;

namespace SamLu.CodeAnalysis.Lua;

partial class LuaCompilation
{
    /// <summary>
    /// 获取源代码语言名称。
    /// </summary>
    /// <value>
    /// 源代码语言名称。
    /// </value>
    public sealed override string Language => LanguageNames.Lua;

    /// <summary>
    /// 获取一个值，表示编译内容是否大小写敏感。
    /// </summary>
    /// <value>
    /// 若编译内容大小写敏感，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </value>
    public sealed override bool IsCaseSensitive => true;

}
