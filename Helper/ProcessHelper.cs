using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace Helper
{
    public static class ProcessHelper
    {
        /// <summary>
        /// Get child processes by parent process id , if insideSubProcess is not empty , it will get the child processes of the
        /// insideSubProcess
        /// </summary>
        /// <param name="parentPid"></param>
        /// <param name="insideSubProcess">child of which process name, keep empty if no need</param>
        /// <returns></returns>
        public static List<Process> GetChildProcessesByParentPid(int parentPid, string insideSubProcess = "")
        {
            // does not support other platforms
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return [];
            }

            const string query = "SELECT ProcessId , Name , ParentProcessId FROM Win32_Process";
            List<Process> result = [];
            using var searcher = new ManagementObjectSearcher(query);
            bool needReScan;
            do
            {
                needReScan = false;
                foreach (ManagementBaseObject? o in searcher.Get())
                {
                    if (o == null)
                        continue;
                    ManagementBaseObject p = o;
                    string? processName = p["Name"].ToString();
                    int processId = Convert.ToInt32(p["ProcessId"]);
                    int parentProcessId = Convert.ToInt32(p["ParentProcessId"]);
                    if (parentProcessId != parentPid)
                        continue;
                    if (!string.IsNullOrEmpty(insideSubProcess))
                    {
                        if (insideSubProcess == processName)
                        {
                            parentPid = processId;
                            needReScan = true;
                            break;
                        }
                    }

                    result.Add(Process.GetProcessById(processId));
                }
            } while (needReScan);

            return result;
        }
    }
}