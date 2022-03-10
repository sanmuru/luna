using System.Globalization;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#else
#error 不支持的语言
#endif
;

internal sealed partial class MessageProvider : CommonMessageProvider, IObjectWritable
{
    public static readonly MessageProvider Instance = new();

    static MessageProvider()
    {
        ObjectBinder.RegisterTypeReader(typeof(MessageProvider), r => MessageProvider.Instance);
    }

    private MessageProvider() { }

    bool IObjectWritable.ShouldReuseInSerialization => true;

    void IObjectWritable.WriteTo(ObjectWriter writer)
    {
        // 不进行写入操作，永远读取或反序列化为全局实例
    }

    public override DiagnosticSeverity GetSeverity(int code) => ErrorFacts.GetSeverity((ErrorCode)code);

    public override string LoadMessage(int code, CultureInfo? language) => ErrorFacts.GetMessage((ErrorCode)code, language);
}
