using GameManager.Models;
using System.Text;

namespace GameManager.Modules.GameInstallAnalyzer
{
    public class ProcessTracerGameInstallAnalyzer : IGameInstallAnalyzer
    {
        private readonly string[] _globalExcludePath =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Path.GetTempPath(),
            "C:\\Program Files\\Sandboxie-Plus"
        ];

        public async Task<Result<string?>> AnalyzeFromFileAsync(string traceDataFilePath, string installFilePath)
        {
            if (string.IsNullOrWhiteSpace(traceDataFilePath) || string.IsNullOrWhiteSpace(installFilePath))
                return Result<string?>.Ok(null);
            if (!File.Exists(traceDataFilePath))
                return Result<string?>.Ok(null);
            Dictionary<FileIOType, HashSet<string>> candidateFilePaths = [];
            var excludePath = new List<string>(_globalExcludePath)
            {
                Path.GetDirectoryName(installFilePath)!
            };
            Dictionary<string, int> pathCounter = new();
            using (var sr = new StreamReader(traceDataFilePath, Encoding.UTF8))
            {
                do
                {
                    string? line = await sr.ReadLineAsync();
                    // Find FileIOCreate or FileIOWrite
                    if (line == null ||
                        (!line.StartsWith("[FileIOWrite]") && !line.StartsWith("[FileIOCreate]")))
                    {
                        continue;
                    }

                    FileIOType type = line.StartsWith("[FileIOWrite]") ? FileIOType.WRITE : FileIOType.CREATE;

                    int startIndex = line.LastIndexOf(", File: ", StringComparison.Ordinal);
                    string filePath = line.Substring(startIndex + 8);
                    // if the file path is in the exclude path, skip it
                    if (excludePath.Any(p => filePath.ToLower().StartsWith(p.ToLower())))
                    {
                        continue;
                    }


                    string? dirPath = Path.GetDirectoryName(filePath);
                    if (filePath.EndsWith('\\'))
                        dirPath = filePath;
                    string ext = Path.GetExtension(filePath);
                    // only find for exe path
                    if (dirPath == null || ext != ".exe") continue;
                    pathCounter[dirPath] = pathCounter.GetValueOrDefault(dirPath) + 1;
                    if (!candidateFilePaths.ContainsKey(type))
                        candidateFilePaths[type] = [];
                    candidateFilePaths[type].Add(filePath);
                } while (!sr.EndOfStream);
            }

            int totalCount = 0;
            foreach (KeyValuePair<FileIOType, HashSet<string>> candidateFilePath in candidateFilePaths)
            {
                totalCount += candidateFilePath.Value.Count;
            }

            if (totalCount == 0)
            {
                return Result<string?>.Ok(null);
            }

            // find candidate , prefer write over create
            List<string> candidates =
                candidateFilePaths.ContainsKey(FileIOType.WRITE) && candidateFilePaths[FileIOType.WRITE].Count > 0
                    ? candidateFilePaths[FileIOType.WRITE].ToList()
                    : candidateFilePaths[FileIOType.CREATE].ToList();
            string? target = candidates.FirstOrDefault();
            int targetCount = pathCounter!.GetValueOrDefault(Path.GetDirectoryName(candidates[0]));
            for (int i = 1; i < candidates.Count; ++i)
            {
                string? dirPath = Path.GetDirectoryName(candidates[i]);
                if (string.IsNullOrEmpty(dirPath) || pathCounter[dirPath] <= targetCount) continue;
                targetCount = pathCounter[dirPath];
                target = candidates[i];
            }

            return Result<string?>.Ok(target);
        }

        private enum FileIOType
        {
            WRITE,
            CREATE
        }
    }
}