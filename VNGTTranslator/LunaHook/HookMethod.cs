using System.Runtime.InteropServices;

namespace VNGTTranslator.LunaHook
{
    public static class HookMethod
    {
        public static void LunaStart(
            LunaDll.ProcessEvent connect,
            LunaDll.ProcessEvent disconnect,
            LunaDll.ThreadEvent create,
            LunaDll.ThreadEvent destroy,
            LunaDll.OutputCallback output,
            LunaDll.ConsoleHandler console,
            LunaDll.HookInsertHandler hookInsert,
            LunaDll.EmbedCallback embed
        )
        {
            if (Program.Is64Bit)
                LunaDll.LunaStart64(connect, disconnect, create, destroy, output, console, hookInsert, embed);
            else
                LunaDll.LunaStart32(connect, disconnect, create, destroy, output, console, hookInsert, embed);
        }

        public static void LunaInject(uint pid, string basePath)
        {
            if (Program.Is64Bit)
                LunaDll.LunaInject64(pid, basePath);
            else
                LunaDll.LunaInject32(pid, basePath);
        }

        public static void LunaDetach(uint pid)
        {
            if (Program.Is64Bit)
                LunaDll.LunaDetach64(pid);
            else
                LunaDll.LunaDetach32(pid);
        }

        public static void LunaSettings(int flushDelay, bool filterRepetition, int defaultCodepage,
            int maxBufferSize, int maxHistorySize)
        {
            if (Program.Is64Bit)
                LunaDll.LunaSettings64(flushDelay, filterRepetition, defaultCodepage, maxBufferSize, maxHistorySize);
            else
                LunaDll.LunaSettings32(flushDelay, filterRepetition, defaultCodepage, maxBufferSize, maxHistorySize);
        }
    }

    public static class LunaDll
    {
        public delegate void ConsoleHandler([MarshalAs(UnmanagedType.LPWStr)] string output);

        public delegate void EmbedCallback([MarshalAs(UnmanagedType.LPWStr)] string output, IntPtr tp);

        public delegate void HookInsertHandler(ulong address, [MarshalAs(UnmanagedType.LPWStr)] string output);

        public delegate bool OutputCallback([MarshalAs(UnmanagedType.LPWStr)] string hookCode,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr threadParam,
            [MarshalAs(UnmanagedType.LPWStr)] string output);

        public delegate void ProcessEvent(uint pid);

        public delegate void ThreadEvent([MarshalAs(UnmanagedType.LPWStr)] string hookCode,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr threadParam);

        private const string LUNA_HOST_DLL64 = "LunaHook/LunaHost64.dll";
        private const string LUNA_HOST_DLL32 = "LunaHook/LunaHost32.dll";

        [DllImport(LUNA_HOST_DLL64, EntryPoint = "Luna_Start"
            , CallingConvention = CallingConvention.Cdecl
            , CharSet = CharSet.Unicode)]
        public static extern void LunaStart64(
            ProcessEvent connect,
            ProcessEvent disconnect,
            ThreadEvent create,
            ThreadEvent destroy,
            OutputCallback output,
            ConsoleHandler console,
            HookInsertHandler hookInsert,
            EmbedCallback embed
        );

        [DllImport(LUNA_HOST_DLL32, EntryPoint = "Luna_Start", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LunaStart32(
            ProcessEvent connect,
            ProcessEvent disconnect,
            ThreadEvent create,
            ThreadEvent destroy,
            OutputCallback output,
            ConsoleHandler console,
            HookInsertHandler hookInsert,
            EmbedCallback embed
        );

        [DllImport(LUNA_HOST_DLL64, EntryPoint = "Luna_Inject", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LunaInject64(uint pid, [MarshalAs(UnmanagedType.LPWStr)] string basePath);

        [DllImport(LUNA_HOST_DLL32, EntryPoint = "Luna_Inject", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LunaInject32(uint pid, [MarshalAs(UnmanagedType.LPWStr)] string basePath);

        [DllImport(LUNA_HOST_DLL64, EntryPoint = "Luna_Detach", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LunaDetach64(uint pid);

        [DllImport(LUNA_HOST_DLL32, EntryPoint = "Luna_Detach", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LunaDetach32(uint pid);

        [DllImport(LUNA_HOST_DLL64, EntryPoint = "Luna_Settings", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LunaSettings64(int flushDelay, bool filterRepetition, int defaultCodepage,
            int maxBufferSize, int maxHistorySize);

        [DllImport(LUNA_HOST_DLL32, EntryPoint = "Luna_Settings", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LunaSettings32(int flushDelay, bool filterRepetition, int defaultCodepage,
            int maxBufferSize, int maxHistorySize);
    }
}