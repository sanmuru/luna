using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SamLu.Lua;

public sealed class String : Object, IComparable, IComparable<String?>, IComparable<string>, IEquatable<String>, IEquatable<string>, ICloneable, IConvertible
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

    #region ICloneable
    public object Clone() => (String)this._value.Clone();
    #endregion

    #region IConvertible
    bool IConvertible.ToBoolean(IFormatProvider? provider) => ((IConvertible)this._value).ToBoolean(provider);

    sbyte IConvertible.ToSByte(IFormatProvider? provider) => ((IConvertible)this._value).ToSByte(provider);

    byte IConvertible.ToByte(IFormatProvider? provider) => ((IConvertible)this._value).ToByte(provider);

    short IConvertible.ToInt16(IFormatProvider? provider) => ((IConvertible)this._value).ToInt16(provider);

    ushort IConvertible.ToUInt16(IFormatProvider? provider) => ((IConvertible)this._value).ToUInt16(provider);

    int IConvertible.ToInt32(IFormatProvider? provider) => ((IConvertible)this._value).ToInt32(provider);

    uint IConvertible.ToUInt32(IFormatProvider? provider) => ((IConvertible)this._value).ToUInt32(provider);

    long IConvertible.ToInt64(IFormatProvider? provider) => ((IConvertible)this._value).ToInt64(provider);

    ulong IConvertible.ToUInt64(IFormatProvider? provider) => ((IConvertible)this._value).ToUInt64(provider);

    float IConvertible.ToSingle(IFormatProvider? provider) => ((IConvertible)this._value).ToSingle(provider);

    double IConvertible.ToDouble(IFormatProvider? provider) => ((IConvertible)this._value).ToDouble(provider);

    decimal IConvertible.ToDecimal(IFormatProvider? provider) => ((IConvertible)this._value).ToDecimal(provider);

    DateTime IConvertible.ToDateTime(IFormatProvider? provider) => ((IConvertible)this._value).ToDateTime(provider);

    char IConvertible.ToChar(IFormatProvider? provider) => ((IConvertible)this._value).ToChar(provider);

    public string ToString(IFormatProvider? provider) => this._value.ToString(provider);

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => this.ChangeType(conversionType);

    TypeCode IConvertible.GetTypeCode() => this._value.GetTypeCode();
    #endregion

    #region Object
    internal static Table? s_mt;

    protected internal override Table? Metatable
    {
        get => String.s_mt;
        set => String.s_mt = value;
    }

    public override int GetHashCode() => this._value.GetHashCode();

    public override TypeInfo GetTypeInfo() => TypeInfo.String;

    /// <inheritdoc/>
    /// <exception cref="InvalidCastException"><paramref name="type"/> 不是能接受的转换目标类型。</exception>
    public override object ChangeType(Type type)
    {
        if (typeof(Object).IsAssignableFrom(type) && type.IsAssignableFrom(typeof(String))) return this;
        else if (type == typeof(string)) return this._value;
        else return ((IConvertible)this._value).ToType(type, null);
    }
    #endregion

    #region 操作符
    public static bool operator ==(String left, String right) => left.Equals(right);
    public static bool operator !=(String left, String right) => !left.Equals(right);

    public static implicit operator String(string value) => new(value);
    public static implicit operator string(String luaString) => luaString._value;
    #endregion
}
