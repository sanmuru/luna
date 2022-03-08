using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua
{
    public sealed class LuaParseOptions : ParseOptions, IEquatable<LuaParseOptions>
    {
        /// <summary>
        /// 默认解析选项。
        /// </summary>
        public static LuaParseOptions Default { get; } = new();
    }
}
