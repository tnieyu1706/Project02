using System.Collections.Generic;

namespace TnieYuPackage.GlobalExtensions
{
    public delegate T SelectionStrategy<T>(IEnumerable<T> collection);

    public static class IEnumerableExtensions
    {
        public static T Get<T>(this IEnumerable<T> collection, SelectionStrategy<T> strategy) => strategy(collection);
    }
}