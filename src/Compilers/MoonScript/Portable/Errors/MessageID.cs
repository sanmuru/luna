﻿using System.Diagnostics;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.MoonScript;

internal enum MessageID
{
    None = 0,
    MessageBase = 1200,

    IDS_FeatureHexadecimalFloatConstant,
    IDS_FeatureBinaryExponent,

    // 预览功能
    IDS_FeatureMultiLineRawStringLiteral,
    IDS_FeatureMultiLineComment,
    IDS_FeatureFloorDivisionAssignmentOperator,
    IDS_FeatureBitwiseAndAssignmentOperator,
    IDS_FeatureBitwiseOrAssignmentOperator,
    IDS_FeatureExponentiationAssignmentOperator,
    IDS_FeatureBitwiseLeftShiftAssignmentOperator,
    IDS_FeatureBitwiseRightShiftAssignmentOperator,
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

        return feature switch
        {
            _ => throw ExceptionUtilities.UnexpectedValue(feature)
        };
    }
}
