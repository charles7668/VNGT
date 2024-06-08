namespace VNGTTranslator.Hooker
{
    public class HookTextReceivedEventArgs : EventArgs
    {
        public ulong Ctx { get; set; }
        public ulong Ctx2 { get; set; }
        public string HookCode { get; set; } = string.Empty;
        public string HookFunc { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}