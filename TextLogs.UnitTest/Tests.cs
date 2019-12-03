using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextLogs;
using System.Diagnostics;

namespace TextLogs.UnitTest
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void Write_Tests()
        {
            Log log = new Log();
            log.Write("Test");
        }

        [TestMethod]
        public void AutoDelete_Tests()
        {
            Log log = new Log(true, 1, 1);
            log.Write("Test Done");
        }
    }
}
