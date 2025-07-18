using GameManager.Models;
using GameManager.Models.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameManager.Modules.GamePlayMonitor
{
    internal class ProcessTracerGamePlayMonitor : IGamePlayMonitor
    {
        public Task<Result> AddMonitorItem(int gameId, string gameName, int pid, Action<GameStartEventArgs>? onStartCallback)
        {
            throw new NotImplementedException();
        }

        public Task RemoveMonitorItem(int pid)
        {
            throw new NotImplementedException();
        }

        public void RegisterCallback(int gameId, Action<GameStartEventArgs> onStartCallback)
        {
            throw new NotImplementedException();
        }

        public void UnregisterCallback(int gameId, Action<GameStartEventArgs> onStartCallback)
        {
            throw new NotImplementedException();
        }

        public bool IsMonitoring(int gameId)
        {
            throw new NotImplementedException();
        }
    }
}
