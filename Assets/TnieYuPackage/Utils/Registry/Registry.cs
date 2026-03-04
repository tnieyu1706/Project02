using System.Collections.Generic;
using System.Linq;
using TnieYuPackage.GlobalExtensions;

namespace TnieYuPackage.Utils
{
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