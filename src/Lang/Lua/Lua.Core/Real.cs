namespace SamLu.Lua;

#pragma warning disable CA1067, CS0660
internal sealed class Real : Number, IComparable<Real>, IComparable<double>, IEquatable<Real>, IEquatable<double>
#pragma warning restore CA1067, CS0660
{
    private readonly double _value;

    private Real(double value) => this._value = value;

    protected override int CompareToCore(Number other) => other switch
    {
        Real real => this._value.CompareTo(real._value),
        DecimalReal decimalReal => this._value.CompareTo((object)(decimal)decimalReal),
        Integer integer => this._value.CompareTo((object)(int)integer),
        LargeInteger largeInteger => this._value.CompareTo((object)(ulong)largeInteger),
        _ => -other.CompareTo(this._value)
    };

    public int CompareTo(Real other) => this._value.CompareTo(other._value);

    public int CompareTo(double other) => this._value.CompareTo(other);

    protected override bool EqualsCore(Number other) => other switch
    {
        Real real => this._value.Equals(real._value),
        DecimalReal decimalReal => this._value.Equals((object)(decimal)decimalReal),
        Integer integer => this._value.Equals((object)(int)integer),
        LargeInteger largeInteger => this._value.Equals((object)(ulong)largeInteger),
        _ => other.Equals(this._value)
    };

    public bool Equals(Real other) => this._value.Equals(other._value);

    public bool Equals(double other) => this._value.Equals(other);

    public override int GetHashCode() => this._value.GetHashCode();

    #region 操作符
    public static bool operator <(Real left!!, Real right!!) => left._value < right._value;
    public static bool operator >(Real left!!, Real right!!) => left._value > right._value;
    public static bool operator <=(Real left!!, Real right!!) => left._value <= right._value;
    public static bool operator >=(Real left!!, Real right!!) => left._value >= right._value;
    public static bool operator ==(Real left!!, Real right!!) => left._value == right._value;
    public static bool operator !=(Real left!!, Real right!!) => left._value != right._value;

    public static implicit operator Real(double value) => new(value);
    public static explicit operator double(Real value) => value._value;
    #endregion
}
