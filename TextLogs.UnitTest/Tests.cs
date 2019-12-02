using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
