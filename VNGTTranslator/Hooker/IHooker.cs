namespace VNGTTranslator.Hooker
{
    public interface IHooker
    {
        delegate Task HookTextReceivedEventHandler(HookTextReceivedEventArgs e);

        public event HookTextReceivedEventHandler OnHookTextReceived;

        void Start();

        void SetHookSetting(HookerSetting setting);

        void Inject(uint pid, string basePath);

        void Detach(uint pid);

        bool InsertHook(uint pid, string hookCode);
    }
}