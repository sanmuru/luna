namespace SamLu.Lua;

#pragma warning disable CA1067
internal sealed class DecimalReal : Number, IComparable<DecimalReal>, IComparable<decimal>, IEquatable<DecimalReal>, IEquatable<decimal>, IConvertible, IFormattable
{
    private readonly decimal _value;

    private DecimalReal(decimal value) => this._value = value;

    protected override int CompareToCore(Number other) => other switch
    {
        DecimalReal decimalReal => this._value.CompareTo(decimalReal._value),
        Integer integer => this._value.CompareTo((long)integer),
        LargeInteger largeInteger => this._value.CompareTo((ulong)largeInteger),
        Real real => this._value.CompareTo((double)real),
        _ => -other.CompareTo(this)
    };

#pragma warning disable CS8767
    public int CompareTo(DecimalReal other) => other is null
        ? throw new ComparisonNotSupportedException(TypeInfo.TypeInfo_Number, TypeInfo.TypeInfo_Nil)
        : this._value.CompareTo(other._value);
#pragma warning restore CS8767

    public int CompareTo(decimal other) => this._value.CompareTo(other);

    protected override bool EqualsCore(Number other) => other switch
    {
        DecimalReal decimalReal => this._value.Equals(decimalReal._value),
        Integer integer => this._value.Equals((long)integer),
        LargeInteger largeInteger => this._value.Equals((ulong)largeInteger),
        Real real => this._value.Equals((double)real),
        _ => other.Equals(this)
    };

#pragma warning disable CS8767
    public bool Equals(DecimalReal other) => other is null
        ? throw new ComparisonNotSupportedException(TypeInfo.TypeInfo_Number, TypeInfo.TypeInfo_Nil)
        : this._value.Equals(other._value);
#pragma warning restore CS8767

    public bool Equals(decimal other) => this._value.Equals(other);

    public override int GetHashCode() => this._value.GetHashCode();

    /// <inheritdoc/>
    /// <exception cref="InvalidCastException"><paramref name="type"/> 不是能接受的转换目标类型。</exception>
    public override object ChangeType(Type type)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));

        if (typeof(Object).IsAssignableFrom(type) && type.IsAssignableFrom(typeof(DecimalReal))) return this;
        else if (type == typeof(Real)) return (Real)(double)this._value;
        else if (type == typeof(Integer)) return (Integer)(long)this._value;
        else if (type == typeof(LargeInteger)) return (LargeInteger)(ulong)this._value;
        else return ((IConvertible)this._value).ToType(type, null);
    }

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

    #region IFormattable
    public string ToString(string? format) => this._value.ToString(format);

    public string ToString(string? format, IFormatProvider? formatProvider) => this._value.ToString(format, formatProvider);
    #endregion

    #region 操作符
    public static implicit operator DecimalReal(decimal value) => new(value);
    public static explicit operator decimal(DecimalReal value) => value._value;
    #endregion
}
