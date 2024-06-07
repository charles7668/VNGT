using System.Runtime.InteropServices;

namespace VNGTTranslator.LunaHook
{
    public class LunaHook
    {
        public static class LunaDll
        {
            public delegate void ConsoleHandler(string output);

            public delegate void EmbedCallback(string output, IntPtr tp);

            public delegate void HookInsertHandler(ulong address, string output);

            public delegate bool OutputCallback(string hookCode, string name, IntPtr tp, string output);

            public delegate void ProcessEvent(uint pid);

            public delegate void ThreadEvent(string hookCode, string name, IntPtr tp);

            [DllImport("LunaHost64.dll", EntryPoint = "Luna_Start", CallingConvention = CallingConvention.Cdecl)]
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

            [DllImport("LunaHost32.dll", EntryPoint = "Luna_Start", CallingConvention = CallingConvention.Cdecl)]
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

            [DllImport("LunaHost64.dll", EntryPoint = "Luna_Inject", CallingConvention = CallingConvention.Cdecl)]
            public static extern void LunaInject64(uint pid, string basePath);

            [DllImport("LunaHost32.dll", EntryPoint = "Luna_Inject", CallingConvention = CallingConvention.Cdecl)]
            public static extern void LunaInject32(uint pid, string basePath);

            [DllImport("LunaHost64.dll", EntryPoint = "Luna_Detach", CallingConvention = CallingConvention.Cdecl)]
            public static extern void LunaDetach64(uint pid);

            [DllImport("LunaHost32.dll", EntryPoint = "Luna_Detach", CallingConvention = CallingConvention.Cdecl)]
            public static extern void LunaDetach32(uint pid);
        }
    }
}