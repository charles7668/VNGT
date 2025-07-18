using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameManager.Modules.GamePlayMonitor
{
    internal class GamePlayMonitorFactory
    {
        public GamePlayMonitorFactory(ILogger<SimpleGamePlayMonitor> simpleGamePlayMonitorLogger)
        {
            _simpleGamePlayMonitor = new SimpleGamePlayMonitor(simpleGamePlayMonitorLogger);
        }

        private readonly IGamePlayMonitor _simpleGamePlayMonitor;
        private readonly IGamePlayMonitor _processTracerGamePlayMonitor = new ProcessTracerGamePlayMonitor();

        public IGamePlayMonitor GetMonitor(bool useProcessTracer)
        {
            return useProcessTracer switch
            {
                true => _processTracerGamePlayMonitor,
                false => _simpleGamePlayMonitor
            };
        }
    }
}