namespace PowerUnit;

public static class CollectionMergeHelper
{
    /// <summary>
    /// Merge items from source collection to dest
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="dest"></param>
    /// <param name="src"></param>
    /// <param name="comparer"></param>
    /// <param name="onDelete"></param>
    /// <param name="onInsert"></param>
    /// <param name="onUpdate"></param>
    public static void Merge<T, T2>(
        this ICollection<T> dest, ICollection<T2> src
        , Func<T, T2, bool> comparer
        , Action<ICollection<T>, T> onDelete
        , Action<ICollection<T>, T2> onInsert
        , Action<ICollection<T>, T, T2> onUpdate)
    {
        var toDelete = dest.Where(x => !src.Any(s => comparer(x, s))).ToList();
        foreach (var item in toDelete)
            onDelete(dest, item);

        foreach (var item in src)
        {
            var exist = dest.FirstOrDefault(x => comparer(x, item));
            if (exist != null)
                onUpdate(dest, exist, item);
            else
                onInsert(dest, item);
        }
    }
}
