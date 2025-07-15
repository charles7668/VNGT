using GameManager.Models;
using System.Text;
using System.Text.RegularExpressions;
using FileInfo = System.IO.FileInfo;

namespace GameManager.Modules.GameInstallAnalyzer
{
    public class ProcessTracerGameInstallAnalyzer : IGameInstallAnalyzer
    {
        // exclude uninstaller file name
        private readonly HashSet<string> _globalExcludeFileName = ["uninst.exe"];

        private readonly HashSet<string> _globalExcludePath =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Path.GetTempPath().TrimEnd('\\'),
            "C:\\Windows\\Temp",
            "C:\\Program Files\\Sandboxie-Plus"
        ];

        public async Task<Result<string?>> AnalyzeFromFileAsync(string traceDataFilePath, string installFilePath)
        {
            if (string.IsNullOrWhiteSpace(traceDataFilePath) || string.IsNullOrWhiteSpace(installFilePath))
                return Result<string?>.Ok(null);
            if (!File.Exists(traceDataFilePath))
                return Result<string?>.Ok(null);
            Dictionary<string, int> candidateFilePaths = [];
            var excludePath = new HashSet<string>(_globalExcludePath)
            {
                Path.GetDirectoryName(installFilePath)!
            };
            string? target = null;
            using (var sr = new StreamReader(traceDataFilePath, Encoding.UTF8))
            {
                do
                {
                    string? line = await sr.ReadLineAsync();
                    if (line == null || !line.Contains("[Hook] NtCreateFile"))
                        continue;
                    string filePath = SolveNtCreateFile(line);
                    string extension = Path.GetExtension(filePath).ToLower();
                    if (string.IsNullOrWhiteSpace(filePath) || extension != ".exe")
                        continue;
                    if (excludePath.Contains(Path.GetDirectoryName(filePath) ?? ""))
                    {
                        continue;
                    }

                    if (_globalExcludeFileName.Contains(Path.GetFileName(filePath)))
                        continue;

                    if (!candidateFilePaths.TryAdd(filePath, 1))
                    {
                        candidateFilePaths[filePath]++;
                    }
                } while (!sr.EndOfStream);
            }

            if (candidateFilePaths.Count == 0)
                return Result<string?>.Ok(target);

            // Use the largest file size as the target
            long maxFileSize = 0;
            foreach (KeyValuePair<string, int> candidateFile in candidateFilePaths)
            {
                var fileInfo = new FileInfo(candidateFile.Key);
                if (fileInfo.Length <= maxFileSize)
                    continue;
                maxFileSize = fileInfo.Length;
                target = candidateFile.Key;
            }

            return Result<string?>.Ok(target);
        }

        private string SolveNtCreateFile(string line)
        {
            string pattern = @"\[DesiredAccess\]\s+(?<access>[0-9A-Fa-f]+),\s+\[FileName\]\s+(?<filename>.+)";

            Match match = Regex.Match(line, pattern);

            if (!match.Success)
                return "";
            string desiredAccessBinary = match.Groups["access"].Value;
            string fileName = match.Groups["filename"].Value;
            ulong desiredAccess = Convert.ToUInt64(desiredAccessBinary, 2);

            if ((desiredAccess & (ulong)DesiredAccessFlag.GENERIC_WRITE) != 0 ||
                (desiredAccess & (ulong)DesiredAccessFlag.GENERIC_ALL) != 0)
            {
                return fileName.Replace("\\??\\", "");
            }

            return "";
        }

        [Flags]
        private enum DesiredAccessFlag : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000
        }
    }
}