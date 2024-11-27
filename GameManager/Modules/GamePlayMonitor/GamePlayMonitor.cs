using GameManager.Models;
using GameManager.Models.EventArgs;
using Helper;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Management;
using ProcessInfo = Helper.Models.ProcessInfo;

namespace GameManager.Modules.GamePlayMonitor
{
    public class GamePlayMonitor(ILogger<GamePlayMonitor> logger) : IGamePlayMonitor
    {
        private readonly ConcurrentDictionary<int, HashSet<int>> _childMap = new();
        private readonly ConcurrentDictionary<int, int> _gameIdPidMap = [];
        private readonly ConcurrentDictionary<int, MonitorItem> _monitorItems = new();
        private readonly object _removingLock = new();
        private CancellationTokenSource _monitorCts = new();
        private Task _monitorTask = Task.CompletedTask;

        public Task<Result> AddMonitorItem(int gameId, string gameName, int pid,
            Action<GameStartEventArgs>? onStartCallback)
        {
            List<Action<GameStartEventArgs>> callbacks = [];
            if (onStartCallback != null)
                callbacks.Add(onStartCallback);
            // if exist monitor item, return
            if (!_monitorItems.TryAdd(pid, new MonitorItem
                {
                    GameId = gameId,
                    GameName = gameName,
                    StartTime = DateTime.UtcNow,
                    OnStartCallbacks = callbacks
                })
                || !_gameIdPidMap.TryAdd(gameId, pid))
            {
                return Task.FromResult(Result.Ok());
            }

            logger.LogInformation("Monitoring game {GameName} with PID {Pid}", gameName, pid);

            // if this is the first item, start monitoring
            if (_monitorItems.Count != 1)
                return Task.FromResult(Result.Ok());
            if (_monitorTask != Task.CompletedTask && _monitorCts.IsCancellationRequested)
            {
                _ = _monitorTask.ContinueWith(_ =>
                {
                    StartNewMonitorTask();
                });
            }
            else
            {
                StartNewMonitorTask();
            }

            return Task.FromResult(Result.Ok());
        }

        public Task RemoveMonitorItem(int pid)
        {
            lock (_removingLock)
            {
                if (!_monitorItems.TryRemove(pid, out MonitorItem? monitorItem))
                    return Task.CompletedTask;
                _gameIdPidMap.TryRemove(monitorItem.GameId, out _);
            }

            return Task.CompletedTask;
        }

        public void RegisterCallback(int gameId, Action<GameStartEventArgs> onStartCallback)
        {
            if (!_gameIdPidMap.TryGetValue(gameId, out int pid))
                return;
            if (!_monitorItems.TryGetValue(pid, out MonitorItem? monitorItem))
                return;
            if (monitorItem.OnStartCallbacks.Contains(onStartCallback))
                return;
            monitorItem.OnStartCallbacks.Add(onStartCallback);
        }

        public void UnregisterCallback(int gameId, Action<GameStartEventArgs> onStartCallback)
        {
            if (!_gameIdPidMap.TryGetValue(gameId, out int pid))
                return;
            if (!_monitorItems.TryGetValue(pid, out MonitorItem? monitorItem))
                return;
            monitorItem.OnStartCallbacks.Remove(onStartCallback);
        }

        public bool IsMonitoring(int gameId)
        {
            return _gameIdPidMap.TryGetValue(gameId, out _);
        }

        private async Task InvokeCallbacks(int pid)
        {
            MonitorItem? monitorItem;
            lock (_removingLock)
            {
                if (_monitorItems.TryRemove(pid, out monitorItem))
                {
                    _gameIdPidMap.TryRemove(monitorItem.GameId, out _);
                }

                _childMap.TryRemove(pid, out _);
            }

            await Task.Run(() =>
            {
                if (monitorItem == null || monitorItem.OnStartCallbacks.Count == 0)
                    return;
                foreach (Action<GameStartEventArgs> cb in monitorItem.OnStartCallbacks)
                    cb.Invoke(new GameStartEventArgs(monitorItem.GameId, monitorItem.GameName, pid,
                        DateTime.UtcNow - monitorItem.StartTime));
            }, CancellationToken.None);
        }

