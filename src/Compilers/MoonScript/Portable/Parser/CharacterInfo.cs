﻿using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.MoonScript;

public static partial class SyntaxFacts
{
    public static partial bool IsWhiteSpace(char c) => CharacterInfo.IsWhiteSpace(c);

    public static partial bool IsNewLine(char c) => CharacterInfo.IsNewLine(c);

    public static partial bool IsNewLine(char firstChar, char secondChar) => CharacterInfo.IsNewLine(firstChar, secondChar);

    public static partial bool IsIdentifierStartCharacter(char c) =>
        UnicodeCharacterUtilities.IsIdentifierStartCharacter(c);

    public static partial bool IsIdentifierPartCharacter(char c) =>
        UnicodeCharacterUtilities.IsIdentifierPartCharacter(c);

    public static partial bool IsValidIdentifier(string? name) =>
        UnicodeCharacterUtilities.IsValidIdentifier(name);

    public static int WhiteSpaceIndent(char c) =>
        c switch
        {
            ' ' => 1,
            '\t' => 4,
            '\v' or
            '\f' => 1,
            '\u00A0' => 1,  // 无中断空格符（U+00A0）
            '\uFEFF' or     // 零宽无中断空格符（U+FEFF）
            '\u001A'        // 替换符（U+001A）
                => 0,
            > (char)255 =>
                CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator ? 1 : 0,
            _ => 0
        };
}
