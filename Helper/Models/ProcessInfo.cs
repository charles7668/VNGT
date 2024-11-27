namespace Helper.Models
{
    public class ProcessInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string ExecutablePath { get; set; } = string.Empty;
    }
}