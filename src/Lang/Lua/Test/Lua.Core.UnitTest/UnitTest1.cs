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
            Object obj = long.MinValue;
            obj = Userdata.Wrap(new UnitTest1());
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
