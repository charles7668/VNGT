using GameManager.Models;
using GameManager.Properties;
using System.Diagnostics;

namespace GameManager.Services
{
    public static class PathService
    {
        public static Result OpenPathInExplorer(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Result.Failure(Resources.Message_DirectoryNotExist);
            }

            try
            {
                Process.Start("explorer.exe", path);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }
    }
}