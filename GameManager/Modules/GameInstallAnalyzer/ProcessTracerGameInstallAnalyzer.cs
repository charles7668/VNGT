using GameManager.Models;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using FileInfo = System.IO.FileInfo;

namespace GameManager.Modules.GameInstallAnalyzer
{
    public class ProcessTracerGameInstallAnalyzer : IGameInstallAnalyzer
    {
        public ProcessTracerGameInstallAnalyzer()
        {
            _globalExcludePaths = InitializeExcludePaths();
            IPathConverter pathConverter = new PathConverter();
            _traceLineProcessor = new TraceLineProcessor(pathConverter, _ntPathToWin32PathCache);
            _filePathAnalyzer = new FilePathAnalyzer();
        }

        private readonly IFilePathAnalyzer _filePathAnalyzer;
        private readonly HashSet<string> _globalExcludePaths;
        private readonly Dictionary<string, string?> _ntPathToWin32PathCache = new();
        private readonly ITraceLineProcessor _traceLineProcessor;

        public async Task<Result<string?>> AnalyzeFromFileAsync(string traceDataFilePath, string installFilePath)
        {
            if (!ValidateInputs(traceDataFilePath, installFilePath))
                return Result<string?>.Ok(null);

            HashSet<string> excludePaths = PrepareExcludePaths(installFilePath);
            var (writeFileCandidates, createFileCandidates) =
                await ProcessTraceFileAsync(traceDataFilePath, excludePaths);

            return await FindBestExecutableAsync(writeFileCandidates, createFileCandidates);
        }

        private static bool ValidateInputs(string traceDataFilePath, string installFilePath)
        {
            return !string.IsNullOrWhiteSpace(traceDataFilePath) &&
                   !string.IsNullOrWhiteSpace(installFilePath) &&
                   File.Exists(traceDataFilePath);
        }

        private HashSet<string> PrepareExcludePaths(string installFilePath)
        {
            var excludePaths = new HashSet<string>(_globalExcludePaths);
            string? installDirectory = Path.GetDirectoryName(installFilePath);
            if (!string.IsNullOrEmpty(installDirectory))
            {
                excludePaths.Add(installDirectory);
            }

            return excludePaths;
        }

        private async Task<(Dictionary<string, int> writeFiles, Dictionary<string, int> createFiles)>
            ProcessTraceFileAsync(
                string traceDataFilePath, HashSet<string> excludePaths)
        {
            var writeFileCandidates = new Dictionary<string, int>();
            var createFileCandidates = new Dictionary<string, int>();

            using var reader = new StreamReader(traceDataFilePath, Encoding.UTF8);

            while (await reader.ReadLineAsync() is { } line)
            {
                var (accessType, filePath) = _traceLineProcessor.ProcessLine(line);

                if (!_filePathAnalyzer.IsValidFile(filePath, excludePaths))
                    continue;

                Dictionary<string, int> candidates =
                    accessType == AccessType.WRITE ? writeFileCandidates : createFileCandidates;
                if (accessType != AccessType.NONE)
                {
                    candidates[filePath] = candidates.GetValueOrDefault(filePath, 0) + 1;
                }
            }

            return (writeFileCandidates, createFileCandidates);
        }

        private async Task<Result<string?>> FindBestExecutableAsync(
            Dictionary<string, int> writeFileCandidates,
            Dictionary<string, int> createFileCandidates)
        {
            string? result = await TryFindExecutableFromCandidatesAsync(writeFileCandidates);
            if (!string.IsNullOrEmpty(result))
                return Result<string?>.Ok(result);

            result = await TryFindExecutableFromCandidatesAsync(createFileCandidates);
            return Result<string?>.Ok(result);
        }

        private static async Task<string?> TryFindExecutableFromCandidatesAsync(Dictionary<string, int> candidates)
        {
            Dictionary<string, int> pathCounter = BuildPathCounter(candidates);
            return await InspectPathsAsync(pathCounter);
        }

        private static Dictionary<string, int> BuildPathCounter(Dictionary<string, int> fileCandidates)
        {
            var pathCounter = new Dictionary<string, int>();

            foreach (var (filePath, count) in fileCandidates)
            {
                string? directory = Path.GetDirectoryName(filePath);
                if (directory == null) continue;

                pathCounter[directory] = pathCounter.GetValueOrDefault(directory, 0) + count;
            }

            return pathCounter;
        }

