namespace GameManager.Models
{
    public interface IStrategy
    {
        public Task ExecuteAsync();
    }
}