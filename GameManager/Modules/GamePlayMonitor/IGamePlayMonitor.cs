using GameManager.Models;
using GameManager.Models.EventArgs;

namespace GameManager.Modules.GamePlayMonitor
{
    public interface IGamePlayMonitor
    {
        /// <summary>
        /// Adds a monitor item for a specific game and process.
        /// </summary>
        /// <param name="gameId">The ID of the game to monitor.</param>
        /// <param name="gameName"></param>
        /// <param name="pid">The process ID to monitor.</param>
        /// <param name="onStartCallback">The callback to invoke when the process stops.</param>
        /// <returns>A result indicating the success or failure of the operation.</returns>
        Task<Result> AddMonitorItem(int gameId, string gameName, int pid, Action<GameStartEventArgs>? onStartCallback);

        /// <summary>
        /// Removes a monitor item for a specific process.
        /// </summary>
        /// <param name="pid">The process ID to remove from monitoring.</param>
        Task RemoveMonitorItem(int pid);

        /// <summary>
        /// Registers a callback for a specific game.
        /// </summary>
        /// <param name="gameId">The ID of the game to register the callback for.</param>
        /// <param name="onStartCallback">The callback to invoke when the process stops.</param>
        void RegisterCallback(int gameId, Action<GameStartEventArgs> onStartCallback);

        /// <summary>
        /// Unregisters a callback for a specific game.
        /// </summary>
        /// <param name="gameId">The ID of the game to unregister the callback for.</param>
        /// <param name="onStartCallback">The callback to remove.</param>
        void UnregisterCallback(int gameId, Action<GameStartEventArgs> onStartCallback);

        /// <summary>
        /// Checks if a specific game is being monitored.
        /// </summary>
        /// <param name="gameId">The ID of the game to check.</param>
        /// <returns>True if the game is being monitored, otherwise false.</returns>
        bool IsMonitoring(int gameId);
    }
}