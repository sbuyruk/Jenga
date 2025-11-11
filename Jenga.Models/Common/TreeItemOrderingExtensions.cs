namespace Jenga.Models.Common
{
    /// <summary>
    /// Ordering extensions for TreeItem lists.
    /// Provides a generic OrderByKey that accepts a typed key selector,
    /// falls back to Comparer.Default and supports ascending/descending.
    /// </summary>
    public static class TreeItemOrderingExtensions
    {
        public static IOrderedEnumerable<TreeItem<T>> OrderByKey<T, TKey>(this IEnumerable<TreeItem<T>> source, Func<TreeItem<T>, TKey?> keySelector, bool ascending)
            where TKey : IComparable?
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            if (ascending)
            {
                return source.OrderBy(x => keySelector(x), Comparer<TKey?>.Default);
            }
            else
            {
                return source.OrderByDescending(x => keySelector(x), Comparer<TKey?>.Default);
            }
        }

        // Backwards-compatible overload used in components where object? selector was provided
        public static IOrderedEnumerable<TreeItem<T>> OrderByKey<T>(this IEnumerable<TreeItem<T>> source, Func<TreeItem<T>, object?> keySelector, bool ascending)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            if (ascending)
                return source.OrderBy(x => keySelector(x), Comparer<object?>.Default);
            else
                return source.OrderByDescending(x => keySelector(x), Comparer<object?>.Default);
        }
    }
}