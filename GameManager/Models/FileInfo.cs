namespace GameManager.Models
{
    public class FileInfo
    {
        public string FileName { get; set; } = string.Empty;

        public DateTime ModifiedTime { get; set; }
        
        public int Size { get; set; }
    }
}