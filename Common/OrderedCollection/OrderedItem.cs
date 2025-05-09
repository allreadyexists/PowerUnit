namespace K2.Common.OrderedCollection;

internal abstract class OrderedItem
{
    private static long _order = -1;

    public long Order { get; internal set; }

    public OrderedItem()
    {
        Order = Interlocked.Increment(ref _order);
    }

    internal void SetOrder(long order)
    {
        Order = order;
    }
}
