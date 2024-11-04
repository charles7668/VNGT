namespace GameManager.Models
{
    public class Result : Result<object>
    {
        private Result(object? value, bool isSuccess, string message, Exception? exception)
            : base(value, isSuccess, message, exception)
        {
        }

        public static Result Ok()
        {
            return new Result(null, true, string.Empty, null);
        }

        public static new Result Failure(string error, Exception? exception = null)
        {
            return new Result(null, false, error, exception);
        }
    }

    public class Result<TValue>
    {
        protected Result(TValue? value, bool isSuccess, string message, Exception? exception)
        {
            Value = value;
            Success = isSuccess;
            Message = message;
            Exception = exception;
        }

        public TValue? Value { get; private set; }

        public bool Success { get; private set; }

        public string Message { get; private set; }

        public Exception? Exception { get; private set; }

        public static Result<TValue> Ok(TValue value)
        {
            return new Result<TValue>(value, true, string.Empty, null);
        }

        public static Result<TValue> Failure(string error, Exception? exception = null)
        {
            return new Result<TValue>(default, false, error, exception);
        }
    }
}