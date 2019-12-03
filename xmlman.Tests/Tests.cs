using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextLogs;
using System.IO;
using System.Diagnostics;

namespace xmlman.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void CreateFile_Tests()
        {
            string filename = "Config.xml";
            bool existsFile = false;
            Log log = new Log();
            Xml xml = new Xml(log: log, filename: filename);

            if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\{filename}"))
                existsFile = true;

            Assert.IsTrue(existsFile);
        }

        [TestMethod]
        public void CreateNode_Test()
        {
            Log log = new Log();
            Xml xml = new Xml(log: log);
            xml.CreateNode("hola", null);
            xml.CreateNode("hola2", null, "hola");
        }

        [TestMethod]
        public void ReadSingleNode_Test()
        {
            string text;
            Log log = new Log();
            Xml xml = new Xml(log: log);
            xml.CreateNode("Tag1", null);
            xml.CreateNode("Tag2", null, "Tag1");
            text = xml.ReadSingleNode("Tag2", "Tag1");
            Debug.Print(text);
            Assert.IsTrue(text == "ROLANDO MEDINA");
        }
    }
}
