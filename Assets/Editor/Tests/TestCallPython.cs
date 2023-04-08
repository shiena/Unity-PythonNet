using NUnit.Framework;
using Python.Runtime;

namespace UnityPython.Tests
{
    public class TestCallPython
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            PythonLifeCycle.Initialize();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            PythonLifeCycle.Shutdown();
        }

        [Test]
        public void CallBuiltinPasses()
        {
            using (Py.GIL())
            {
                using dynamic platformModule = Py.Import("platform");
                var version = (string)platformModule.python_version();
                Assert.AreEqual("3.11.3", version);
            }
        }
    }
}