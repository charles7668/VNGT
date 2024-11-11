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

        /// <summary>
        /// Executes an asynchronous action with exception handling for a specific exception type.
        /// </summary>
        /// <typeparam name="T">The type of exception to handle.</typeparam>
        /// <param name="action">The asynchronous action to execute.</param>
        /// <param name="exceptionHandler">The asynchronous handler for the exception of type T.</param>
        /// <param name="finallyHandler">The asynchronous handler to execute in the finally block.</param>
        /// <returns>Returns true if an exception of type T occurs, otherwise false.</returns>
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

        /// <summary>
        /// Executes an asynchronous action with exception handling for two specific exception types.
        /// </summary>
        /// <typeparam name="TException1">The first type of exception to handle.</typeparam>
        /// <typeparam name="TException2">The second type of exception to handle.</typeparam>
        /// <param name="action">The asynchronous action to execute.</param>
        /// <param name="exceptionHandler1">The asynchronous handler for the first type of exception.</param>
        /// <param name="exceptionHandler2">The asynchronous handler for the second type of exception.</param>
        /// <param name="finallyHandler">The asynchronous handler to execute in the finally block.</param>
        /// <returns>Returns true if an exception of type TException1 or TException2 occurs, otherwise false.</returns>
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

        /// <summary>
        /// Executes an asynchronous action with exception handling for a general exception type.
        /// </summary>
        /// <param name="action">The asynchronous action to execute.</param>
        /// <param name="exceptionHandler">The asynchronous handler for the general exception.</param>
        /// <param name="finallyHandler">The asynchronous handler to execute in the finally block.</param>
        /// <returns>Returns true if an exception occurs, otherwise false.</returns>
        [UsedImplicitly]
        public static async Task<bool> ExecuteWithExceptionHandlingAsync(
            Func<Task> action,
            Func<Exception, Task>? exceptionHandler = null,
            Func<Task>? finallyHandler = null)
        {
            return await ExecuteWithExceptionHandlingAsync<Exception>(action, exceptionHandler, finallyHandler);
        }

        /// <summary>
        /// Executes an action with exception handling for three specific exception types.
        /// </summary>
        /// <typeparam name="TException1">The first type of exception to handle.</typeparam>
        /// <typeparam name="TException2">The second type of exception to handle.</typeparam>
        /// <typeparam name="TException3">The third type of exception to handle.</typeparam>
        /// <param name="action">The action to execute.</param>
        /// <param name="exceptionHandler1">The handler for the first type of exception.</param>
        /// <param name="exceptionHandler2">The handler for the second type of exception.</param>
        /// <param name="exceptionHandler3">The handler for the third type of exception.</param>
        /// <param name="finallyHandler">The handler to execute in the finally block.</param>
        /// <returns>Returns true if an exception of type TException1, TException2, or TException3 occurs, otherwise false.</returns>
        [UsedImplicitly]
        public static bool ExecuteWithExceptionHandling<TException1, TException2, TException3>(
            Action action,
            Action<TException1>? exceptionHandler1 = null,
            Action<TException2>? exceptionHandler2 = null,
            Action<TException3>? exceptionHandler3 = null,
            Action? finallyHandler = null)
            where TException1 : Exception
            where TException2 : Exception
            where TException3 : Exception
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
            catch (TException3 ex)
            {
                exceptionHandler3?.Invoke(ex);
                return true;
            }
            finally
            {
                finallyHandler?.Invoke();
            }
        }

        /// <summary>
        /// Executes an asynchronous action with exception handling for three specific exception types.
        /// </summary>
        /// <typeparam name="TException1">The first type of exception to handle.</typeparam>
        /// <typeparam name="TException2">The second type of exception to handle.</typeparam>
        /// <typeparam name="TException3">The third type of exception to handle.</typeparam>
        /// <param name="action">The asynchronous action to execute.</param>
        /// <param name="exceptionHandler1">The asynchronous handler for the first type of exception.</param>
        /// <param name="exceptionHandler2">The asynchronous handler for the second type of exception.</param>
        /// <param name="exceptionHandler3">The asynchronous handler for the third type of exception.</param>
        /// <param name="finallyHandler">The asynchronous handler to execute in the finally block.</param>
        /// <returns>Returns true if an exception of type TException1, TException2, or TException3 occurs, otherwise false.</returns>
        [UsedImplicitly]
        public static async Task<bool> ExecuteWithExceptionHandlingAsync<TException1, TException2, TException3>(
            Func<Task> action,
            Func<TException1, Task>? exceptionHandler1 = null,
            Func<TException2, Task>? exceptionHandler2 = null,
            Func<TException3, Task>? exceptionHandler3 = null,
            Func<Task>? finallyHandler = null)
            where TException1 : Exception
            where TException2 : Exception
            where TException3 : Exception
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
            catch (TException3 ex)
            {
                if (exceptionHandler3 != null)
                {
                    await exceptionHandler3(ex);
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
    }
}