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
            try
            {
                action();
                return false;
            }
            catch (Exception e)
            {
                exceptionHandler?.Invoke(e);
                return true;
            }
            finally
            {
                finallyHandler?.Invoke();
            }
        }

        /// <summary>
        /// Execute action with handle all exception
        /// </summary>
        /// <param name="action">action</param>
        /// <param name="exceptionHandler"></param>
        /// <param name="finallyHandler"></param>
        /// <returns>Return true if exception occur , otherwise false</returns>
        [UsedImplicitly]
        public static async Task<bool> ExecuteWithExceptionHandlingAsync(Func<Task> action,
            Func<Exception, Task>? exceptionHandler = null, Func<Task>? finallyHandler = null)
        {
            try
            {
                await action();
                return false;
            }
            catch (Exception e)
            {
                if (exceptionHandler != null)
                {
                    await exceptionHandler(e);
                }

                return true;
            }
            finally
            {
                finallyHandler?.Invoke();
            }
        }
    }
}