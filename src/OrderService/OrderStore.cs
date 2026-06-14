using Contracts;
using System.Collections.Concurrent;

namespace OrderService;

public class OrderStore
{
    private readonly ConcurrentDictionary<Guid, OrderPlaced> _orders = new();

    public void Save(OrderPlaced order) => _orders[order.OrderId] = order;
    public OrderPlaced? Get(Guid orderId) => _orders.TryGetValue(orderId, out var o) ? o : null;
}
