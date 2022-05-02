using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SamLu.Lua;

namespace Lua.Core.UnitTest
{

    static class Program
    {
        static void Main()
        {
            Number n1, n2;
            n1 = ulong.MaxValue;
            n2 = ulong.MaxValue;
            Number n3 = n1 + n2;
        }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

        }
    }
}
