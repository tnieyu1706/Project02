namespace TnieYuPackage.DesignPatterns
{
    public interface IMyBuilder<out T>
    {
        T Build();
    }
}