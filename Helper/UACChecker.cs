using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Helper
{
    // referred from https://www.experts-exchange.com/questions/25089585/check-from-C-app-whether-another-exe-file-requires-admin-rights-UAC.html
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class UACChecker
    {
        private const int ERROR_ELEVATION_REQUIRED = 740;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreateProcess(string? lpApplicationName,
            string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles,
            CreationFlags dwCreationFlags, IntPtr lpEnvironment, string? lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);


        public static bool RequiresElevation(string fileName)
        {
            bool requiresElevation = false;
            int win32Error = 0;
            var sInfo = new STARTUPINFO();
            var pSec = new SECURITY_ATTRIBUTES();
            var tSec = new SECURITY_ATTRIBUTES();

            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            // Attempt to start "Filename" in a suspended state
            bool success = CreateProcess(null, fileName,
                ref pSec, ref tSec, false, CreationFlags.CREATE_SUSPENDED,
                IntPtr.Zero, null, ref sInfo, out PROCESS_INFORMATION pInfo);


            // if start successful, we don't need elevation
            if (success)
                requiresElevation = false;
            else
            {
                // if the error is ERROR_ELEVATION_REQUIRED, then we need elevation
                win32Error = Marshal.GetLastWin32Error();
                if (win32Error == ERROR_ELEVATION_REQUIRED)
                    requiresElevation = true;
            }


            // We don't actually want "Filename" to run, so kill the process and close the handles in pInfo
            TerminateProcess(pInfo.hProcess, 0);
            CloseHandle(pInfo.hThread);
            CloseHandle(pInfo.hProcess);

            // If there was an error, and that error was NOT elevation is required then throw an exception
            if (win32Error != 0 && win32Error != ERROR_ELEVATION_REQUIRED)
                throw new Win32Exception(win32Error);

            return requiresElevation;
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO

        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [Flags]
        private enum CreationFlags : uint
        {
            CREATE_SUSPENDED = 0x4
        }
    }
}