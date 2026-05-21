// shared/Contracts/OrderPlaced.cs
public record OrderPlaced
{
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
    public string CustomerId { get; init; }
    public List<OrderItem> Items { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime PlacedAt { get; init; }
}

public record PaymentFailed
{
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; }
}

public record StockReserved
{
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
}

public record StockReleased
{
    public Guid OrderId { get; init; }
}