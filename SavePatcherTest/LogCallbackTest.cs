using SavePatcher.Logs;

namespace SavePatcherTest
{
    [TestClass]
    public class LogCallbackTest
    {
        [TestMethod]
        public void TestLogInfoCallback()
        {
            var logCallbacks = new LogCallbacks();
            string callbackText = string.Empty;
            object? callbackSender = null;
            logCallbacks.LogInfoEvent += (sender, e) =>
            {
                callbackSender = sender;
                callbackText = e.Message;
            };
            logCallbacks.OnLogInfo(this, "test");
            Assert.AreEqual(this, callbackSender);
            Assert.AreEqual("test", callbackText);
        }

        [TestMethod]
        public void TestLogErrorCallback()
        {
            var logCallbacks = new LogCallbacks();
            string callbackText = string.Empty;
            object? callbackSender = null;
            logCallbacks.LogErrorEvent += (sender, e) =>
            {
                callbackSender = sender;
                callbackText = e.Message;
            };
            logCallbacks.OnLogError(this, "test");
            Assert.AreEqual(this, callbackSender);
            Assert.AreEqual("test", callbackText);
        }

        [TestMethod]
        public void TestLogWarningCallback()
        {
            var logCallbacks = new LogCallbacks();
            string callbackText = string.Empty;
            object? callbackSender = null;
            logCallbacks.LogWarningEvent += (sender, e) =>
            {
                callbackSender = sender;
                callbackText = e.Message;
            };
            logCallbacks.OnLogWarning(this, "test");
            Assert.AreEqual(this, callbackSender);
            Assert.AreEqual("test", callbackText);
        }

        [TestMethod]
        public void TestLogFatalCallback()
        {
            var logCallbacks = new LogCallbacks();
            string callbackText = string.Empty;
            object? callbackSender = null;
            logCallbacks.LogFatalEvent += (sender, e) =>
            {
                callbackSender = sender;
                callbackText = e.Message;
            };
            logCallbacks.OnLogFatal(this, "test");
            Assert.AreEqual(this, callbackSender);
            Assert.AreEqual("test", callbackText);
        }

        [TestMethod]
        public void TestLogTraceCallback()
        {
            var logCallbacks = new LogCallbacks();
            string callbackText = string.Empty;
            object? callbackSender = null;
            logCallbacks.LogTraceEvent += (sender, e) =>
            {
                callbackSender = sender;
                callbackText = e.Message;
            };
            logCallbacks.OnLogTrace(this, "test");
            Assert.AreEqual(this, callbackSender);
            Assert.AreEqual("test", callbackText);
        }

        [TestMethod]
        public void TestLogDebugCallback()
        {
            var logCallbacks = new LogCallbacks();
            string callbackText = string.Empty;
            object? callbackSender = null;
            logCallbacks.LogDebugEvent += (sender, e) =>
            {
                callbackSender = sender;
                callbackText = e.Message;
            };
            logCallbacks.OnLogDebug(this, "test");
            Assert.AreEqual(this, callbackSender);
            Assert.AreEqual("test", callbackText);
        }
    }
}