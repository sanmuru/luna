using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SamLu.Lua;

#pragma warning disable CS0659, CS0661
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
#pragma warning restore CS8767

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
    public static implicit operator String(Number value) => value.ToString() ?? string.Empty;
    #endregion

    #region Lua 操作符函数
    public static Number Addition(Number left, Number right)
    {
        Number doubles(double l, double r) => l + r;

        Number decimals(decimal l, decimal r)
        {
            try { return l + r; }
            catch (OverflowException) { return doubles((double)l, (double)r); } // 算数溢出，转型为更大范围的双精度浮点数运算。
        }

        Number eitherUnsigned(long l, ulong r)
        {
            if (l >= 0 && (ulong)l <= (ulong.MaxValue - r)) return (ulong)l + r;
            else
            {
                decimal result = (decimal)l + (decimal)r;
                if (result > ulong.MaxValue) return result;
                else if (result > long.MaxValue) return (ulong)result;
                else return (long)result;
            };
        }

        Number bothUnsigned(ulong l, ulong r)
        {
            if (l <= (ulong.MaxValue - r)) return l + r;
            else
            {
                decimal result = (decimal)l + (decimal)r;
                if (result > ulong.MaxValue) return result;
                else return (ulong)result;
            };
        }

        Number bothSigned(long l, long r)
        {
            if (l < 0 && r < 0)
            {
                long result = unchecked(l + r);
                if (result >= 0) return (decimal)l + (decimal)r;
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

    public static Number Subtraction(Number left, Number right)
    {
        Number doubles(double l, double r) => l + r;

        Number decimals(decimal l, decimal r)
        {
            try { return l - r; }
            catch (OverflowException) { return doubles((double)l, (double)r); } // 算数溢出，转型为更大范围的双精度浮点数运算。
        }

        Number eitherUnsigned(decimal l, decimal r)
        {
            decimal result = l - r;
            if (result > ulong.MaxValue) return result;
            else if (result < long.MinValue) return result;
            else if (result > long.MaxValue) return (ulong)result;
            else return (long)result;
        }

        Number bothUnsigned(ulong l, ulong r)
        {
            if (l < r) return -(long)(r - l);
            else return l - r;
        }

        Number bothSigned(long l, long r)
        {
            if (l < 0 && r > 0)
            {
                long result = unchecked(l - r);
                if (result >= 0) return (decimal)l - (decimal)r;
                else return result;
            }
            else if (l > 0 && r < 0)
            {
                long result = unchecked(l - r);
                if (result < 0) return (decimal)l - (decimal)r;
                else return result;
            }
            else return l + r;
        }

        return (left, right) switch
        {
            (Real, _) or (_, Real) => doubles((double)left.ChangeType(typeof(double)), (double)left.ChangeType(typeof(double))),
            (DecimalReal, _) or (_, DecimalReal) => decimals((decimal)left.ChangeType(typeof(decimal)), (decimal)left.ChangeType(typeof(decimal))),
            (Integer, LargeInteger) or (LargeInteger, Integer) => eitherUnsigned((decimal)left.ChangeType(typeof(decimal)), (decimal)right.ChangeType(typeof(decimal))),
            (LargeInteger, LargeInteger) => bothUnsigned((ulong)(LargeInteger)left, (ulong)(LargeInteger)right),
            (Integer, Integer) => bothSigned((long)(Integer)left, (long)(Integer)right),
            _ => throw new NotSupportedException()
        };
    }

    public static Number Multiplication(Number left, Number right)
    {
        Number doubles(double l, double r) => l * r;

        Number decimals(decimal l, decimal r)
        {
            try { return l * r; }
            catch (OverflowException) { return doubles((double)l, (double)r); } // 算数溢出，转型为更大范围的双精度浮点数运算。
        }

        Number eitherUnsigned(decimal l, decimal r)
        {
            Number numResult = decimals(l, r);
            if (numResult is not DecimalReal decimalRealResult) return numResult; // 超出了十进制浮点数运算的范围。

            decimal result = (decimal)decimalRealResult;
            if (result > ulong.MaxValue) return decimalRealResult;
            else if (result < long.MinValue) return decimalRealResult;
            else if (result > long.MaxValue) return (ulong)result;
            else return (long)result;
        }

        Number bothUnsigned(ulong l, ulong r)
        {
            Number numResult = decimals(l, r);
            if (numResult is not DecimalReal decimalRealResult) return numResult; // 超出了十进制浮点数运算的范围。

            decimal result = (decimal)decimalRealResult;
            if (result > ulong.MaxValue) return decimalRealResult;
            else return (ulong)result;
        }

        Number bothSigned(long l, long r)
        {
            Number numResult = decimals(l, r);
            if (numResult is not DecimalReal decimalRealResult) return numResult; // 超出了十进制浮点数运算的范围。

            decimal result = (decimal)decimalRealResult;
            if (result > ulong.MaxValue) return decimalRealResult;
            else if (result < long.MinValue) return decimalRealResult;
            else if (result > long.MaxValue) return (ulong)result;
            else return (long)result;
        }

        return (left, right) switch
        {
            (Real, _) or (_, Real) => doubles((double)left.ChangeType(typeof(double)), (double)left.ChangeType(typeof(double))),
            (DecimalReal, _) or (_, DecimalReal) => decimals((decimal)left.ChangeType(typeof(decimal)), (decimal)left.ChangeType(typeof(decimal))),
            (Integer, LargeInteger) or (LargeInteger, Integer) => eitherUnsigned((decimal)left.ChangeType(typeof(decimal)), (decimal)right.ChangeType(typeof(decimal))),
            (LargeInteger, LargeInteger) => bothUnsigned((ulong)(LargeInteger)left, (ulong)(LargeInteger)right),
            (Integer, Integer) => bothSigned((long)(Integer)left, (long)(Integer)right),
            _ => throw new NotSupportedException()
        };
    }

    public static Number FloatDivision(Number left, Number right)
    {
        return (double)left.ChangeType(typeof(double)) / (double)right.ChangeType(typeof(double));
    }

    public static Number FloorDivision(Number left, Number right)
    {
        double result = Math.Floor((double)left.ChangeType(typeof(double)) / (double)right.ChangeType(typeof(double)));

        if (result > (double)decimal.MaxValue) return result;
        else if (result < (double)decimal.MinValue) return result;
        else if (result > ulong.MaxValue) return decimal.Round((decimal)result);
        else if (result < long.MinValue) return decimal.Round((decimal)result);
        else if (result > long.MaxValue) return (ulong)result;
        else return (long)result;
    }

    public static Number Modulo(Number left, Number right)
    {
        Number doubles(double l, double r) => l % r;

        Number decimals(decimal l, decimal r) => l % r;

        Number eitherUnsigned(decimal l, decimal r)
        {
            decimal result = l % r;
            if (result > long.MaxValue) return (ulong)result;
            else return (long)result;
        }

        Number bothUnsigned(ulong l, ulong r) => l % r;

        Number bothSigned(long l, long r) => l % r;

        return (left, right) switch
        {
            (Real, _) or (_, Real) => doubles((double)left.ChangeType(typeof(double)), (double)left.ChangeType(typeof(double))),
            (DecimalReal, _) or (_, DecimalReal) => decimals((decimal)left.ChangeType(typeof(decimal)), (decimal)left.ChangeType(typeof(decimal))),
            (Integer, LargeInteger) or (LargeInteger, Integer) => eitherUnsigned((decimal)left.ChangeType(typeof(decimal)), (decimal)right.ChangeType(typeof(decimal))),
            (LargeInteger, LargeInteger) => bothUnsigned((ulong)(LargeInteger)left, (ulong)(LargeInteger)right),
            (Integer, Integer) => bothSigned((long)(Integer)left, (long)(Integer)right),
            _ => throw new NotSupportedException()
        };
    }

    public static Number Exponentiation(Number left, Number right)
    {
        return Math.Pow((double)left.ChangeType(typeof(double)), (double)right.ChangeType(typeof(double)));
    }

    public static Number Minus(Number value)
    {
        return value switch
        {
            Real real => -(double)real,
            DecimalReal decimalReal => -(decimal)decimalReal,
            Integer integer => (long)integer switch
            {
                long.MinValue => (ulong)long.MaxValue + 1UL,
                _ => -(long)integer
            },
            LargeInteger largeInteger => (ulong)largeInteger switch
            {
                <= long.MaxValue => -(long)(ulong)largeInteger,
                _ => -(decimal)largeInteger
            },
            _ => throw new NotSupportedException()
        };
    }

    public static bool Equality(Number left, Number right) => left.Equals(right);

    public static bool Inequality(Number left, Number right) => !left.Equals(right);

    public static bool LessThan(Number left, Number right) => left.CompareTo(right) < 0;

    public static bool GreaterThan(Number left, Number right) => left.CompareTo(right) > 0;

    public static bool LessOrEqual(Number left, Number right) => left.CompareTo(right) <= 0;

    public static bool GreaterOrEqual(Number left, Number right) => left.CompareTo(right) >= 0;
    #endregion

    public static Number operator +(Number value) => value;
    public static Number operator -(Number value) => Number.Minus(value);
    public static Number operator ++(Number value) => value + 1;
    public static Number operator --(Number value) => value - 1;
    public static Number operator +(Number left, Number right) => Number.Addition(left, right);
    public static Number operator -(Number left, Number right) => Number.Subtraction(left, right);
    public static Number operator *(Number left, Number right) => Number.Multiplication(left, right);
    public static Number operator /(Number left, Number right) => Number.FloatDivision(left, right);
    public static Number operator %(Number left, Number right) => Number.Modulo(left, right);
    public static bool operator ==(Number left, Number right) => Number.Equality(left, right);
    public static bool operator !=(Number left, Number right) => Number.Inequality(left, right);
    public static bool operator <(Number left, Number right) => Number.LessThan(left, right);
    public static bool operator >(Number left, Number right) => Number.GreaterThan(left, right);
    public static bool operator <=(Number left, Number right) => Number.LessOrEqual(left, right);
    public static bool operator >=(Number left, Number right) => Number.GreaterOrEqual(left, right);
    #endregion
}
