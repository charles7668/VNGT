using SavePatcher.Models;

namespace SavePatcherTest
{
    [TestClass]
    public class ResultModelTest
    {
        [TestMethod]
        public void TestResultSuccess()
        {
            Result<int> test1 = Result<int>.Ok(1);
            Assert.IsTrue(test1.Success);
            Assert.AreEqual(1, test1.Value);
            Assert.AreEqual(string.Empty, test1.Message);
            Result<double> test2 = Result<double>.Ok(1.0);
            Assert.IsTrue(test2.Success);
            Assert.AreEqual(1.0, test2.Value);
            Assert.AreEqual(string.Empty, test2.Message);
            Result<string> test3 = Result<string>.Ok("test");
            Assert.IsTrue(test3.Success);
            Assert.AreEqual("test", test3.Value);
            Assert.AreEqual(string.Empty, test3.Message);
        }

        [TestMethod]
        public void TestFailure()
        {
            Result<int> test1 = Result<int>.Failure("test1");
            Assert.IsFalse(test1.Success);
            Assert.AreEqual(default, test1.Value);
            Assert.AreEqual("test1", test1.Message);
            Result<double> test2 = Result<double>.Failure("test2");
            Assert.IsFalse(test2.Success);
            Assert.AreEqual(default, test2.Value);
            Assert.AreEqual("test2", test2.Message);
            Result<string> test3 = Result<string>.Failure("test3");
            Assert.IsFalse(test3.Success);
            Assert.AreEqual(default, test3.Value);
            Assert.AreEqual("test3", test3.Message);
        }
    }
}