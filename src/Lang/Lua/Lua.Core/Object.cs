using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace SamLu.Lua;

/// <summary>
/// 提供 Lua 的所有此实例的基类。此类必须被继承，不可直接实例化。
/// </summary>
public abstract partial class Object : IDynamicMetaObjectProvider
{
    /// <summary>
    /// 获取或设置此实例中特定键对应的值。
    /// </summary>
    /// <param name="key">查找使用的键。</param>
    /// <returns>此实例中 <paramref name="key"/> 对应的值。</returns>
    public virtual Object? this[Object? key]
    {
        get => this.GetMetatableIndex(key);
        set => this.SetMetatableIndex(key, value);
    }

    /// <summary>
    /// 获取或设置此实例的元表。
    /// </summary>
    protected internal abstract Table? Metatable { get; set; }

    /// <summary>
    /// 获取一个值，指示此示例是否可以被调用。
    /// </summary>
    public virtual bool IsCallable => this.GetMetavalue(Lua.Metatable.Metavalue_CallOperation)?.IsCallable == true;

    /// <summary>
    /// 获取此实例的元表中特定建对应的值。
    /// </summary>
    /// <param name="key">查找使用的键。</param>
    /// <returns>此实例的元表中 <paramref name="key"/> 对应的值。</returns>
    /// <remarks>
    /// 此方法会查找元值 <c>__index</c> 。
    /// 如果元值为 <see langword="null"/>，则表示不存在 <paramref name="key"/> 对应的值;
    /// 如果元值为一个 <see cref="Function"/>，则调用以获取结果；
    /// 否则调用其索引器获取结果。
    /// </remarks>
    protected internal Object? GetMetatableIndex(Object? key)
    {
        var mvIndex = this.GetMetavalue(Lua.Metatable.Metavalue_IndexingAccessOperation);
        if (mvIndex is null)
            return null;
        else if (mvIndex is Function func)
            return func.Invoke(this, key)[0];
        else
            return mvIndex[key];
    }

    /// <summary>
    /// 设置此实例的元表中特定建对应的值。
    /// </summary>
    /// <param name="key">查找使用的键。</param>
    /// <param name="value">要设置给此实例的元表中 <paramref name="key"/> 的值。</param>
    /// <returns>若为 <see langword="true"/>，表示成功设置；若为 <see langword="false"/>，表示未成功设置。</returns>
    /// <remarks>
    /// 此方法会查找元值 <c>__newindex</c>。
    /// 如果元值为 <see langword="null"/>，则返回 <see langword="false"/>;
    /// 如果元值为一个 <see cref="Function"/>，则调用以设置 <paramref name="value"/>，最后返回 <see langword="true"/>;
    /// 否则调用其索引器设置 <paramref name="value"/>，最后返回 <see langword="true"/>。
    /// </remarks>
    protected internal bool SetMetatableIndex(Object? key, Object? value)
    {
        var mvNewIndex = this.GetMetavalue(Lua.Metatable.Metavalue_IndexingAssignOperation);
        if (mvNewIndex is null)
            return false;
        else if (mvNewIndex is Function func)
        {
            func.Invoke(this, key, value);
            return true;
        }
        else
        {
            mvNewIndex[key] = value;
            return true;
        }
    }

    /// <summary>
    /// 获取此实例的元表，若存在元值 <c>__metatable</c>，则返回其值。
    /// </summary>
    /// <returns>此实例的元表，或元值 <c>__metatable</c> 的值。</returns>
    public virtual Object? GetMetatable()
    {
        var metatable = this.Metatable;
        if (metatable is null) return null;
        else
        {
            var mvMetatable = Table.RawGet(metatable, Lua.Metatable.Metavalue_Metatable);
            if (mvMetatable is not null) return mvMetatable;
            else return metatable;
        }
    }

