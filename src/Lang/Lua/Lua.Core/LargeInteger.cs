namespace SamLu.Lua;

internal sealed class LargeInteger : Number
{
    private readonly ulong _value;

    private LargeInteger(ulong value) => this._value = value;

    public static implicit operator LargeInteger(ulong value) => new(value);
    public static explicit operator ulong(LargeInteger value) => value._value;
}
