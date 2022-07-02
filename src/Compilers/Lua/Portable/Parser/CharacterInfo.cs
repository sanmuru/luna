using System.Diagnostics.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua;

public static partial class SyntaxFacts
{
    /// <exception cref="ArgumentNullException"><paramref name="restChars"/>的值为<see langword="null"/>。</exception>
    public static partial bool IsNewLine(char firstChar, params char[] restChars)
    {
        if (restChars is null) throw new ArgumentNullException(nameof(restChars));

        if (restChars.Length == 0)
            return SyntaxFacts.IsNewLine(firstChar);
        else
            return firstChar == '\r' && restChars[0] == '\n';
    }


    public static partial bool IsIdentifierStartCharacter(char c) =>
        UnicodeCharacterUtilities.IsIdentifierStartCharacter(c);

    public static partial bool IsIdentifierPartCharacter(char c) =>
        UnicodeCharacterUtilities.IsIdentifierPartCharacter(c);

    public static partial bool IsValidIdentifier(string? name) =>
        UnicodeCharacterUtilities.IsValidIdentifier(name);
}