    /// <summary>
    /// 获取此实例的元值。
    /// </summary>
    /// <param name="index">获取元值的字符串键。</param>
    /// <returns>此实例的元表中 <paramref name="index"/> 对应的元值</returns>
    protected internal Object? GetMetavalue(String index)
    {
        if (index is null) throw new ArgumentNullException(nameof(index));

        if (this.GetMetatable() is not Table mt)
            return null;
        else
            return Table.RawGet(mt, index);
    }

    public abstract override int GetHashCode();

    public abstract TypeInfo GetTypeInfo();

    /// <summary>
    /// 调用此示例。
    /// </summary>
    /// <param name="args">传入的参数。</param>
    /// <returns>调用的返回值。</returns>
    public virtual MultiReturns Invoke(params Object?[] args)
    {
        var mvCall = this.GetMetavalue(Lua.Metatable.Metavalue_CallOperation);

        if (mvCall is not Function) throw new InvalidInvocationException(mvCall);

        Function func = (Function)mvCall;
        Object?[] newArgs = new Object?[args.Length + 1];
        newArgs[0] = this;
        args.CopyTo(newArgs, 1);
        return func.Invoke(newArgs);
    }

    /// <summary>
    /// 返回一个特定类型的对象，其值与此实例相等。
    /// </summary>
    /// <param name="type">要返回的目标类型</param>
    /// <returns>一个类型为 <paramref name="type"/> 的对象，其值与此实例相等。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> 的值为 <see langword="null"/>。</exception>
    public abstract object ChangeType(Type type);

    /// <summary>
    /// 尝试将此实例转型为一个特定类型的对象，返回一个值，指示转型是否成功。
    /// </summary>
    /// <param name="type">要转换的目标类型。</param>
    /// <param name="result">一个类型为 <paramref name="type"/> 的对象，其值与此实例相等。</param>
    /// <returns>若为 <see langword="true"/> 时，表示转型成功；若为 <see langword="false"/> 时，表示转型失败。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> 的值为 <see langword="null"/>。</exception>
    public bool TryChangeType(Type type, out object result)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));

        try
        {
            result = this.ChangeType(type);
            return true;
        }
        catch
        {
#pragma warning disable CS8625
            result = null;
#pragma warning restore CS8625
            return false;
        }
    }

    #region DynamicObject
    DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new ObjectDynamicMetaObject(parameter, this);
    #endregion

    public static implicit operator Object(bool value) => (Boolean)value;
    [CLSCompliant(false)] public static implicit operator Object(sbyte value) => (Number)value;
    public static implicit operator Object(byte value) => (Number)value;
    public static implicit operator Object(short value) => (Number)value;
    [CLSCompliant(false)] public static implicit operator Object(ushort value) => (Number)value;
    public static implicit operator Object(int value) => (Number)value;
    [CLSCompliant(false)] public static implicit operator Object(uint value) => (Number)value;
    public static implicit operator Object(long value) => (Number)value;
    [CLSCompliant(false)] public static implicit operator Object(ulong value) => (Number)value;
    public static implicit operator Object(float value) => (Number)value;
    public static implicit operator Object(double value) => (Number)value;
    public static implicit operator Object(decimal value) => (Number)value;
    public static implicit operator Object(string value) => (String)value;
    public static implicit operator Object(Delegate value) => (Function)value;

    public static bool operator true(Object? value) => value is not null;
    public static bool operator false(Object? value) => value is null;

    /// <summary>
    /// 将一个 <see cref="object"/> 对象转型为 Lua 环境的 <see cref="Object"/> 对象。
    /// </summary>
    public static Object? ConvertFrom(object? value) => value switch
    {
        null => null,
        bool => (Object)(bool)value,
        sbyte => (Object)(sbyte)value,
        byte => (Object)(byte)value,
        short => (Object)(short)value,
        ushort => (Object)(ushort)value,
        int => (Object)(int)value,
        uint => (Object)(uint)value,
        long => (Object)(long)value,
        ulong => (Object)(ulong)value,
        float => (Object)(float)value,
        double => (Object)(double)value,
        decimal => (Object)(decimal)value,
        string => (Object)(string)value,
        Delegate => (Object)(Delegate)value,
        _ => Userdata.Wrap(value)
    };
}
