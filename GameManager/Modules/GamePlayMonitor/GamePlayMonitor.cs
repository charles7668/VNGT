using GameManager.Models;
using GameManager.Models.EventArgs;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipes;

namespace GameManager.Modules.GamePlayMonitor
{
    public class GamePlayMonitor(ILogger<GamePlayMonitor> logger) : IGamePlayMonitor
    {
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
                _monitorTask.Wait();
            }

            StartNewMonitorTask();

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

        private Task MonitorGamePlay(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(async () =>
            {
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
                        bool isAlive = false;
                        try
                        {
                            var monitorProc = Process.GetProcessById(monitorItemPid);
                            isAlive = monitorProc is { HasExited: false, ProcessName: "ProcessTracer" };
                        }
                        catch
                        {
                            // ignore
                        }

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
            }, TaskCreationOptions.LongRunning).Unwrap();
        }

        private void StartNewMonitorTask()
        {
            _monitorCts.Dispose();
            _monitorCts = new CancellationTokenSource();
            _monitorTask = MonitorGamePlay(_monitorCts.Token);
        }

        public void SendStopRequest(int gameId)
        {
            var exist = _gameIdPidMap.TryGetValue(gameId, out int pid);
            if (!exist)
                return;
            bool isAlive;
            try
            {
                var proc = Process.GetProcessById(pid);
                isAlive = proc is { HasExited: false, ProcessName: "ProcessTracer" };
            }
            catch
            {
                return;
            }

            if (!isAlive)
                return;

            using var pipeClient =
                new NamedPipeClientStream(".", "ProcessTracerPipe:" + pid, PipeDirection.Out,
                    PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            try
            {
                pipeClient.Connect(100);
                if (pipeClient.IsConnected)
                {
                    using var writer = new StreamWriter(pipeClient);
                    writer.AutoFlush = true;
                    writer.WriteLine("[CloseApp]");
                }
            }
            catch
            {
                logger.Log(LogLevel.Warning, "Failed to send stop request to game {GameId} with PID {Pid}",
                    gameId, pid);
            }
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