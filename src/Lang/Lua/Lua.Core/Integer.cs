using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SamLu.Lua;

#pragma warning disable CA1067
public sealed class Integer : Number, IComparable<Integer>, IComparable<long>, IEquatable<Integer>, IEquatable<long>
#pragma warning restore CA1067
{
    private readonly long _value;

    private Integer(long value) => this._value = value;

    protected override int CompareToCore(Number other) => other switch
    {
        Real real => this._value.CompareTo((object)(double)real),
        Integer integer => this.CompareTo(integer),
        _ => -other.CompareTo(this._value)
    };

    public int CompareTo(Integer? other) => other switch
    {
        null => this._value.CompareTo(null),
        _ => this._value.CompareTo(other._value)
    };

    public int CompareTo(long other) => this._value.CompareTo(other);

    protected override bool EqualsCore(Number other) => other switch
    {
        Real real => this._value.Equals((object)(double)real),
        Integer integer => this.Equals(integer),
        _ => other.Equals(this._value)
    };

    public bool Equals(Integer? other) => other switch
    {
        null => this._value.Equals(null),
        _ => this._value.Equals(other._value)
    };

    public bool Equals(long other) => this._value.Equals(other);

    public override int GetHashCode() => this._value.GetHashCode();

    [CLSCompliant(false)] public static implicit operator Integer(sbyte value) => new(value);
    public static implicit operator Integer(byte value) => new(value);
    public static implicit operator Integer(short value) => new(value);
    [CLSCompliant(false)] public static implicit operator Integer(ushort value) => new(value);
    public static implicit operator Integer(int value) => new(value);
    [CLSCompliant(false)] public static implicit operator Integer(uint value) => new(value);
    public static implicit operator Integer(long value) => new(value);

    public static explicit operator long(Integer value) => value._value;
}
