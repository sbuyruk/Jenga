namespace Jenga.Models.Common
{
    /// <summary>
    /// Sorts tree items by the selected key in ascending or descending order.
    /// </summary>
    public static class TreeItemOrderingExtensions
    {
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
