﻿using System.Linq.Expressions;

namespace GameManager.Models.TaskManager
{
    public interface ITaskManager
    {
        Task<Result> StartBackgroundIntervalTask(string taskName, Expression<Action> task, Action cancelTask,
            int intervalMinutes, bool immediate = false);

        Task<Result> StartBackgroundTask(string taskName, Expression<Action> task, Action cancelTask);

        void CancelTask(string taskName);
    }
}