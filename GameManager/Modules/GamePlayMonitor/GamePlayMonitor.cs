using GameManager.Models;
using GameManager.Models.EventArgs;
using Helper;
using System.Collections.Concurrent;
using System.Management;
using ProcessInfo = Helper.Models.ProcessInfo;

namespace GameManager.Modules.GamePlayMonitor
{
    public class GamePlayMonitor : IGamePlayMonitor
    {
        private readonly ConcurrentDictionary<int, HashSet<int>> _childMap = new();
        private readonly ConcurrentDictionary<int, int> _gameIdPidMap = [];
        private readonly ConcurrentDictionary<int, MonitorItem> _monitorItems = new();
        private readonly object _removingLock = new();
        private CancellationTokenSource _monitorCts = new();
        private Task _monitorTask = Task.CompletedTask;

        public Task<Result> AddMonitorItem(int gameId, string gameName, int pid,
            Action<GameStopEventArgs>? onStopCallback)
        {
            List<Action<GameStopEventArgs>> callbacks = [];
            if (onStopCallback != null)
                callbacks.Add(onStopCallback);
            // if exist monitor item, return
            if (!_monitorItems.TryAdd(pid, new MonitorItem
                {
                    GameId = gameId,
                    GameName = gameName,
                    StartTime = DateTime.UtcNow,
                    OnStopCallbacks = callbacks
                })
                || !_gameIdPidMap.TryAdd(gameId, pid))
            {
                return Task.FromResult(Result.Ok());
            }

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

        public void RegisterCallback(int gameId, Action<GameStopEventArgs> onStopCallback)
        {
            if (!_gameIdPidMap.TryGetValue(gameId, out int pid))
                return;
            if (!_monitorItems.TryGetValue(pid, out MonitorItem? monitorItem))
                return;
            if (monitorItem.OnStopCallbacks.Contains(onStopCallback))
                return;
            monitorItem.OnStopCallbacks.Add(onStopCallback);
        }

        public void UnregisterCallback(int gameId, Action<GameStopEventArgs> onStopCallback)
        {
            if (!_gameIdPidMap.TryGetValue(gameId, out int pid))
                return;
            if (!_monitorItems.TryGetValue(pid, out MonitorItem? monitorItem))
                return;
            monitorItem.OnStopCallbacks.Remove(onStopCallback);
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
            }

            await Task.Run(() =>
            {
                if (monitorItem == null || monitorItem.OnStopCallbacks.Count == 0)
                    return;
                foreach (Action<GameStopEventArgs> cb in monitorItem.OnStopCallbacks)
                    cb.Invoke(new GameStopEventArgs(monitorItem.GameId, monitorItem.GameName, pid,
                        DateTime.UtcNow - monitorItem.StartTime));
            }, CancellationToken.None);
        }

        /// <summary>
        /// Monitors the target process and its child processes to determine if they are alive.
        /// </summary>
        /// <param name="monitorItemPid">The process ID of the target process to monitor.</param>
        /// <param name="processInfos">A list of process information objects.</param>
        /// <param name="childMap">A dictionary mapping process IDs to their child process IDs.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean indicating whether any
        /// processes are alive.
        /// </returns>
        private static Task<bool> MonitorTargetPid(int monitorItemPid, List<ProcessInfo> processInfos,
            ConcurrentDictionary<int, HashSet<int>> childMap)
        {
            Queue<int> queue = new();
            queue.Enqueue(monitorItemPid);
            int aliveCount = 0;
            if (processInfos.Find(x => x.Id == monitorItemPid) != null)
                aliveCount = 1;
            if (!childMap.TryGetValue(monitorItemPid, out HashSet<int>? oldChildSet))
                oldChildSet = [];
            while (queue.Count > 0)
            {
                int pid = queue.Dequeue();
                IEnumerable<ProcessInfo> childListOfItem =
                    processInfos.Where(x => x.ParentId == pid);
                foreach (int childId in childListOfItem.Select(x => x.Id))
                {
                    aliveCount++;
                    childMap[monitorItemPid].Add(childId);
                    oldChildSet.Remove(childId);
                    queue.Enqueue(childId);
                }

                foreach (int oldChildId in oldChildSet)
                {
                    queue.Enqueue(oldChildId);
                }
            }

            return Task.FromResult(aliveCount != 0);
        }

        private void OnProcessStopEvent(object sender, EventArrivedEventArgs args)
        {
            var process = (ManagementBaseObject)args.NewEvent["TargetInstance"];
            ProcessInfo processInfo = process.ParseProcessInfo();
            foreach (int pid in _monitorItems.Select(x => x.Key))
            {
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
                ProcessHelper.StartProcessStopWatcher();
                ProcessHelper.RegisterProcessStopCallback(OnProcessStopEvent);
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

                    List<ProcessInfo> processInfos = ProcessHelper.GetProcessInfos();
                    var monitorPidList = _monitorItems.Keys.ToList();
                    foreach (int monitorItemPid in monitorPidList)
                    {
                        bool isAlive = await MonitorTargetPid(monitorItemPid, processInfos, _childMap);

                        // if no child process alive, invoke the callback
                        if (!isAlive)
                            await InvokeCallbacks(monitorItemPid);
                    }

                    await Task.Delay(500, cancellationToken);
                }

                ProcessHelper.StopProcessStopWatcher();
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
            public List<Action<GameStopEventArgs>> OnStopCallbacks { get; init; } = [];
        }
    }
}