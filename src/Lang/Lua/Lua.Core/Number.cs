using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SamLu.Lua;

public abstract class Number : Object, IComparable, IComparable<Number>, IEquatable<Number>
{
    public int CompareTo(object? obj)
    {
        var number = this.ConvertComparandFrom(obj);

        return this.CompareToCore(number);
    }

    public int CompareTo(Number? other)
    {
        if (other is null) throw new ComparisonNotSupportedException(TypeInfo.TypeInfo_Number, TypeInfo.TypeInfo_Nil);

        return this.CompareToCore(other);
    }

    protected abstract int CompareToCore(Number other);

    public sealed override bool Equals(object? obj)
    {
        Number number = this.ConvertComparandFrom(obj);

        return this.EqualsCore(number);
    }

    public bool Equals(Number? other)
    {
        if (other is null) throw new ComparisonNotSupportedException(TypeInfo.TypeInfo_Number, TypeInfo.TypeInfo_Nil);

        return this.EqualsCore(other);
    }

    protected abstract bool EqualsCore(Number other);

    protected virtual Number ConvertComparandFrom(object? obj) => obj switch
    {
        byte or sbyte or short or ushort or int or uint or long => (Integer)(long)obj,
        ulong unsigned => unsigned switch
        {
            <= long.MaxValue => (Integer)unchecked((long)unsigned),
            _ => (LargeInteger)(ulong)obj
        },
        float or double => (Real)(double)obj,
        decimal => (DecimalReal)(decimal)obj,

        Number => (Number)obj,

        null => throw new ComparisonNotSupportedException(TypeInfo.TypeInfo_Number, TypeInfo.TypeInfo_Nil),
        _ => throw new ComparisonNotSupportedException(this.GetTypeInfo(), new TypeInfo(obj?.GetType()))
    };

    public abstract override int GetHashCode();

    #region Object
    internal static Table? s_mt;

    protected internal override Table? Metatable
    {
        get => Number.s_mt;
        set => Number.s_mt = value;
    }

    public override TypeInfo GetTypeInfo() => TypeInfo.Number;
    #endregion

    [CLSCompliant(false)] public static implicit operator Number(sbyte value) => (Integer)value;
    public static implicit operator Number(byte value) => (Integer)value;
    public static implicit operator Number(short value) => (Integer)value;
    [CLSCompliant(false)] public static implicit operator Number(ushort value) => (Integer)value;
    public static implicit operator Number(int value) => (Integer)value;
    [CLSCompliant(false)] public static implicit operator Number(uint value) => (Integer)value;
    public static implicit operator Number(long value) => (Integer)value;
    [CLSCompliant(false)] public static implicit operator Number(ulong value) => (LargeInteger)value;
    public static implicit operator Number(float value) => (Real)value;
    public static implicit operator Number(double value) => (Real)value;
    public static implicit operator Number(decimal value) => (DecimalReal)value;

    public static bool operator <(Number left, Number right) => left.CompareTo(right) < 0;
    public static bool operator >(Number left, Number right) => left.CompareTo(right) > 0;
    public static bool operator <=(Number left, Number right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Number left, Number right) => left.CompareTo(right) >= 0;
    public static bool operator ==(Number left, Number right) => left.Equals(right);
    public static bool operator !=(Number left, Number right) => !left.Equals(right);
}
