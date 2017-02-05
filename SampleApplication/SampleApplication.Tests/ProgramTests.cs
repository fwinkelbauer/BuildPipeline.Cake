using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApplication.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void HelloWorldTest()
        {
            Assert.AreEqual("Hello World!", Program.HelloWorld());
        }
    }
}