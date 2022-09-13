namespace SamLu.Lua;

internal sealed class Vanilla : Userdata, IEquatable<Vanilla>
{
    private readonly object _value;

    public Vanilla(object value) => this._value = value ?? throw new ArgumentNullException(nameof(value));

    public override bool Equals(object? obj) => obj is Vanilla vanilla && this.Equals(vanilla);

    public bool Equals(Vanilla? other) => other is not null && this._value.Equals(other._value);

    public override int GetHashCode() => this._value.GetHashCode();

    #region Object
    internal Table? mt;

    protected internal override Table? Metatable
    {
        get => this.mt;
        set => this.mt = value;
    }

    public override TypeInfo GetTypeInfo() => new(this._value.GetType());

    /// <inheritdoc/>
    /// <exception cref="InvalidCastException"><paramref name="type"/> 不是能接受的转换目标类型。</exception>
    public override object ChangeType(Type type)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));

        if (typeof(Object).IsAssignableFrom(type) && type.IsAssignableFrom(typeof(Vanilla))) return this;
        else if (type.IsAssignableFrom(this._value.GetType())) return this._value;
        else if (this._value is IConvertible) return Convert.ChangeType(this._value, type);
        else throw new InvalidCastException();
    }
    #endregion
}
