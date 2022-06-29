using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua;

internal enum MessageID
{
    None = 0,
    MessageBase = 1200,

    IDS_FeatureHexadecimalFloatConstant,
    IDS_FeatureBinaryExponent
}

internal static partial class MessageIDExtensions
{
    internal static partial string? RequiredFeature(this MessageID feature) =>
        feature switch
        {
            _ => null
        };

    internal static partial LanguageVersion RequiredVersion(this MessageID feature)
    {
        Debug.Assert(MessageIDExtensions.RequiredFeature(feature) is null);

        // 在语言分析器中检查特性的支持版本。
        return feature switch
        {
            // Lua 5.2的特性
            MessageID.IDS_FeatureHexadecimalFloatConstant or
            MessageID.IDS_FeatureBinaryExponent
                => LanguageVersion.Lua5_2,

            _ => throw ExceptionUtilities.UnexpectedValue(feature)
        };
    }
}
