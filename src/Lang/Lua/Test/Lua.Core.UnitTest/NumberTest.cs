using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SamLu.Lua.UnitTest;

[TestClass]
public class NumberTest
{
    [TestMethod]
    public void ConvertTypeTest()
    {
        Random random = new Random();
        T[] getRandomNumbers<T>(int count, Func<T> random, T min, T max)
        {
            T[] nums = new T[count + 2];
            nums[0] = min;
            for (int i = 1; i <= count + 1; i++)
                nums[i] = random();
            nums[nums.Length - 1] = max;
            return nums;
        }

        foreach (sbyte _sbyte in getRandomNumbers(256, () => (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue), sbyte.MinValue, sbyte.MaxValue))
        {
            Number n = _sbyte;
            Assert.IsInstanceOfType(n, typeof(Integer));
            Assert.AreEqual(_sbyte, (long)(Integer)n);
            Assert.AreEqual(_sbyte, (sbyte)n.ChangeType(typeof(sbyte)));
        }

        foreach (byte _byte in getRandomNumbers(256, () => (byte)random.Next(byte.MinValue, byte.MaxValue), byte.MinValue, byte.MaxValue))
        {
            Number n = _byte;
            Assert.IsInstanceOfType(n, typeof(Integer));
            Assert.AreEqual(_byte, (long)(Integer)n);
            Assert.AreEqual(_byte, (byte)n.ChangeType(typeof(byte)));
        }

        foreach (short _short in getRandomNumbers(256, () => (short)random.Next(short.MinValue, short.MaxValue), short.MinValue, short.MaxValue))
        {
            Number n = _short;
            Assert.IsInstanceOfType(n, typeof(Integer));
            Assert.AreEqual(_short, (long)(Integer)n);
            Assert.AreEqual(_short, (short)n.ChangeType(typeof(short)));
        }

        foreach (ushort _ushort in getRandomNumbers(256, () => (ushort)random.Next(ushort.MinValue, ushort.MaxValue), ushort.MinValue, ushort.MaxValue))
        {
            Number n = _ushort;
            Assert.IsInstanceOfType(n, typeof(Integer));
            Assert.AreEqual(_ushort, (long)(Integer)n);
            Assert.AreEqual(_ushort, (ushort)n.ChangeType(typeof(ushort)));
        }

        foreach (int _int in getRandomNumbers(256, () => random.Next(int.MinValue, int.MaxValue), int.MinValue, int.MaxValue))
        {
            Number n = _int;
            Assert.IsInstanceOfType(n, typeof(Integer));
            Assert.AreEqual(_int, (long)(Integer)n);
            Assert.AreEqual(_int, (int)n.ChangeType(typeof(int)));
        }

        foreach (uint _uint in getRandomNumbers(256, () => unchecked((uint)random.Next(int.MinValue, int.MaxValue)), uint.MinValue, uint.MaxValue))
        {
            Number n = _uint;
            Assert.IsInstanceOfType(n, typeof(Integer));
            Assert.AreEqual(_uint, (long)(Integer)n);
            Assert.AreEqual(_uint, (uint)n.ChangeType(typeof(uint)));
        }

        foreach (long _long in getRandomNumbers(256, () =>

#if NET472
            unchecked((long)random.Next(int.MinValue, int.MaxValue) + (long)random.Next(int.MinValue, int.MaxValue))
#elif NET6_0
            random.NextInt64(long.MinValue, long.MaxValue)
#endif
        , long.MinValue, long.MaxValue))
        {
            Number n = _long;
            Assert.IsInstanceOfType(n, typeof(Integer));
            Assert.AreEqual(_long, (long)(Integer)n);
            Assert.AreEqual(_long, (long)n.ChangeType(typeof(long)));
        }

        foreach (ulong _ulong in getRandomNumbers(256, () =>

#if NET472
            unchecked((ulong)random.Next(int.MinValue, int.MaxValue) + (ulong)random.Next(int.MinValue, int.MaxValue))
#elif NET6_0
            unchecked((ulong)random.NextInt64(long.MinValue, long.MaxValue))
#endif
        , ulong.MinValue, ulong.MaxValue))
        {
            Number n = _ulong;
            Assert.IsInstanceOfType(n, typeof(LargeInteger));
            Assert.AreEqual(_ulong, (ulong)(LargeInteger)n);
            Assert.AreEqual(_ulong, (ulong)n.ChangeType(typeof(ulong)));
        }
    }

    [TestMethod]
    public void ArithmeticOperationTest()
    {

    }
}
