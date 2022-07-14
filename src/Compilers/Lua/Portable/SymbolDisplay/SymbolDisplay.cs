using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;
#endif

public static partial class SymbolDisplay
{
    public static partial string? FormatPrimitive(object obj, bool quoteStrings, bool useHexadecimalNumbers)
    {
        var options = ObjectDisplayOptions.EscapeNonPrintableCharacters;

        if (quoteStrings)
            options |= ObjectDisplayOptions.UseQuotes;

        if (useHexadecimalNumbers)
            options |= ObjectDisplayOptions.UseHexadecimalNumbers;

        return ObjectDisplay.FormatPrimitive(obj, options);
    }

    public static partial string FormatLiteral(string value, bool quoteStrings)
    {
        var options = ObjectDisplayOptions.EscapeNonPrintableCharacters;
        if (quoteStrings) options |= ObjectDisplayOptions.UseQuotes;

        return ObjectDisplay.FormatLiteral(value, options);
    }
}
