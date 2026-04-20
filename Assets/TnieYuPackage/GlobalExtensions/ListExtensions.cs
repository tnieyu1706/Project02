using System;
using System.Collections.Generic;

namespace TnieYuPackage.GlobalExtensions
{
    public static class ListExtensions
    {
        public static void RemoveIf<T>(this IList<T> list, Predicate<T> predicate)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate.Invoke(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static void RemoveIfAction<T>(this IList<T> list, Predicate<T> predicate, Action<IList<T>, int> onRemove)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate.Invoke(list[i]))
                {
                    onRemove.Invoke(list, i);
                }
            }
        }
    }
}