namespace VNGTTranslator.Hooker
{
    public class ReceivedHookData
    {
        public string DisplayHookCode => $"{HookFunc} {HookCode} {Ctx}:{Ctx2}";

        public string HookCode { set; get; } = string.Empty;

        public string HookFunc { set; get; } = string.Empty;

        public string Data { set; get; } = string.Empty;

        public ulong Ctx { get; set; }

        public ulong Ctx2 { get; set; }
    }
}