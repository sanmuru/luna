using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SamLu.Lua;

public sealed class String : Object, IComparable, IComparable<String>, IComparable<string>, IEquatable<String>, IEquatable<string>
{
    private readonly string _value;

    private String(string value) => this._value = value;

    static String()
    {
#warning 设置字符串数据类型的默认元表。
    }

    public int CompareTo(object? obj) => obj switch
    {
        String str => this.CompareTo(str),
        _ => this._value.CompareTo(obj)
    };

    public int CompareTo(String? other) => other switch
    {
        null => this._value.CompareTo(null),
        _ => this._value.CompareTo(other._value)
    };

    public int CompareTo(string? other) => other switch
    {
        null => this._value.CompareTo(null),
        _ => this._value.CompareTo(other)
    };

    public override bool Equals(object? obj) => obj switch
    {
        String str => this.Equals(str),
        _ => this._value.Equals(obj)
    };

    public bool Equals(String? other) => other switch
    {
        null => this._value.Equals(null),
        _ => this._value.Equals(other._value)
    };

    public bool Equals(string? other) => other switch
    {
        null => this._value.Equals(null),
        _ => this._value.Equals(other)
    };

    public override int GetHashCode() => this._value.GetHashCode();

    #region Object
    internal static Table? s_mt;

    protected internal override Table? Metatable
    {
        get => String.s_mt;
        set => String.s_mt = value;
    }

    public override TypeInfo GetTypeInfo() => TypeInfo.String;
    #endregion

    #region 操作符
    public static bool operator ==(String left, String right) => left.Equals(right);
    public static bool operator !=(String left, String right) => !left.Equals(right);

    public static implicit operator String(string value) => new(value);
    public static implicit operator string(String luaString) => luaString._value;
    #endregion
}
