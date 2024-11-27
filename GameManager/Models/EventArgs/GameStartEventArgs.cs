using JetBrains.Annotations;

namespace GameManager.Models.EventArgs
{
    public class GameStartEventArgs(int gameId, string gameName, int pid, TimeSpan duration) : System.EventArgs
    {
        [UsedImplicitly]
        public int GameId { get; set; } = gameId;

        [UsedImplicitly]
        public string GameName { get; set; } = gameName;

        [UsedImplicitly]
        public int Pid { get; set; } = pid;

        [UsedImplicitly]
        public TimeSpan Duration { get; set; } = duration;
    }
}