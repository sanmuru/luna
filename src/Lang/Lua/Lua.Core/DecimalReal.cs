namespace SamLu.Lua;

internal sealed class DecimalReal : Number, IComparable<DecimalReal>, IComparable<decimal>, IEquatable<DecimalReal>, IEquatable<decimal>
{
    private readonly decimal _value;

    private DecimalReal(decimal value) => this._value = value;

    public static implicit operator DecimalReal(decimal value) => new(value);
    public static explicit operator decimal(DecimalReal value) => value._value;
}
