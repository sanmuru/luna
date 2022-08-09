using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace SamLu.Lua;

public class Table : Object
{
    protected internal Table? mt;
    protected internal readonly Dictionary<Object, Object?> dictionary = new();

    public override Object? this[Object? key]
    {
        get
        {
            // 首先在字典中查找键对应的值。
            Object? value;
            if (key is null) // 字典不支持空值。
                value = null;
            else
                value = Table.RawGet(this, key);

            // 若字典中不存在键对应的值，再通过元表查找键对应的值。
            value ??= this.GetMetatableIndex(key);

            return value;
        }
        set
        {
            if (key is null) throw new ArgumentNullException(nameof(value)); // 字典不支持空值。

            // 若字典中存在指定键；或虽不存在指定键，但通过元表设置指定键的值失败。
            if (this.dictionary.ContainsKey(key) || !this.SetMetatableIndex(key, value))
                // 直接设置字典中键对应的值。
                Table.RawSet(this, key, value);
        }
    }

    protected internal override Table? Metatable
    {
        get => this.mt;
        set => this.mt = value;
    }

    /// <summary>
    /// 设置此实例的元表。
    /// </summary>
    /// <param name="table">一个 Lua 表，作为元表设置到此实例。</param>
    /// <returns>设置成功后的此实例的元表。</returns>
    public virtual Table? SetMetatable(Table? table)
    {
        var metatable = this.Metatable;
        if (metatable is not null)
        {
            var mvMetatable = Table.RawGet(metatable, Lua.Metatable.Metavalue_Metatable);
            if (mvMetatable is not null)
                throw new LuaException(mvMetatable);
        }

        this.mt = table;
        return this.mt;
    }

    /// <summary>
    /// 获取表中指定键对应的值，忽略元表的影响。
    /// </summary>
    /// <param name="table">值所在的表。</param>
    /// <param name="index">要查询的键。</param>
    /// <returns></returns>
    public static Object? RawGet(Table table, Object index)
    {
        if (table is null) throw new ArgumentNullException(nameof(table));
        if (index is null) throw new ArgumentNullException(nameof(index));

        if (table.dictionary.TryGetValue(index, out var value))
            return value;
        else
            return null;
    }

    /// <summary>
    /// 获取表中指定键对应的值，忽略元表的影响。
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Table RawSet(Table table, Object index, Object? value)
    {
        if (table is null) throw new ArgumentNullException(nameof(table));
        if (index is null) throw new ArgumentNullException(nameof(index));

        if (table.dictionary.ContainsKey(index))
        {
            if (value is null)
                table.dictionary.Remove(index);
            else
                table.dictionary[index] = value;
        }
        else
            table.dictionary.Add(index, value);

        return table;
    }

    public override int GetHashCode() => this.dictionary.GetHashCode();

    public override TypeInfo GetTypeInfo() => TypeInfo.Table;

    /// <inheritdoc/>
    /// <exception cref="InvalidCastException"><paramref name="type"/> 不是能接受的转换目标类型。</exception>
    public override object ChangeType(Type type)
    {
        if (typeof(Object).IsAssignableFrom(type) && type.IsAssignableFrom(this.GetType())) return this;
        else throw new InvalidCastException();
    }

    public static Object? Next(Table table, Object? index = null)
    {
        if (table is null) throw new ArgumentNullException(nameof(table));

        var etor = table.dictionary.Keys.GetEnumerator();
        if (index is null)
        {
            if (etor.MoveNext())
                return etor.Current;
            else
                return null;
        }

        var comparer = table.dictionary.Comparer;
        while (etor.MoveNext())
        {
            if (comparer.Equals(etor.Current, index))
            {
                if (etor.MoveNext())
                    return etor.Current;
                else
                    return null;
            }
        }

        return null;
    }

    [return: Tuple(3)]
    [return:
        TupleItemType(0, typeof(Function)),
        TupleItemTypeSameAs(1, "t"),
        TupleItemType(2, typeof(long))
    ]
    public static MultiReturns IndexedPair(Table t)
    {
        if (t is null) throw new ArgumentNullException(nameof(t));

        return new(
            (Function)new Func<Table, long, long>((_, i) => i++),
            t,
            (Number)0L
        );
    }

    [return: Tuple(2)]
    [return:
        TupleItemType(0, typeof(Func<Table, Object?, Object?>)),
        TupleItemTypeSameAs(1, "t")
    ]
    public static MultiReturns Pair(Table t)
    {
        if (t is null) throw new ArgumentNullException(nameof(t));

        return new(
            (Function)new Func<Table, Object?, Object?>(Table.Next),
            t
        );
    }
}
