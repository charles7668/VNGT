namespace VNGTTranslator.Models
{
    /// <summary>
    /// Result class for handling detail return value
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    internal class Result<T>
    {
        private Result(T? value, bool isSuccess, string errorMessage)
        {
            Value = value;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public T? Value { get; set; }
        private bool IsSuccess { get; }
        public string ErrorMessage { get; set; }

        public static implicit operator bool(Result<T> result)
        {
            return result.IsSuccess;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value, true, string.Empty);
        }

        public static Result<T> Fail(string errorMessage)
        {
            return new Result<T>(default, false, errorMessage);
        }

        public static Result<T> Fail(T value, string errorMessage)
        {
            return new Result<T>(value, false, errorMessage);
        }
    }

    /// <summary>
    /// Result class for handling detail return value
    /// </summary>
    public class Result
    {
        private Result(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        private bool IsSuccess { get; }
        public string ErrorMessage { get; set; }

        public static implicit operator bool(Result result)
        {
            return result.IsSuccess;
        }

        public static Result Success()
        {
            return new Result(true, string.Empty);
        }

        public static Result Fail(string errorMessage)
        {
            return new Result(false, errorMessage);
        }
    }
}