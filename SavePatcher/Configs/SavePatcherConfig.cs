namespace SavePatcher.Configs
{
    public class SavePatcherConfig
    {
        /// <summary>
        /// config name
        /// </summary>
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>
        /// save file path , can use http or local file path
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// if file is zip , password to extract zip
        /// </summary>
        public string ZipPassword { get; set; } = string.Empty;

        /// <summary>
        /// file list to specify which files in zip to patch
        /// </summary>
        public string[] PatchFiles { get; set; } = [];

        /// <summary>
        /// destination path , if the destination path is left empty, it will be selected manually
        /// </summary>
        public string DestinationPath { get; set; } = string.Empty;
    }
}