namespace SavePatcher.Models
{
    public class Result : Result<object>
    {
        private Result(object? value, bool isSuccess, string message)
            : base(value, isSuccess, message)
        {
        }

        public static Result Ok()
        {
            return new Result(null, true, string.Empty);
        }

        public static new Result Failure(string error)
        {
            return new Result(null, false, error);
        }
    }

    public class Result<TValue>
    {
        protected Result(TValue? value, bool isSuccess, string message)
        {
            Value = value;
            Success = isSuccess;
            Message = message;
        }

        public TValue? Value { get; private set; }

        public bool Success { get; private set; }

        public string Message { get; private set; }

        public static Result<TValue> Ok(TValue value)
        {
            return new Result<TValue>(value, true, string.Empty);
        }

        public static Result<TValue> Failure(string error)
        {
            return new Result<TValue>(default, false, error);
        }
    }
}