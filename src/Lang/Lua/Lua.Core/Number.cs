using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SamLu.Lua;

public abstract class Number : Object, IComparable, IComparable<Number>, IEquatable<Number>
{
#pragma warning disable CS8767
    public int CompareTo(object obj) => this.CompareToCore(this.ConvertComparandFrom(obj));

    public int CompareTo(Number other) =>
        other is null
            ? throw new ComparisonNotSupportedException(TypeInfo.TypeInfo_Number, TypeInfo.TypeInfo_Nil)
            : this.CompareToCore(other);
#pragma warning restore CS8767

    protected abstract int CompareToCore(Number other);

#pragma warning disable CS8765
    public sealed override bool Equals(object obj) => this.EqualsCore(this.ConvertComparandFrom(obj));
#pragma warning restore CS8765

#pragma warning disable CS8767
    public bool Equals(Number other) =>
        other is null
            ? throw new ComparisonNotSupportedException(TypeInfo.TypeInfo_Number, TypeInfo.TypeInfo_Nil)
            : this.EqualsCore(other);
#pragma warning restore CS8765, CS8767

    protected abstract bool EqualsCore(Number other);

    protected virtual Number ConvertComparandFrom(object obj) => obj switch
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

    #region Object
    internal static Table? s_mt;

    protected internal override Table? Metatable
    {
        get => Number.s_mt;
        set => Number.s_mt = value;
    }

    public override TypeInfo GetTypeInfo() => TypeInfo.Number;
    #endregion

    #region 操作符
    #region 转型操作符
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
    #endregion

    #region Lua 操作符函数
    public static Number Addition(Number left, Number right)
    {
        Number doubles(double l, double r)
        {
            if (l < 0 && r < 0)
            {
                double result = unchecked(l + r);
                if (result > 0) return double.NegativeInfinity;
                else return result;
            }
            else if (l > 0 && r > 0)
            {
                double result = unchecked(l + r);
                if (result < 0) return double.PositiveInfinity;
                else return result;
            }
            else
                return l + r; // 符号相反，相加不会溢出。
        }

        Number decimals(decimal l, decimal r)
        {
            if (l < 0 && r < 0)
            {
                decimal result = unchecked(l + r);
                if (result > 0) return double.NegativeInfinity;
                else return result;
            }
            else if (l > 0 && r > 0)
            {
                decimal result = unchecked(l + r);
                if (result < 0) return double.PositiveInfinity;
                else return result;
            }
            else
                return l + r; // 符号相反，相加不会溢出。
        }

        Number eitherUnsigned(long l, ulong r)
        {
            if (l < 0) return unchecked(r - (ulong)(-l));
            else if ((ulong)l <= (ulong.MaxValue - r)) return (ulong)l + r;
            else return (decimal)l + (decimal)r;
        }

        Number bothUnsigned(ulong l, ulong r)
        {
            if (l <= (ulong.MaxValue - r)) return l + r;
            else return (decimal)l + (decimal)r;
        }

        Number bothSigned(long l, long r)
        {
            if (l < 0 && r < 0)
            {
                long result = unchecked(l + r);
                if (result > 0) return (decimal)l + (decimal)r;
                else return result;
            }
            else if (l > 0 && r > 0)
            {
                long result = unchecked(l + r);
                if (result < 0) return (decimal)l + (decimal)r;
                else return result;
            }
            else return l + r;
        }

        return (left, right) switch
        {
            (Real, _) or (_, Real) => doubles((double)left.ChangeType(typeof(double)), (double)left.ChangeType(typeof(double))),
            (DecimalReal, _) or (_, DecimalReal) => decimals((decimal)left.ChangeType(typeof(decimal)), (decimal)left.ChangeType(typeof(decimal))),
            (Integer, LargeInteger) => eitherUnsigned((long)(Integer)left, (ulong)(LargeInteger)right),
            (LargeInteger, Integer) => eitherUnsigned((long)(Integer)right, (ulong)(LargeInteger)left),
            (LargeInteger, LargeInteger) => bothUnsigned((ulong)(LargeInteger)left, (ulong)(LargeInteger)right),
            (Integer, Integer) => bothSigned((long)(Integer)left, (long)(Integer)right),
            _ => throw new NotSupportedException()
        };
    }
    #endregion

    public static Number operator +(Number value) => value;
    public static Number operator -(Number value) => value switch
    {
        Integer integer => (Integer)(-(long)integer),
        LargeInteger largeInteger => (DecimalReal)(-(decimal)(ulong)largeInteger),
        Real real => (Real)(-(double)real),
        DecimalReal decimalReal => (DecimalReal)(-(decimal)decimalReal),
        _ => throw new NotSupportedException()
    };
    public static Number operator !(Number value) => null;
    public static Number operator ~(Number value) => null;
    public static Number operator +(Number left, Number right) => Number.Addition(left, right);

    public static bool operator <(Number left, Number right) => left.CompareTo(right) < 0;
    public static bool operator >(Number left, Number right) => left.CompareTo(right) > 0;
    public static bool operator <=(Number left, Number right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Number left, Number right) => left.CompareTo(right) >= 0;
    public static bool operator ==(Number left, Number right) => left.Equals(right);
    public static bool operator !=(Number left, Number right) => !left.Equals(right);
    #endregion
}
