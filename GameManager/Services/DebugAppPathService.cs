namespace GameManager.Services
{
    internal class DebugAppPathService : AppPathService
    {
        public DebugAppPathService()
        {
            AppDirPath = AppDomain.CurrentDomain.BaseDirectory;
            ConfigDirPath = Path.Combine(AppDirPath, "configs");
            CoverDirPath = Path.Combine(ConfigDirPath, "covers");
            ToolsDirPath = Path.Combine(ConfigDirPath, "tools");
            DBFilePath = Path.Combine(ConfigDirPath, "game.db");
            LogDirPath = Path.Combine(ConfigDirPath, "logs");
            SaveFileBackupDirPath = Path.Combine(ConfigDirPath, "save-file-backup");
        }
    }
}