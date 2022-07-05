using System.Globalization;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;
#endif

internal sealed partial class MessageProvider : IObjectWritable
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
}
