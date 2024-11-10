using JetBrains.Annotations;

namespace Helper
{
    public static class ExceptionHelper
    {
        /// <summary>
        /// Execute action with handle all exception
        /// </summary>
        /// <param name="action"></param>
        /// <param name="exceptionHandler"></param>
        /// <param name="finallyHandler"></param>
        /// <returns>Return true if exception occur , otherwise false</returns>
        [UsedImplicitly]
        public static bool ExecuteWithExceptionHandling(Action action, Action<Exception>? exceptionHandler = null,
            Action? finallyHandler = null)
        {
            return ExecuteWithExceptionHandling<Exception>(action, exceptionHandler, finallyHandler);
        }

        /// <summary>
        /// Execute action with handle specific exception
        /// </summary>
        /// <param name="action"></param>
        /// <param name="exceptionHandler"></param>
        /// <param name="finallyHandler"></param>
        /// <typeparam name="T">Type of exception</typeparam>
        /// <returns>Return true if exception occur , otherwise false</returns>
        [UsedImplicitly]
        public static bool ExecuteWithExceptionHandling<T>(Action action, Action<T>? exceptionHandler = null,
            Action? finallyHandler = null)
            where T : Exception
        {
            try
            {
                action();
                return false;
            }
            catch (T ex)
            {
                exceptionHandler?.Invoke(ex);
                return true;
            }
            finally
            {
                finallyHandler?.Invoke();
            }
        }

        /// <summary>
        /// Execute action with handle specific exceptions
        /// </summary>
        /// <param name="action"></param>
        /// <param name="exceptionHandler1"></param>
        /// <param name="exceptionHandler2"></param>
        /// <param name="finallyHandler"></param>
        /// <typeparam name="TException1">type of exception 1</typeparam>
        /// <typeparam name="TException2">type of exception 2</typeparam>
        /// <returns>Return true if exception occur , otherwise false</returns>
        [UsedImplicitly]
        public static bool ExecuteWithExceptionHandling<TException1, TException2>(Action action,
            Action<TException1>? exceptionHandler1 = null, Action<TException2>? exceptionHandler2 = null,
            Action? finallyHandler = null)
            where TException1 : Exception
            where TException2 : Exception
        {
            try
            {
                action();
                return false;
            }
            catch (TException1 ex)
            {
                exceptionHandler1?.Invoke(ex);
                return true;
            }
            catch (TException2 ex)
            {
                exceptionHandler2?.Invoke(ex);
                return true;
            }
            finally
            {
                finallyHandler?.Invoke();
            }
        }

        [UsedImplicitly]
        public static async Task<bool> ExecuteWithExceptionHandlingAsync<T>(
            Func<Task> action,
            Func<T, Task>? exceptionHandler = null,
            Func<Task>? finallyHandler = null) where T : Exception
        {
            try
            {
                await action();
                return false;
            }
            catch (T ex)
            {
                if (exceptionHandler != null)
                {
                    await exceptionHandler(ex);
                }

                return true;
            }
            finally
            {
                if (finallyHandler != null)
                {
                    await finallyHandler();
                }
            }
        }

        [UsedImplicitly]
        public static async Task<bool> ExecuteWithExceptionHandlingAsync<TException1, TException2>(
            Func<Task> action,
            Func<TException1, Task>? exceptionHandler1 = null,
            Func<TException2, Task>? exceptionHandler2 = null,
            Func<Task>? finallyHandler = null)
            where TException1 : Exception
            where TException2 : Exception
        {
            try
            {
                await action();
                return true;
            }
            catch (TException1 ex)
            {
                if (exceptionHandler1 != null)
                {
                    await exceptionHandler1(ex);
                }

                return false;
            }
            catch (TException2 ex)
            {
                if (exceptionHandler2 != null)
                {
                    await exceptionHandler2(ex);
                }

                return false;
            }
            finally
            {
                if (finallyHandler != null)
                {
                    await finallyHandler();
                }
            }
        }

        [UsedImplicitly]
        public static async Task<bool> ExecuteWithExceptionHandlingAsync(
            Func<Task> action,
            Func<Exception, Task>? exceptionHandler = null,
            Func<Task>? finallyHandler = null)
        {
            return await ExecuteWithExceptionHandlingAsync<Exception>(action, exceptionHandler, finallyHandler);
        }
    }
}