namespace GameManager.Services
{
    public interface IAppPathService
    {
        string AppDirPath { get; }
        string ConfigDirPath { get; }
        string CoverDirPath { get; }
        string ToolsDirPath { get; }
        string DBFilePath { get; }
        string LogDirPath { get; }
        string SaveFileBackupDirPath { get; }

        void CreateAppPath();
    }
}