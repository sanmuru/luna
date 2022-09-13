using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;

using ThisResource = LuaResources;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;

using ThisResource = MoonScriptResources;
#endif

#warning 未实现。
internal static partial class ErrorFacts
{
    private static string GetId(ErrorCode errorCode) => MessageProvider.Instance.GetIdForErrorCode((int)errorCode);

    internal static DiagnosticSeverity GetSeverity(ErrorCode code)
    {
        if (code == ErrorCode.Void)
            return InternalDiagnosticSeverity.Void;
        else if (code == ErrorCode.Unknown)
            return InternalDiagnosticSeverity.Unknown;
        else if (IsWarning(code))
            return DiagnosticSeverity.Warning;
        else if (IsInfo(code))
            return DiagnosticSeverity.Info;
        else if (IsHidden(code))
            return DiagnosticSeverity.Hidden;
        else
            return DiagnosticSeverity.Error;
    }

    private static ResourceManager? s_resourceManager;
    [MemberNotNull(nameof(ErrorFacts.s_resourceManager))]
    private static ResourceManager ResourceManager =>
        ErrorFacts.s_resourceManager ??= new(typeof(ThisResource).FullName, typeof(ErrorCode).GetTypeInfo().Assembly);

    public static string GetMessage(MessageID code, CultureInfo? culture)
    {
        var message = ErrorFacts.ResourceManager.GetString(code.ToString(), culture);
        Debug.Assert(!string.IsNullOrEmpty(message), code.ToString());
        return message;
    }

    public static string GetMessage(ErrorCode code, CultureInfo? culture)
    {
        var message = ErrorFacts.ResourceManager.GetString(code.ToString(), culture);
        Debug.Assert(!string.IsNullOrEmpty(message), code.ToString());
        return message;
    }
}
