namespace SavePatcher.Logs
{
    public sealed class LogCallbacks
    {
        public delegate void LogCallbackEventHandler(object sender, LogCallbackEventArgs e);

        public event LogCallbackEventHandler? LogInfoEvent;
        public event LogCallbackEventHandler? LogErrorEvent;
        public event LogCallbackEventHandler? LogWarningEvent;
        public event LogCallbackEventHandler? LogDebugEvent;
        public event LogCallbackEventHandler? LogFatalEvent;
        public event LogCallbackEventHandler? LogTraceEvent;

        public void OnLogInfo(object sender, string message)
        {
            LogInfoEvent?.Invoke(sender, new LogCallbackEventArgs(message));
        }

        public void OnLogError(object sender, string message)
        {
            LogErrorEvent?.Invoke(sender, new LogCallbackEventArgs(message));
        }

        public void OnLogWarning(object sender, string message)
        {
            LogWarningEvent?.Invoke(sender, new LogCallbackEventArgs(message));
        }

        public void OnLogDebug(object sender, string message)
        {
            LogDebugEvent?.Invoke(sender, new LogCallbackEventArgs(message));
        }

        public void OnLogFatal(object sender, string message)
        {
            LogFatalEvent?.Invoke(sender, new LogCallbackEventArgs(message));
        }

        public void OnLogTrace(object sender, string message)
        {
            LogTraceEvent?.Invoke(sender, new LogCallbackEventArgs(message));
        }

        public class LogCallbackEventArgs(string message) : EventArgs
        {
            public string Message { get; set; } = message;
        }
    }
}