        /// <summary>
        /// Monitors the target process and its child processes to determine if they are alive.
        /// </summary>
        /// <param name="monitorItemPid">The process ID of the target process to monitor.</param>
        /// <param name="childMap">A dictionary mapping process IDs to their child process IDs.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean indicating whether any
        /// processes are alive.
        /// </returns>
        private static async Task<bool> MonitorTargetPid(int monitorItemPid,
            ConcurrentDictionary<int, HashSet<int>> childMap)
        {
            int aliveCount = 0;
            for (int i = 0; i < 6; ++i)
            {
                if (aliveCount > 0)
                    break;
                List<ProcessInfo> processInfos = ProcessHelper.GetProcessInfos();
                aliveCount = 0;
                if (processInfos.Find(x => x.Id == monitorItemPid) != null)
                    aliveCount = 1;
                if (!childMap.TryRemove(monitorItemPid, out HashSet<int>? oldChildSet))
                    oldChildSet = [];
                childMap.TryAdd(monitorItemPid, []);
                foreach (int oldChild in oldChildSet)
                {
                    var childListOfItem =
                        processInfos.Where(x => x.ParentId == oldChild).ToList();
                    childMap[monitorItemPid].Add(oldChild);
                    aliveCount += childListOfItem.Count;
                    foreach (int childId in childListOfItem.Select(x => x.Id))
                        childMap[monitorItemPid].Add(childId);
                }

                await Task.Delay(50);
            }

            return aliveCount != 0;
        }

        private void OnProcessChangedEvent(object sender, EventArrivedEventArgs args)
        {
            var process = (ManagementBaseObject)args.NewEvent["TargetInstance"];
            ProcessInfo processInfo = process.ParseProcessInfo();
            foreach (int pid in _monitorItems.Select(x => x.Key))
            {
                _childMap.TryAdd(pid, []);
                if (processInfo.ParentId == pid)
                {
                    _childMap[pid].Add(processInfo.Id);
                    return;
                }

                if (!_childMap.TryGetValue(pid, out HashSet<int>? childSet) ||
                    !childSet.Contains(processInfo.ParentId))
                    continue;
                _childMap[pid].Add(processInfo.Id);
                return;
            }
        }

        private Task MonitorGamePlay(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                ProcessHelper.StartProcessStartWatcher();
                ProcessHelper.RegisterProcessStartCallback(OnProcessChangedEvent);
                int tryCount = 0;
                const int maxTryCount = 10;
                await Task.Delay(500, cancellationToken);
                while (!cancellationToken.IsCancellationRequested && tryCount < maxTryCount)
                {
                    if (_monitorItems.Count == 0)
                    {
                        tryCount++;
                        await Task.Delay(500, cancellationToken);
                        continue;
                    }

                    tryCount = 0;

                    var monitorPidList = _monitorItems.Keys.ToList();
                    foreach (int monitorItemPid in monitorPidList)
                    {
                        bool isAlive = await MonitorTargetPid(monitorItemPid, _childMap);

                        // if no child process alive, invoke the callback
                        if (!isAlive)
                        {
                            logger.LogInformation("Game {GameName} with PID {Pid} stopped",
                                _monitorItems[monitorItemPid].GameName, monitorItemPid);
                            await InvokeCallbacks(monitorItemPid);
                        }
                    }

                    await Task.Delay(500, cancellationToken);
                }

                logger.LogDebug("Monitor task stopped");
                ProcessHelper.StopProcessStartWatcher();
                _childMap.Clear();
            }, cancellationToken);
        }

        private void StartNewMonitorTask()
        {
            _monitorCts.Dispose();
            _monitorCts = new CancellationTokenSource();
            _monitorTask = MonitorGamePlay(_monitorCts.Token);
        }

        private sealed class MonitorItem
        {
            public int GameId { get; init; }
            public string GameName { get; init; } = string.Empty;
            public DateTime StartTime { get; init; } = DateTime.UtcNow;
            public List<Action<GameStartEventArgs>> OnStartCallbacks { get; init; } = [];
        }
    }
}