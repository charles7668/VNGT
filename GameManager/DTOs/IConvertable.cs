namespace GameManager.DTOs
{
    public interface IConvertable<out T>
    {
        T Convert();
    }
}