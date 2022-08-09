namespace SamLu.Lua;

public abstract class Userdata : Object
{
    /// <summary>
    /// 将一个 <see cref="object"/> 对象包装为能够在 Lua 环境中使用的 <see cref="Userdata"/> 对象。
    /// </summary>
    public static Userdata Wrap(object value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        return value is Userdata userdata ? userdata : new Vanilla(value);
    }

    public override TypeInfo GetTypeInfo() => TypeInfo.Userdata;
}
