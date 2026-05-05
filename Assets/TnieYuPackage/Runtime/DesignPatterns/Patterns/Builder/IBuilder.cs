namespace TnieYuPackage.DesignPatterns
{
    public interface IBuilder<out T>
    {
        T Build();
    }
}