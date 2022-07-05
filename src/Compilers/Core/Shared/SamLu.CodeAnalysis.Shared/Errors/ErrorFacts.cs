using System.Globalization;
using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;
#endif

#warning 未实现。
internal static partial class ErrorFacts
{
    internal static DiagnosticSeverity GetSeverity(ErrorCode code)
    {
        throw new NotImplementedException();
    }

    public static string GetMessage(MessageID code, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }

    public static string GetMessage(ErrorCode code, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}
