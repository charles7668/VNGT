using Helper.Models;
using JetBrains.Annotations;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace Helper
{
    public static class ProcessHelper
    {
        private const double PROCESS_START_WATCHER_INTERVAL = 0.5;
        private static ManagementEventWatcher? _ProcessStartWatcher;

        /// <summary>
        /// Registers a callback for process start events.
        /// </summary>
        /// <param name="callback">The callback to register.</param>
        [UsedImplicitly]
        public static void RegisterProcessStartCallback(EventArrivedEventHandler callback)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;
            if (_ProcessStartWatcher == null)
                return;
            _ProcessStartWatcher.EventArrived += callback;
        }

        /// <summary>
        /// Unregisters a callback for process start events.
        /// </summary>
        /// <param name="callback">The callback to unregister.</param>
        [UsedImplicitly]
        public static void UnregisterProcessStartCallback(EventArrivedEventHandler callback)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;
            if (_ProcessStartWatcher == null)
                return;
            _ProcessStartWatcher.EventArrived -= callback;
        }

        /// <summary>
        /// Stops the process start watcher and releases its resources.
        /// </summary>
        [UsedImplicitly]
        public static void StopProcessStartWatcher()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;
            if (_ProcessStartWatcher == null)
                return;
            _ProcessStartWatcher.Stop();
            _ProcessStartWatcher.Dispose();
            _ProcessStartWatcher = null;
        }

        /// <summary>
        /// Starts the process start watcher to monitor for process termination events.
        /// </summary>
        [UsedImplicitly]
        public static void StartProcessStartWatcher()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;
            if (_ProcessStartWatcher != null)
            {
                return;
            }

            string query =
                $"SELECT * FROM __InstanceCreationEvent WITHIN {PROCESS_START_WATCHER_INTERVAL} WHERE TargetInstance ISA 'Win32_Process'";

            _ProcessStartWatcher = new ManagementEventWatcher(new WqlEventQuery(query));

            _ProcessStartWatcher.Start();
        }

        /// <summary>
        /// Parses a <see cref="ManagementBaseObject" /> to extract process information.
        /// </summary>
        /// <param name="baseObject">The <see cref="ManagementBaseObject" /> containing process information.</param>
        /// <returns>A <see cref="ProcessInfo" /> object with the parsed process information.</returns>
        [UsedImplicitly]
        public static ProcessInfo ParseProcessInfo(this ManagementBaseObject baseObject)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new ProcessInfo();
            }

            string? processName = baseObject["Name"].ToString();
            int processId = Convert.ToInt32(baseObject["ProcessId"]);
            int parentProcessId = Convert.ToInt32(baseObject["ParentProcessId"]);
            string? executablePath = baseObject["ExecutablePath"]?.ToString();
            return new ProcessInfo
            {
                Name = processName ?? string.Empty,
                Id = processId,
                ParentId = parentProcessId,
                ExecutablePath = executablePath ?? string.Empty
            };
        }

        /// <summary>
        /// Retrieves process information for a given process ID.
        /// </summary>
        /// <param name="pid">The process ID to retrieve information for.</param>
        /// <returns>
        /// A <see cref="ProcessInfo" /> object containing the process information, or null if the platform is not Windows or the
        /// process is not found.
        /// </returns>
        [UsedImplicitly]
        public static ProcessInfo? GetProcessInfoByPid(int pid)
        {
            // does not support other platforms
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return null;
            }

            string query =
                $"SELECT ProcessId , Name , ParentProcessId , ExecutablePath FROM Win32_Process WHERE ProcessId = {pid}";
            ProcessInfo? result = null;
            using var searcher = new ManagementObjectSearcher(query);
            foreach (ManagementBaseObject? o in searcher.Get())
            {
                if (o == null)
                    continue;
                result = o.ParseProcessInfo();
            }

            return result;
        }

        /// <summary>
        /// Get all processes info
        /// </summary>
        /// <returns>A collection of <see cref="ProcessInfo" />.</returns>
        [UsedImplicitly]
        public static List<ProcessInfo> GetProcessInfos()
        {
            // does not support other platforms
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return [];
            }

            const string query = "SELECT ProcessId , Name , ParentProcessId , ExecutablePath FROM Win32_Process";
            List<ProcessInfo> result = [];
            using var searcher = new ManagementObjectSearcher(query);
            foreach (ManagementBaseObject? o in searcher.Get())
            {
                if (o == null)
                    continue;
                result.Add(o.ParseProcessInfo());
            }

            return result;
        }

        /// <summary>
        /// Get child processes by parent process id , if insideSubProcess is not empty , it will get the child processes of the
        /// insideSubProcess
        /// </summary>
        /// <param name="parentPid"></param>
        /// <param name="insideSubProcess">child of which process name, keep empty if no need</param>
        /// <returns>A list of processes</returns>
        [UsedImplicitly]
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
                    if (!string.IsNullOrEmpty(insideSubProcess) && insideSubProcess == processName)
                    {
                        parentPid = processId;
                        needReScan = true;
                        break;
                    }

                    result.Add(Process.GetProcessById(processId));
                }
            } while (needReScan);

            return result;
        }
    }
}