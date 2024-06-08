using System.IO;
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
            if (Environment.Is64BitProcess)
                LunaDll.LunaStart64(connect, disconnect, create, destroy, output, console, hookInsert, embed);
            else
                LunaDll.LunaStart32(connect, disconnect, create, destroy, output, console, hookInsert, embed);
        }

        public static void LunaInject(uint pid, string basePath)
        {
            if (Environment.Is64BitProcess)
                LunaDll.LunaInject64(pid, basePath);
            else
                LunaDll.LunaInject32(pid, basePath);
        }

        public static void LunaDetach(uint pid)
        {
            if (Environment.Is64BitProcess)
                LunaDll.LunaDetach64(pid);
            else
                LunaDll.LunaDetach32(pid);
        }

        public static void LunaSettings(int flushDelay, bool filterRepetition, int defaultCodepage,
            int maxBufferSize, int maxHistorySize)
        {
            if (Environment.Is64BitProcess)
                LunaDll.LunaSettings64(flushDelay, filterRepetition, defaultCodepage, maxBufferSize, maxHistorySize);
            else
                LunaDll.LunaSettings32(flushDelay, filterRepetition, defaultCodepage, maxBufferSize, maxHistorySize);
        }

        public static bool LunaInsertHookCode(uint pid, string hookCode)
        {
            if (Environment.Is64BitProcess)
                return LunaDll.LunaInsertHookCode64(pid, hookCode);
            return LunaDll.LunaInsertHookCode32(pid, hookCode);
        }
    }

    public static class LunaDll
    {
        public delegate void ConsoleHandler([MarshalAs(UnmanagedType.LPWStr)] string output);

        public delegate void EmbedCallback([MarshalAs(UnmanagedType.LPWStr)] string output, IntPtr tp);

        public delegate void HookInsertHandler(ulong address, [MarshalAs(UnmanagedType.LPWStr)] string output);

        public delegate bool OutputCallback([MarshalAs(UnmanagedType.LPWStr)] string hookCode,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr threadParam,
            [MarshalAs(UnmanagedType.LPWStr)] string output);

        public delegate void ProcessEvent(uint pid);

        public delegate void ThreadEvent([MarshalAs(UnmanagedType.LPWStr)] string hookCode,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr threadParam);

        private const string LUNA_HOST_DLL64 = "LunaHook/LunaHost64.dll";
        private const string LUNA_HOST_DLL32 = "LunaHook/LunaHost32.dll";

        public static readonly string LunaHookDllPath =
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LunaHook"));

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

        [DllImport(LUNA_HOST_DLL64, EntryPoint = "Luna_InsertHookCode", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LunaInsertHookCode64(uint pid, [MarshalAs(UnmanagedType.LPWStr)] string hookCode);

        [DllImport(LUNA_HOST_DLL32, EntryPoint = "Luna_InsertHookCode", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LunaInsertHookCode32(uint pid, [MarshalAs(UnmanagedType.LPWStr)] string hookCode);

        public struct ThreadParam
        {
            public uint ProcessId;
            public ulong Address;
            public ulong Ctx;
            public ulong Ctx2;
        }
    }
}