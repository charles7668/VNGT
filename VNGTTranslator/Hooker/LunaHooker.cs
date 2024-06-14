using System.Runtime.InteropServices;
using VNGTTranslator.LunaHook;

namespace VNGTTranslator.Hooker
{
    public class LunaHooker : IHooker
    {
        public LunaHooker()
        {
            _outputCallBack = OutputCallBack;
        }

        public LunaHooker(HookerSetting setting) : this()
        {
            _currentSetting = setting;
        }

        private readonly HookerSetting _currentSetting = new();

        private readonly LunaDll.OutputCallback _outputCallBack;

        private readonly object _outputCallBackLock = new();

        public event IHooker.HookTextReceivedEventHandler? OnHookTextReceived;

        public void Start()
        {
            HookMethod.LunaStart(_ =>
            {
            }, _ =>
            {
            }, (_, _, _) =>
            {
            }, (_, _, _) =>
            {
            }, _outputCallBack, _ =>
            {
            }, (_, _) =>
            {
            }, (_, _) =>
            {
            });
            SetHookSetting(_currentSetting);
        }

        public void SetHookSetting(HookerSetting setting)
        {
            HookMethod.LunaSettings(setting.FlushDelay, setting.FilterRepetition,
                setting.Codepage, setting.MaxBufferSize, setting.MaxHistorySize);
        }

        public void Inject(uint pid, string basePath)
        {
            HookMethod.LunaInject(pid, basePath);
        }

        public void Detach(uint pid)
        {
            HookMethod.LunaDetach(pid);
        }

        public bool InsertHook(uint pid, string hookCode)
        {
            return HookMethod.LunaInsertHookCode(pid, hookCode);
        }

        private bool OutputCallBack(string hookCode, string name, IntPtr tp, string text)
        {
            bool lockTaken = false;
            Monitor.TryEnter(_outputCallBackLock, ref lockTaken);
            if (!lockTaken)
                return false;
            LunaDll.ThreadParam threadParam = Marshal.PtrToStructure<LunaDll.ThreadParam>(tp);
            Task? task = OnHookTextReceived?.Invoke(new HookTextReceivedEventArgs
            {
                Ctx = threadParam.Ctx,
                Ctx2 = threadParam.Ctx2,
                HookCode = hookCode,
                HookFunc = name,
                Text = text
            });
            if (task != null)
                task.Wait();
            Monitor.Exit(_outputCallBackLock);

            return false;
        }
    }
}