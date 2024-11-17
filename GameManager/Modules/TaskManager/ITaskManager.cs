using GameManager.Models;
using System.Linq.Expressions;

namespace GameManager.Modules.TaskManager
{
    public interface ITaskManager
    {
        Task<Result> StartBackgroundIntervalTask(string taskName, Expression<Action> task, Action cancelTask,
            int intervalMinutes, bool immediate = false);

        Task<Result> StartBackgroundTask(string taskName, Expression<Action> task, Action cancelTask);

        Task<TaskExecutor.TaskStatus> GetTaskStatus(string taskName);

        void CancelTask(string taskName);
    }
}