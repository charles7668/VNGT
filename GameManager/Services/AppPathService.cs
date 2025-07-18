using System.Diagnostics.CodeAnalysis;

namespace GameManager.Services
{
    internal class AppPathService : IAppPathService
    {
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public AppPathService()
        {
            AppDirPath = AppDomain.CurrentDomain.BaseDirectory;
            ConfigDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VNGT",
                "configs");
            CoverDirPath = Path.Combine(ConfigDirPath, "covers");
            ToolsDirPath = Path.Combine(ConfigDirPath, "tools");
            DBFilePath = Path.Combine(ConfigDirPath, "game.db");
            LogDirPath = Path.Combine(ConfigDirPath, "logs");
            SaveFileBackupDirPath = Path.Combine(ConfigDirPath, "save-file-backup");
            ScreenShotsDirPath = Path.Combine(ConfigDirPath, "screenshots");
            ProcessTracerDirPath = Path.GetFullPath("ProcessTracer");
        }

        public string AppDirPath { get; protected init; }
        public string ConfigDirPath { get; protected init; }
        public string CoverDirPath { get; protected init; }
        public string ToolsDirPath { get; protected init; }
        public string DBFilePath { get; protected init; }
        public string LogDirPath { get; protected init; }
        public string SaveFileBackupDirPath { get; protected init; }
        public string ScreenShotsDirPath { get; protected init; }
        public string ProcessTracerDirPath { get; protected init; }

        public void CreateAppPath()
        {
            Directory.CreateDirectory(ConfigDirPath);
            Directory.CreateDirectory(LogDirPath);
            Directory.CreateDirectory(ToolsDirPath);
            Directory.CreateDirectory(CoverDirPath);
            Directory.CreateDirectory(SaveFileBackupDirPath);
            Directory.CreateDirectory(ScreenShotsDirPath);
        }
    }
}