        private static async Task<string?> InspectPathsAsync(Dictionary<string, int> pathCounter)
        {
            IEnumerable<string> sortedPaths = pathCounter
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key);

            foreach (string path in sortedPaths)
            {
                string? executable = await Task.Run(() => FindLargestExecutableInDirectory(path));
                if (!string.IsNullOrEmpty(executable))
                    return executable;
            }

            return null;
        }

        private static string? FindLargestExecutableInDirectory(string directoryPath)
        {
            HashSet<string> excludeExtensions = ["uninst.exe"];
            try
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                FileInfo? largestExecutable = directoryInfo
                    .EnumerateFiles("*.exe", SearchOption.TopDirectoryOnly)
                    .Where(x => !excludeExtensions.Contains(x.Name.ToLowerInvariant()))
                    .MaxBy(file => file.Length);

                return largestExecutable?.FullName;
            }
            catch
            {
                return null;
            }
        }

        private static HashSet<string> InitializeExcludePaths()
        {
            return
            [
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Path.GetTempPath().TrimEnd('\\'),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft", "Windows", "Explorer"),

                "C:\\Windows\\Temp",
                "C:\\Program Files\\Sandboxie-Plus"
            ];
        }

        private interface IPathConverter
        {
            string? ConvertNtPathToWin32Path(string ntPath);
        }

        private interface ITraceLineProcessor
        {
            (AccessType accessType, string filePath) ProcessLine(string line);
        }

        private interface IFilePathAnalyzer
        {
            bool IsValidFile(string filePath, HashSet<string> excludePaths);
        }

        private class PathConverter : IPathConverter
        {
            private const int MAX_PATH = 260;

            public string? ConvertNtPathToWin32Path(string ntPath)
            {
                if (string.IsNullOrEmpty(ntPath) ||
                    !ntPath.StartsWith("\\Device\\", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                byte[] buffer = new byte[MAX_PATH * 2]; // Unicode characters are 2 bytes
                uint result = GetLogicalDriveStrings(MAX_PATH, buffer);

                if (result == 0 || result > MAX_PATH)
                    return null;

                string driveStrings = Encoding.Unicode.GetString(buffer, 0, (int)result * 2);
                string[] drives = driveStrings.Split('\0', StringSplitOptions.RemoveEmptyEntries);

                return FindMatchingDrive(ntPath, drives);
            }

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern uint GetLogicalDriveStrings(uint nBufferLength, byte[] lpBuffer);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern uint QueryDosDevice(string lpDeviceName, [Out] StringBuilder lpTargetPath,
                uint ucchMax);

            private static string? FindMatchingDrive(string ntPath, string[] drives)
            {
                foreach (string drive in drives)
                {
                    string driveLetter = drive.TrimEnd('\\');
                    var dosDeviceName = new StringBuilder(MAX_PATH);

                    if (QueryDosDevice(driveLetter, dosDeviceName, MAX_PATH) <= 0)
                        continue;

                    string deviceName = dosDeviceName.ToString();
                    if (ntPath.StartsWith(deviceName, StringComparison.OrdinalIgnoreCase))
                    {
                        return drive + ntPath.Substring(deviceName.Length);
                    }
                }

                return null;
            }
        }

        private class TraceLineProcessor(IPathConverter pathConverter, Dictionary<string, string?> pathCache)
            : ITraceLineProcessor
        {
            private static readonly Regex _CreateFileRegex = new(
                @"\[DesiredAccess\]\s+(?<access>[0-9A-Fa-f]+),\s+\[FileName\]\s+(?<filename>.+)",
                RegexOptions.Compiled);

            private static readonly Regex _WriteFileRegex = new(
                @"NtWriteFile\s+(?<filename>.+)",
                RegexOptions.Compiled);

            private static readonly Regex _SetInfoFileRegex = new(
                @"NtSetInformationFile\s+(?<filename>.+)",
                RegexOptions.Compiled);

            public (AccessType accessType, string filePath) ProcessLine(string line)
            {
                if (string.IsNullOrEmpty(line))
                    return (AccessType.NONE, "");

                return line switch
                {
                    var l when l.Contains("[Hook] NtCreateFile") => ProcessCreateFile(l),
                    var l when l.Contains("[Hook] NtWriteFile") => ProcessWriteFile(l),
                    var l when l.Contains("[Hook] NtSetInformationFile") => ProcessSetInfoFile(l),
                    _ => (AccessType.NONE, "")
                };
            }

            private (AccessType, string) ProcessCreateFile(string line)
            {
                Match match = _CreateFileRegex.Match(line);
                if (!match.Success)
                    return (AccessType.NONE, "");

                string accessHex = match.Groups["access"].Value;
                string fileName = match.Groups["filename"].Value;

                if (!ulong.TryParse(accessHex, NumberStyles.HexNumber, null,
                        out ulong desiredAccess))
                    return (AccessType.NONE, "");

                bool hasWriteAccess = (desiredAccess & (ulong)DesiredAccessFlag.GENERIC_WRITE) != 0 ||
                                      (desiredAccess & (ulong)DesiredAccessFlag.GENERIC_ALL) != 0;

                bool hasReadAccess = (desiredAccess & (ulong)DesiredAccessFlag.GENERIC_READ) != 0;

                if (!hasWriteAccess && !hasReadAccess)
                    return (AccessType.NONE, "");

                AccessType accessType = hasWriteAccess ? AccessType.WRITE : AccessType.READ;
                string cleanPath = ConvertToWin32Path(fileName.Replace("\\??\\", ""));

                return (accessType, cleanPath);
            }

            private (AccessType, string) ProcessWriteFile(string line)
            {
                Match match = _WriteFileRegex.Match(line);
                if (!match.Success)
                    return (AccessType.NONE, "");

                string fileName = match.Groups["filename"].Value;
                if (IsDevicePath(fileName))
                    return (AccessType.NONE, "");

                return (AccessType.WRITE, ConvertToWin32Path(fileName));
            }

            private (AccessType, string) ProcessSetInfoFile(string line)
            {
                Match match = _SetInfoFileRegex.Match(line);
                if (!match.Success)
                    return (AccessType.NONE, "");

                string fileName = match.Groups["filename"].Value;
                if (IsDevicePath(fileName))
                    return (AccessType.NONE, "");

                return (AccessType.WRITE, ConvertToWin32Path(fileName));
            }

            private static bool IsDevicePath(string fileName)
            {
                return fileName.StartsWith("\\device\\cdrom", StringComparison.OrdinalIgnoreCase);
            }

            private string ConvertToWin32Path(string ntPath)
            {
                if (pathCache.TryGetValue(ntPath, out string? cachedPath))
                    return cachedPath ?? "";

                string? win32Path = pathConverter.ConvertNtPathToWin32Path(ntPath);
                pathCache[ntPath] = win32Path;
                return win32Path ?? "";
            }
        }

        private class FilePathAnalyzer : IFilePathAnalyzer
        {
            public bool IsValidFile(string filePath, HashSet<string> excludePaths)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return false;

                // 目前不限制副檔名，允許所有檔案類型
                string? directory = Path.GetDirectoryName(filePath);
                return !IsExcludedPath(directory, excludePaths);
            }

            private static bool IsExcludedPath(string? dirPath, HashSet<string> excludePaths)
            {
                if (string.IsNullOrEmpty(dirPath))
                    return true;

                try
                {
                    string normalizedDirPath = Path.GetFullPath(dirPath).TrimEnd(Path.DirectorySeparatorChar);

                    return excludePaths.Any(excludePath =>
                    {
                        if (string.IsNullOrEmpty(excludePath))
                            return false;

                        string normalizedExcludePath =
                            Path.GetFullPath(excludePath).TrimEnd(Path.DirectorySeparatorChar);

                        return string.Equals(normalizedDirPath, normalizedExcludePath,
                                   StringComparison.OrdinalIgnoreCase) ||
                               normalizedDirPath.StartsWith(normalizedExcludePath + Path.DirectorySeparatorChar,
                                   StringComparison.OrdinalIgnoreCase);
                    });
                }
                catch
                {
                    return true; // 如果路徑無效，則排除
                }
            }
        }

        private enum AccessType
        {
            NONE,
            READ,
            WRITE
        }

        [Flags]
        private enum DesiredAccessFlag : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_ALL = 0x10000000
        }
    }

    // 分離的介面和實作類別
}