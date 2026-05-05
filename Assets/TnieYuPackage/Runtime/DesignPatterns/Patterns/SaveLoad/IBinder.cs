namespace TnieYuPackage.DesignPatterns
{
    /// <summary>
    /// Interface for Object need apply data from <T> data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBinder<in T>
    {
        void BindData(T data);
    }
}