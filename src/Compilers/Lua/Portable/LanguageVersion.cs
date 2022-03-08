using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua
{
    public enum LanguageVersion
    {
        /// <summary>
        /// Lua语言版本1.0。
        /// </summary>
        Lua1 = 1,
        /// <summary>
        /// Lua语言版本1.1。
        /// </summary>
        Lua1_1,
        /// <summary>
        /// Lua语言版本2.1。
        /// </summary>
        Lua2_1,
        /// <summary>
        /// Lua语言版本2.2。
        /// </summary>
        Lua2_2,
        /// <summary>
        /// Lua语言版本2.3。
        /// </summary>
        Lua2_3,
        /// <summary>
        /// Lua语言版本2.4。
        /// </summary>
        Lua2_4,
        /// <summary>
        /// Lua语言版本2.5。
        /// </summary>
        Lua2_5,
        /// <summary>
        /// Lua语言版本3.0。
        /// </summary>
        Lua3,
        /// <summary>
        /// Lua语言版本3.1。
        /// </summary>
        Lua3_1,
        /// <summary>
        /// Lua语言版本3.2。
        /// </summary>
        Lua3_2,
        /// <summary>
        /// Lua语言版本4.0。
        /// </summary>
        Lua4,
        /// <summary>
        /// Lua语言版本5.0。
        /// </summary>
        Lua5,
        /// <summary>
        /// Lua语言版本5.1。
        /// </summary>
        Lua5_1,
        /// <summary>
        /// Lua语言版本5.2。
        /// </summary>
        Lua5_2,
        /// <summary>
        /// Lua语言版本5.3。
        /// </summary>
        Lua5_3,
        /// <summary>
        /// Lua语言版本5.4。
        /// </summary>
        Lua5_4,

        /// <summary>
        /// 支持的最新的主要版本。
        /// </summary>
        LatestMajor = int.MaxValue - 2,
        /// <summary>
        /// 下一个预览版本。
        /// </summary>
        Preview = int.MaxValue - 1,
        /// <summary>
        /// 支持的最新的版本。
        /// </summary>
        Latest = int.MaxValue,
        /// <summary>
        /// 默认的语言版本，也就是支持的最新的版本。
        /// </summary>
        Default = 0
    }

    internal static class LanguageVersionExtensionsInternal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsValid(this LanguageVersion value) => Enum.IsDefined(typeof(LanguageVersion), value);

        internal static ErrorCode GetErrorCode(this LanguageVersion version)
        {
            throw new NotSupportedException();
        }
    }

    internal class LuaRequiredLanguageVersion : RequiredLanguageVersion
    {
        internal LanguageVersion Version { get; init; }

        internal LuaRequiredLanguageVersion(LanguageVersion version) => this.Version = version;

        public override string ToString() => this.Version.ToDisplayString();
    }

    public static class LanguageVersionFacts
    {
        internal const LanguageVersion LuaNext = LanguageVersion.Preview;

        public static string ToDisplayString(this LanguageVersion version) =>
            version switch
            {
                LanguageVersion.Lua1 => "1.0",
                LanguageVersion.Lua1_1 => "1.1",
                LanguageVersion.Lua2_1 => "2.1",
                LanguageVersion.Lua2_2 => "2.2",
                LanguageVersion.Lua2_3 => "2.3",
                LanguageVersion.Lua2_4 => "2.4",
                LanguageVersion.Lua2_5 => "2.5",
                LanguageVersion.Lua3 => "3.0",
                LanguageVersion.Lua3_1 => "3.1",
                LanguageVersion.Lua3_2 => "3.2",
                LanguageVersion.Lua4 => "4.0",
                LanguageVersion.Lua5 => "5.0",
                LanguageVersion.Lua5_1 => "5.1",
                LanguageVersion.Lua5_2 => "5.2",
                LanguageVersion.Lua5_3 => "5.3",
                LanguageVersion.Lua5_4 => "5.4",
                LanguageVersion.Default => "default",
                LanguageVersion.Latest => "latest",
                LanguageVersion.LatestMajor => "latestmajor",
                LanguageVersion.Preview => "preview",
                _ => throw ExceptionUtilities.UnexpectedValue(version)
            };

#warning 未完成
    }
}
