using System.Collections.ObjectModel;

namespace K2.Common.OrderedCollection;

internal abstract class OrderedCollection<TKey, TValue> : KeyedCollection<TKey, TValue> where TKey : notnull where TValue : OrderedItem
{
    internal sealed class ComparerImpl : IComparer<OrderedItem>
    {
        int IComparer<OrderedItem>.Compare(OrderedItem? x, OrderedItem? y)
        {
            if (x == null || y == null)
                throw new ArgumentNullException();
            return x.Order.CompareTo(y.Order);
        }
    }

    private static readonly ComparerImpl _comparer = new();

    private int TryFastIndexOf(TValue item)
    {
        if (Items is List<TValue> list)
            return list.BinarySearch(item, _comparer);
        return Items.IndexOf(item);
    }

    public TValue AddOrUpdate(TValue value)
    {
        if (TryGetValue(GetKeyForItem(value), out var result))
        {
            var index = TryFastIndexOf(result);
            if (index >= 0)
            {
                // Order должен остаться от старого элемента - т.к. он уже есть
                value.SetOrder(result.Order);
                SetItem(index, value);
                return value;
            }
        }

        Add(value);
        return value;
    }
}
