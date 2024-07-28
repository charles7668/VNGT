namespace GameManager.Extractor
{
    public class ExtractOption
    {
        public string Password { get; set; } = string.Empty;

        public string TargetPath { get; set; } = string.Empty;

        public bool CreateNewFolder { get; set; } = false;
    }
}