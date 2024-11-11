using Hangfire;
using Hangfire.Storage;
using System.Linq.Expressions;

namespace GameManager.Models.TaskManager
{
    public class TaskManager
        : ITaskManager
    {
        public TaskManager(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
        {
            _ = new BackgroundJobServer();
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
        }

        private readonly Dictionary<string, Action> _cancelTaskDict = [];
        private readonly Dictionary<string, string> _taskJobDict = [];

        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;

        public Task<Result> StartBackgroundIntervalTask(string taskName, Expression<Action> task, Action cancelTask,
            int intervalMinutes, bool immediate)
        {
            if (intervalMinutes <= 0)
                throw new Exception("Interval must be greater than 0");
            if (_taskJobDict.TryGetValue(taskName, out string? hangfireJobId))
            {
                if (IsRecurringJobExists(hangfireJobId))
                {
                    return Task.FromResult(Result.Failure("Task is already running"));
                }
            }

            _recurringJobManager.AddOrUpdate(taskName, task, $"*/{intervalMinutes} * * * *");
            if (immediate)
                _recurringJobManager.Trigger(taskName);
            _taskJobDict[taskName] = taskName;
            _cancelTaskDict[taskName] = cancelTask;
            return Task.FromResult(Result.Ok());
        }

        public Task<Result> StartBackgroundTask(string taskName, Expression<Action> task, Action cancelTask)
        {
            if (_taskJobDict.TryGetValue(taskName, out string? hangfireJobId))
            {
                if (IsHangfireJobRunning(hangfireJobId))
                {
                    return Task.FromResult(Result.Failure("Task is already running"));
                }
            }

            hangfireJobId = _backgroundJobClient.Enqueue(task);
            _taskJobDict[taskName] = hangfireJobId;
            _cancelTaskDict[taskName] = cancelTask;
            return Task.FromResult(Result.Ok());
        }

        public Task<TaskExecutor.TaskStatus> GetTaskStatus(string taskName)
        {
            if (!_taskJobDict.TryGetValue(taskName, out string? hangfireJobId))
                return Task.FromResult(TaskExecutor.TaskStatus.NOT_FOUND);
            if (IsRecurringJobExists(hangfireJobId))
            {
                return Task.FromResult(TaskExecutor.TaskStatus.RUNNING);
            }

            return Task.FromResult(IsHangfireJobRunning(hangfireJobId)
                ? TaskExecutor.TaskStatus.RUNNING
                : TaskExecutor.TaskStatus.EXECUTE_PENDING);
        }

        public void CancelTask(string taskName)
        {
            if (!_taskJobDict.TryGetValue(taskName, out string? hangfireJobId)) return;
            if (IsRecurringJobExists(hangfireJobId))
            {
                _cancelTaskDict[taskName]();
                _recurringJobManager.RemoveIfExists(hangfireJobId);
            }
            else if (IsHangfireJobRunning(taskName))
            {
                _cancelTaskDict[taskName]();
                // _cancelTaskDict[taskName];
                _backgroundJobClient.Delete(hangfireJobId);
            }

            _taskJobDict.Remove(taskName);
            _cancelTaskDict.Remove(taskName);
        }

        private static bool IsRecurringJobExists(string jobId)
        {
            IStorageConnection? connection = JobStorage.Current.GetConnection();
            List<RecurringJobDto>? recurringJobs = connection.GetRecurringJobs();
            if (recurringJobs == null)
                return false;
            return recurringJobs.Any(job => job.Id == jobId);
        }

        private static string GetHangfireJobState(string jobId)
        {
            using IStorageConnection? connection = JobStorage.Current.GetConnection();
            JobData? jobData = connection.GetJobData(jobId);
            return jobData != null ? jobData.State : "not found";
        }

        private static bool IsHangfireJobRunning(string jobId)
        {
            return GetHangfireJobState(jobId) == "Processing";
        }
    }
}