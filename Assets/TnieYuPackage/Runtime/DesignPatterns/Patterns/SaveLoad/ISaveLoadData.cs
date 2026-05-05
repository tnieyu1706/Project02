namespace TnieYuPackage.DesignPatterns
{
    public interface ISaveLoadData<T> : IBinder<T>
    {
        T SaveData();
    }
}