using Helper.Models;
using JetBrains.Annotations;
using System.Diagnostics;

namespace Helper.Sandboxie
{
    public static class SandboxieHelper
    {
        /// <summary>
        /// Retrieves a list of processes running in a specified Sandboxie sandbox.
        /// </summary>
        /// <param name="sandboxieStartExePath">The path to the Sandboxie start executable.</param>
        /// <param name="boxName">The name of the Sandboxie sandbox.</param>
        /// <returns>A list of <see cref="ProcessInfo" /> objects representing the processes running in the specified sandbox.</returns>
        [UsedImplicitly]
        public static List<ProcessInfo> GetSandboxieProcesses(string sandboxieStartExePath, string boxName)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = sandboxieStartExePath,
                Arguments = $"/box:{boxName} /listpids",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(startInfo);
            if (process == null)
                return [];
            string output = process.StandardOutput.ReadToEnd();
            string[] split = output.Split('\n');
            List<ProcessInfo> processInfos = new();
            foreach (string s in split)
            {
                if (!int.TryParse(s, out int pid))
                {
                    continue;
                }

                ProcessInfo? processInfo = ProcessHelper.GetProcessInfoByPid(pid);
                if (processInfo != null)
                {
                    processInfos.Add(processInfo);
                }
            }

            return processInfos;
        }
    }
}