using System.Collections.Generic;
using System.Linq;

namespace TnieYuPackage.Utils.Registry
{
    public delegate T SelectionStrategy<T>(HashSet<T> items);

    public static class Registry<T> where T : class
    {
        private static readonly HashSet<T> Items = new();

        public static bool TryAdd(T item)
        {
            return item is not null && Items.Add(item);
        }

        public static bool Remove(T item) => Items.Remove(item);
        public static void Clear() => Items.Clear();

        public static T GetFirst() => Items.FirstOrDefault();
        public static T Get(SelectionStrategy<T> strategy) => strategy(Items);

        public static IEnumerable<T> All => Items;
    }
}