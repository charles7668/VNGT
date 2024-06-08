namespace VNGTTranslator.Hooker
{
    public struct HookerSetting
    {
        public HookerSetting()
        {
        }

        public int FlushDelay { get; set; } = 100;
        public bool FilterRepetition { get; set; } = false;
        public int Codepage { get; set; } = 932;
        public int MaxBufferSize { get; set; } = 30000;
        public int MaxHistorySize { get; set; } = 10000000;
    }
}